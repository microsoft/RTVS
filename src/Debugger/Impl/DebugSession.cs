using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
        // State machine that processes breakpoints hit. We need to issue several commands in response
        // in succession; this keeps track of where we were on the last interaction.
        private enum BreakpointHitProcessingState {
            None,
            BrowserSetDebug,
            Continue
        }

        private Task _initializeTask;
        private readonly object _initializeLock = new object();

        private CancellationTokenSource _initialPromptCts = new CancellationTokenSource();
        private TaskCompletionSource<object> _stepTcs;
        private BreakpointHitProcessingState _bpHitProcState;
        private DebugStackFrame _bpHitFrame;
        private volatile EventHandler<DebugBrowseEventArgs> _browse;
        private readonly object _browseLock = new object();

        // Key is filename + line number.
        private Dictionary<DebugBreakpointLocation, DebugBreakpoint> _breakpoints = new Dictionary<DebugBreakpointLocation, DebugBreakpoint>();
        public IRSession RSession { get; private set; }
        public bool IsBrowsing { get; private set; }

        public event EventHandler<DebugBrowseEventArgs> Browse {
            add {
                if (IsBrowsing) {
                    value?.Invoke(this, new DebugBrowseEventArgs(false, null));
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

        public Task InitializeAsync() {
            lock (_initializeLock) {
                if (_initializeTask == null) {
                    _initializeTask = InitializeWorkerAsync();
                }
                return _initializeTask;
            }
        }

        private async Task InitializeWorkerAsync() {
            ThrowIfDisposed();

            await TaskUtilities.SwitchToBackgroundThread();

            string helpers;
            using (var stream = typeof(DebugSession).Assembly.GetManifestResourceStream(typeof(DebugSession).Namespace + ".DebugHelpers.R"))
            using (var reader = new StreamReader(stream)) {
                helpers = await reader.ReadToEndAsync();
            }
            helpers = helpers.Replace("\r", "");

            REvaluationResult res;
            using (var eval = await RSession.BeginEvaluationAsync()) {
                res = await eval.EvaluateAsync(helpers);
            }

            if (res.ParseStatus != RParseStatus.OK) {
                throw new InvalidDataException("DebugHelpers.R failed to parse: " + res.ParseStatus);
            } else if (res.Error != null) {
                throw new InvalidDataException("DebugHelpers.R failed to eval: " + res.Error);
            }

            // Attach might happen when session is already at the Browse prompt, in which case we have
            // missed the corresponding BeginRequest event, but we want to raise Browse anyway. So
            // grab an interaction and check the prompt.
            RSession.BeginInteractionAsync().ContinueWith(async t => {
                using (var inter = await t) {
                    // If we got AfterRequest before we got here, then that has already taken care of
                    // the prompt; or if it's not a Browse prompt, will do so in a future event. Bail out.'
                    if (_initialPromptCts.IsCancellationRequested) {
                        return;
                    }

                    // Otherwise, treat it the same as if AfterRequest just happened.
                    ProcessBrowsePrompt(inter.Contexts);
                }
            }).DoNotWait();
        }

        public async Task ExecuteBrowserCommandAsync(string command) {
            ThrowIfDisposed();
            await TaskUtilities.SwitchToBackgroundThread();

            using (var inter = await RSession.BeginInteractionAsync(isVisible: true)) {
                if (IsBrowserContext(inter.Contexts)) {
                    await inter.RespondAsync(command + "\n");
                }
            }
        }

        internal async Task<TToken> InvokeDebugHelperAsync<TToken>(string expression)
            where TToken : JToken {

            TaskUtilities.AssertIsOnBackgroundThread();
            ThrowIfDisposed();

            REvaluationResult res;
            using (var eval = await RSession.BeginEvaluationAsync(false)) {
                res = await eval.EvaluateAsync(expression, REvaluationKind.Json);
                if (res.ParseStatus != RParseStatus.OK || res.Error != null || res.JsonResult == null) {
                    Trace.Fail(Invariant($"Internal debugger evaluation {expression} failed: {res}"));
                    throw new REvaluationException(res);
                }
            }

            var token = res.JsonResult as TToken;
            if (token == null) {
                var err = Invariant($"Expected to receive {typeof(TToken).Name} in response to {expression}, but got {res.JsonResult?.GetType().Name}");
                Trace.Fail(err);
                throw new JsonException(err);
            }

            return token;
        }

        public Task<DebugEvaluationResult> EvaluateAsync(string expression, string name = null, string env = "NULL") {
            return EvaluateAsync(null, expression, name, env);
        }

        public async Task<DebugEvaluationResult> EvaluateAsync(DebugStackFrame stackFrame, string expression, string name = null, string env = "NULL") {
            ThrowIfDisposed();

            await TaskUtilities.SwitchToBackgroundThread();
            await InitializeAsync();

            var jEvalResult = await InvokeDebugHelperAsync<JObject>(Invariant($".rtvs.toJSON(.rtvs.eval({expression.ToRStringLiteral()}, {env}))"));
            return DebugEvaluationResult.Parse(stackFrame, name, jEvalResult);
        }

        public Task StepIntoAsync() {
            return StepAsync("s");
        }

        public Task StepOverAsync() {
            return StepAsync("n");
        }

        public Task StepOutAsync() {
            return StepAsync("browserSetDebug()", "c");
        }

        private async Task StepAsync(params string[] commands) {
            Trace.Assert(commands.Length > 0);
            ThrowIfDisposed();

            await TaskUtilities.SwitchToBackgroundThread();

            _stepTcs = new TaskCompletionSource<object>();
            for (int i = 0; i < commands.Length - 1; ++i) {
                await ExecuteBrowserCommandAsync(commands[i]);
            }

            ExecuteBrowserCommandAsync(commands.Last()).DoNotWait();
            await _stepTcs.Task;
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

        public async Task<IReadOnlyList<DebugStackFrame>> GetStackFramesAsync() {
            ThrowIfDisposed();

            await TaskUtilities.SwitchToBackgroundThread();
            await InitializeAsync();

            var jFrames = await InvokeDebugHelperAsync<JArray>(".rtvs.traceback()");
            Trace.Assert(jFrames.All(t => t is JObject), ".rtvs.traceback(): array of objects expected.\n\n" + jFrames);

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

        public async Task<DebugBreakpoint> CreateBreakpointAsync(DebugBreakpointLocation location) {
            ThrowIfDisposed();

            await TaskUtilities.SwitchToBackgroundThread();
            await InitializeAsync();

            DebugBreakpoint bp;
            if (!_breakpoints.TryGetValue(location, out bp)) {
                bp = new DebugBreakpoint(this, location);
                _breakpoints.Add(location, bp);
            }

            await bp.SetBreakpointAsync();
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
            if (_bpHitProcState != BreakpointHitProcessingState.None) {
                _bpHitProcState = BreakpointHitProcessingState.None;
                // If we were in the middle of processing a breakpoint hit, we need to make sure that
                // any pending stepping is canceled, since we can no longer handle it at this point.
                CancelStep();
            }
        }

        private void ProcessBrowsePrompt(IReadOnlyList<IRContext> contexts) {
            if (!IsBrowserContext(contexts)) {
                InterruptBreakpointHitProcessing();
                return;
            }

            IsBrowsing = true;

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
            }).SilenceException<OperationCanceledException>().DoNotWait();
        }

        private async Task ProcessBrowsePromptWorker(IRSessionInteraction inter) {
            DebugStackFrame lastFrame = null;

            switch (_bpHitProcState) {
                case BreakpointHitProcessingState.None:
                    {
                        lastFrame = (await GetStackFramesAsync()).LastOrDefault();
                        if (lastFrame?.FrameKind == DebugStackFrameKind.TracebackAfterBreakpoint) {
                            // If we're stopped at a breakpoint, step out of .doTrace, so that the next stepping command that
                            // happens is applied at the actual location inside the function where the breakpoint was set.
                            // Determine how many steps out we need to make by counting back to .doTrace. Also stash away the
                            // .doTrace frame - we will need it to correctly report filename and line number after we unwind.
                            int n = 0;
                            _bpHitFrame = lastFrame;
                            for (
                                _bpHitFrame = lastFrame;
                                _bpHitFrame != null && _bpHitFrame.FrameKind != DebugStackFrameKind.DoTrace;
                                _bpHitFrame = _bpHitFrame.CallingFrame
                            ) {
                                ++n;
                            }

                            // Set the destination for the next "c", which we will issue on the following prompt.
                            await inter.RespondAsync(Invariant($"browserSetDebug({n})\n"));
                            _bpHitProcState = BreakpointHitProcessingState.BrowserSetDebug;
                            return;
                        } else {
                            _bpHitFrame = null;
                        }
                        break;
                    }

                case BreakpointHitProcessingState.BrowserSetDebug:
                    // We have issued a browserSetDebug() on the previous interaction prompt to set destination
                    // for the "c" command. Issue that command now, and move to the next step.
                    await inter.RespondAsync("c\n");
                    _bpHitProcState = BreakpointHitProcessingState.Continue;
                    return;

                case BreakpointHitProcessingState.Continue:
                    // We have issued a "c" command on the previous interaction prompt to unwind the stack back
                    // to the function in which the breakpoint was set. We are in the proper context now, and
                    // can report step completion and raise Browse below.
                    _bpHitProcState = BreakpointHitProcessingState.None;
                    break;
            }

            if (lastFrame == null) {
                lastFrame = (await GetStackFramesAsync()).LastOrDefault();
            }

            // Report breakpoints first, so that by the time step completion is reported, all actions associated
            // with breakpoints (e.g. printing messages for tracepoints) have already been completed.
            IReadOnlyCollection<DebugBreakpoint> breakpointsHit = null;
            if (lastFrame.FileName != null && lastFrame.LineNumber != null) {
                var location = new DebugBreakpointLocation(lastFrame.FileName, lastFrame.LineNumber.Value);
                DebugBreakpoint bp;
                if (_breakpoints.TryGetValue(location, out bp)) {
                    bp.RaiseBreakpointHit();
                    breakpointsHit = Enumerable.Repeat(bp, bp.UseCount).ToArray();
                }
            }

            bool isStepCompleted = false;
            if (_stepTcs != null) {
                var stepTcs = _stepTcs;
                _stepTcs = null;
                stepTcs.TrySetResult(null);
                isStepCompleted = true;
            }

            _browse?.Invoke(this, new DebugBrowseEventArgs(isStepCompleted, breakpointsHit));
        }

        private void RSession_Connected(object sender, EventArgs e) {
            lock (_initializeLock) {
                _initializeTask = null;
            }

            InitializeAsync().SilenceException<OperationCanceledException>().DoNotWait();
        }

        private void RSession_BeforeRequest(object sender, RRequestEventArgs e) {
            _initialPromptCts.Cancel();
            ProcessBrowsePrompt(e.Contexts);
        }

        private void RSession_AfterRequest(object sender, RRequestEventArgs e) {
            IsBrowsing = false;
        }
    }

    public class REvaluationException : Exception {
        public REvaluationResult Result { get; }

        public REvaluationException(REvaluationResult result) {
            Result = result;
        }
    }

    public class DebugBrowseEventArgs : EventArgs {
        public bool IsStepCompleted { get; }
        public IReadOnlyCollection<DebugBreakpoint> BreakpointsHit { get; }

        public DebugBrowseEventArgs(bool isStepCompleted, IReadOnlyCollection<DebugBreakpoint> breakpointsHit) {
            IsStepCompleted = isStepCompleted;
            BreakpointsHit = breakpointsHit ?? new DebugBreakpoint[0];
        }
    }
}
