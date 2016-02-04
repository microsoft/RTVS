using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.R.Host.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static System.FormattableString;

namespace Microsoft.R.Debugger {
    public sealed class DebugSession : IDisposable {
        private Task _initializeTask;
        private readonly object _initializeLock = new object();

        private CancellationTokenSource _initialPromptCts = new CancellationTokenSource();
        private TaskCompletionSource<bool> _stepTcs;
        private DebugStackFrame _bpHitFrame;
        private volatile EventHandler<DebugBrowseEventArgs> _browse;
        private volatile DebugBrowseEventArgs _currentBrowseEventArgs;
        private readonly object _browseLock = new object();

        private Dictionary<DebugBreakpointLocation, DebugBreakpoint> _breakpoints = new Dictionary<DebugBreakpointLocation, DebugBreakpoint>();

        public IReadOnlyCollection<DebugBreakpoint> Breakpoints => _breakpoints.Values;

        public IRSession RSession { get; private set; }

        public bool IsBrowsing => _currentBrowseEventArgs != null;

        public event EventHandler<DebugBrowseEventArgs> Browse {
            add {
                var eventArgs = _currentBrowseEventArgs;
                if (eventArgs != null) {
                    value?.Invoke(this, eventArgs);
                }

                lock (_browseLock) {
                    _browse += value;
                }
            }
            remove {
                lock (_browseLock) {
                    _browse -= value;
                }
            }
        }

        public DebugSession(IRSession session) {
            RSession = session;
            RSession.Connected += RSession_Connected;
            RSession.BeforeRequest += RSession_BeforeRequest;
            RSession.AfterRequest += RSession_AfterRequest;
        }

        public void Dispose() {
            RSession.Connected -= RSession_Connected;
            RSession.BeforeRequest -= RSession_BeforeRequest;
            RSession.AfterRequest -= RSession_AfterRequest;
            RSession = null;
        }

        private void ThrowIfDisposed() {
            if (RSession == null) {
                throw new ObjectDisposedException(nameof(DebugSession));
            }
        }

        public Task InitializeAsync(CancellationToken cancellationToken = default(CancellationToken)) {
            lock (_initializeLock) {
                if (_initializeTask == null) {
                    _initializeTask = InitializeWorkerAsync(cancellationToken);
                }

                return _initializeTask;
            }
        }

        private async Task InitializeWorkerAsync(CancellationToken cancellationToken) {
            ThrowIfDisposed();
            await TaskUtilities.SwitchToBackgroundThread();
            try {
                using (var eval = await RSession.BeginEvaluationAsync(cancellationToken: cancellationToken)) {
                    // Re-initialize the breakpoint table.
                    foreach (var bp in _breakpoints.Values) {
                        await eval.EvaluateAsync(bp.GetAddBreakpointExpression(false)); // TODO: mark breakpoint as invalid if this fails.
                    }

                    await eval.EvaluateAsync("rtvs:::reapply_breakpoints()"); // TODO: mark all breakpoints as invalid if this fails.
                }

                // Attach might happen when session is already at the Browse prompt, in which case we have
                // missed the corresponding BeginRequest event, but we want to raise Browse anyway. So
                // grab an interaction and check the prompt.
                RSession.BeginInteractionAsync(cancellationToken: cancellationToken).ContinueWith(async t => {
                    using (var inter = await t) {
                        // If we got AfterRequest before we got here, then that has already taken care of
                        // the prompt; or if it's not a Browse prompt, will do so in a future event. Bail out.'
                        if (_initialPromptCts.IsCancellationRequested) {
                            return;
                        }

                        // Otherwise, treat it the same as if AfterRequest just happened.
                        ProcessBrowsePrompt(inter.Contexts);
                    }
                }, cancellationToken).DoNotWait();
            } catch (Exception ex) when (!ex.IsCriticalException()) {
                Dispose();
                throw;
            }
        }

        public async Task ExecuteBrowserCommandAsync(string command, CancellationToken cancellationToken) {
            ThrowIfDisposed();
            await TaskUtilities.SwitchToBackgroundThread();

            using (var inter = await RSession.BeginInteractionAsync(isVisible: true, cancellationToken: cancellationToken)) {
                if (IsBrowserContext(inter.Contexts)) {
                    await inter.RespondAsync(command + "\n");
                }
            }
        }

        internal async Task<REvaluationResult> InvokeDebugHelperAsync(string expression, CancellationToken cancellationToken, bool json = false) {
            TaskUtilities.AssertIsOnBackgroundThread();
            ThrowIfDisposed();

            REvaluationResult res;
            using (var eval = await RSession.BeginEvaluationAsync(false, cancellationToken)) {
                res = await eval.EvaluateAsync(expression, json ? REvaluationKind.Json : REvaluationKind.Normal);
                if (res.ParseStatus != RParseStatus.OK || res.Error != null || (json && res.JsonResult == null)) {
                    Trace.Fail(Invariant($"Internal debugger evaluation {expression} failed: {res}"));
                    throw new REvaluationException(res);
                }
            }

            return res;
        }

        internal async Task<TToken> InvokeDebugHelperAsync<TToken>(string expression, CancellationToken cancellationToken)
            where TToken : JToken {

            var res = await InvokeDebugHelperAsync(expression, cancellationToken, json: true);

            var token = res.JsonResult as TToken;
            if (token == null) {
                var err = Invariant($"Expected to receive {typeof(TToken).Name} in response to {expression}, but got {res.JsonResult?.GetType().Name}");
                Trace.Fail(err);
                throw new JsonException(err);
            }

            return token;
        }

        public Task<DebugEvaluationResult> EvaluateAsync(string expression, CancellationToken cancellationToken) {
            return EvaluateAsync(null, expression, cancellationToken:cancellationToken);
        }

        public async Task<DebugEvaluationResult> EvaluateAsync(
            DebugStackFrame stackFrame,
            string expression,
            string name = null,
            string env = null,
            DebugEvaluationResultFields fields = DebugEvaluationResultFields.All,
            int? reprMaxLength = null,
            CancellationToken cancellationToken = default (CancellationToken)
        ) {
            ThrowIfDisposed();

            await TaskUtilities.SwitchToBackgroundThread();
            await InitializeAsync(cancellationToken);

            env = env ?? stackFrame?.SysFrame ?? "NULL";
            var code = Invariant($"rtvs:::toJSON(rtvs:::eval_and_describe({expression.ToRStringLiteral()}, {env},, {fields.ToRVector()},, {reprMaxLength}))");
            var jEvalResult = await InvokeDebugHelperAsync<JObject>(code, cancellationToken);
            return DebugEvaluationResult.Parse(stackFrame, name, jEvalResult);
        }

        public async Task Break(CancellationToken ct = default(CancellationToken)) {
            await TaskUtilities.SwitchToBackgroundThread();
            using (var inter = await RSession.BeginInteractionAsync(true, ct)) {
                await inter.RespondAsync("browser()\n");
            }
        }

        public async Task Continue(CancellationToken cancellationToken = default(CancellationToken)) {
            await TaskUtilities.SwitchToBackgroundThread();
            ExecuteBrowserCommandAsync("c", cancellationToken)
                .SilenceException<MessageTransportException>()
                .SilenceException<RException>()
                .DoNotWait();
        }

        public Task<bool> StepIntoAsync(CancellationToken cancellationToken = default(CancellationToken)) {
            return StepAsync(cancellationToken, "s");
        }

        public Task<bool> StepOverAsync(CancellationToken cancellationToken = default(CancellationToken)) {
            return StepAsync(cancellationToken, "n");
        }

        public Task<bool> StepOutAsync(CancellationToken cancellationToken = default(CancellationToken)) {
            return StepAsync(cancellationToken, "rtvs:::browser_set_debug()", "c");
        }

        /// <returns>
        /// <c>true</c> if step completed successfully, and <c>false</c> if it was interrupted midway by something
        /// else pausing the process, such as a breakpoint.
        /// </returns>
        private async Task<bool> StepAsync(CancellationToken cancellationToken, params string[] commands) {
            Trace.Assert(commands.Length > 0);
            ThrowIfDisposed();

            await TaskUtilities.SwitchToBackgroundThread();

            _stepTcs = new TaskCompletionSource<bool>();
            for (int i = 0; i < commands.Length - 1; ++i) {
                await ExecuteBrowserCommandAsync(commands[i], cancellationToken);
            }

            // If RException happens, it means that the expression we just stepped over caused an error.
            // The step is still considered successful and complete in that case, so we just ignore it.
            ExecuteBrowserCommandAsync(commands.Last(), cancellationToken)
                .SilenceException<MessageTransportException>()
                .SilenceException<RException>()
                .DoNotWait();

            return await _stepTcs.Task;
        }

        public bool CancelStep() {
            ThrowIfDisposed();

            if (_stepTcs == null) {
                return false;
            }

            _stepTcs.TrySetCanceled();
            _stepTcs = null;
            return true;
        }

        public async Task<IReadOnlyList<DebugStackFrame>> GetStackFramesAsync(CancellationToken cancellationToken = default(CancellationToken)) {
            ThrowIfDisposed();

            await TaskUtilities.SwitchToBackgroundThread();
            await InitializeAsync(cancellationToken);

            var jFrames = await InvokeDebugHelperAsync<JArray>("rtvs:::describe_traceback()", cancellationToken);
            Trace.Assert(jFrames.All(t => t is JObject), "rtvs:::describe_traceback(): array of objects expected.\n\n" + jFrames);

            var stackFrames = new List<DebugStackFrame>();

            DebugStackFrame lastFrame = null;
            int i = 0;
            foreach (JObject jFrame in jFrames) {
                var fallbackFrame = (_bpHitFrame != null && _bpHitFrame.Index == i) ? _bpHitFrame : null;
                lastFrame = new DebugStackFrame(this, i, lastFrame, jFrame, fallbackFrame);
                stackFrames.Add(lastFrame);
                ++i;
            }

            return stackFrames;
        }

        public async Task EnableBreakpointsAsync(bool enable, CancellationToken ct = default(CancellationToken)) {
            ThrowIfDisposed();
            await TaskUtilities.SwitchToBackgroundThread();
            await InvokeDebugHelperAsync(Invariant($"rtvs:::enable_breakpoints({(enable ? "TRUE" : "FALSE")})"), ct);
        }

        public async Task<DebugBreakpoint> CreateBreakpointAsync(DebugBreakpointLocation location, CancellationToken cancellationToken = default(CancellationToken)) {
            ThrowIfDisposed();

            await TaskUtilities.SwitchToBackgroundThread();
            await InitializeAsync(cancellationToken);

            DebugBreakpoint bp;
            if (!_breakpoints.TryGetValue(location, out bp)) {
                bp = new DebugBreakpoint(this, location);
                _breakpoints.Add(location, bp);
            }

            await bp.SetBreakpointAsync(cancellationToken);
            return bp;
        }

        internal void RemoveBreakpoint(DebugBreakpoint breakpoint) {
            Trace.Assert(breakpoint.Session == this);
            _breakpoints.Remove(breakpoint.Location);
        }

        private bool IsBrowserContext(IReadOnlyList<IRContext> contexts) {
            return contexts.SkipWhile(context => context.CallFlag.HasFlag(RContextType.Restart))
                .FirstOrDefault()?.CallFlag.HasFlag(RContextType.Browser) == true;
        }

        private void InterruptBreakpointHitProcessing() {
            _bpHitFrame = null;
        }

        private void ProcessBrowsePrompt(IReadOnlyList<IRContext> contexts) {
            if (!IsBrowserContext(contexts)) {
                InterruptBreakpointHitProcessing();
                return;
            }

            RSession.BeginInteractionAsync().ContinueWith(async t => {
                using (var inter = await t) {
                    if (inter.Contexts != contexts) {
                        // Someone else has already responded to this interaction.
                        InterruptBreakpointHitProcessing();
                        return;
                    } else {
                        await ProcessBrowsePromptWorker(inter);
                    }
                }
            }).DoNotWait();
        }

        private async Task ProcessBrowsePromptWorker(IRSessionInteraction inter) {
            var frames = await GetStackFramesAsync();

            // If there's .doTrace(rtvs:::breakpoint) anywhere on the stack, we're inside the internal machinery
            // that triggered Browse> prompt when hitting a breakpoint. We need to step out of it until we're
            // back at the frame where the breakpoint was actually set, so that those internal frames do not show
            // on the call stack, and further stepping does not try to step through them. 
            // Since browserSetDebug-based step out is not reliable in the presence of loops, we'll just keep
            // stepping over with "n" until we're all the way out. Every step will trigger a new prompt, and
            // we will come back to this method again.
            var doTraceFrame = frames.FirstOrDefault(frame => frame.FrameKind == DebugStackFrameKind.DoTrace);
            if (doTraceFrame != null) {
                // Save the .doTrace frame so that we can report file / line number info correctly later, once we're fully stepped out.
                // TODO: remove this hack when injected breakpoints get proper source info (#570).
                _bpHitFrame = doTraceFrame;

                await inter.RespondAsync(Invariant($"n\n"));
                return;
            }

            IReadOnlyCollection<DebugBreakpoint> breakpointsHit = null;
            var lastFrame = frames.LastOrDefault();
            if (lastFrame != null) {
                // Report breakpoints first, so that by the time step completion is reported, all actions associated
                // with breakpoints (e.g. printing messages for tracepoints) have already been completed.
                if (lastFrame.FileName != null && lastFrame.LineNumber != null) {
                    var location = new DebugBreakpointLocation(lastFrame.FileName, lastFrame.LineNumber.Value);
                    DebugBreakpoint bp;
                    if (_breakpoints.TryGetValue(location, out bp)) {
                        bp.RaiseBreakpointHit();
                        breakpointsHit = Enumerable.Repeat(bp, bp.UseCount).ToArray();
                    }
                }
            }

            bool isStepCompleted = false;
            if (_stepTcs != null) {
                var stepTcs = _stepTcs;
                _stepTcs = null;
                stepTcs.TrySetResult(breakpointsHit == null || breakpointsHit.Count == 0);
                isStepCompleted = true;
            }

            EventHandler<DebugBrowseEventArgs> browse;
            lock (_browseLock) {
                browse = _browse;
            }

            var eventArgs = new DebugBrowseEventArgs(inter.Contexts, isStepCompleted, breakpointsHit);
            _currentBrowseEventArgs = eventArgs;
            browse?.Invoke(this, eventArgs);
        }

        private void RSession_Connected(object sender, EventArgs e) {
            lock (_initializeLock) {
                _initializeTask = null;
            }

            InitializeAsync().DoNotWait();
        }

        private void RSession_BeforeRequest(object sender, RRequestEventArgs e) {
            _initialPromptCts.Cancel();
            ProcessBrowsePrompt(e.Contexts);
        }

        private void RSession_AfterRequest(object sender, RRequestEventArgs e) {
            _currentBrowseEventArgs = null;
        }
    }

    public class REvaluationException : Exception {
        public REvaluationResult Result { get; }

        public REvaluationException(REvaluationResult result) {
            Result = result;
        }
    }

    public class DebugBrowseEventArgs : EventArgs {
        public IReadOnlyList<IRContext> Contexts { get; }
        public bool IsStepCompleted { get; }
        public IReadOnlyCollection<DebugBreakpoint> BreakpointsHit { get; }

        public DebugBrowseEventArgs(IReadOnlyList<IRContext> contexts, bool isStepCompleted, IReadOnlyCollection<DebugBreakpoint> breakpointsHit) {
            Contexts = contexts;
            IsStepCompleted = isStepCompleted;
            BreakpointsHit = breakpointsHit ?? new DebugBreakpoint[0];
        }
    }
}
