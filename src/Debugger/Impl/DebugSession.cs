// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.R.Host.Client;
using Newtonsoft.Json.Linq;
using static System.FormattableString;

namespace Microsoft.R.Debugger {
    /// <summary>
    /// An R debug session for a specific <see cref="IRSession"/>. Provides functionality to:
    /// <list type="bullet">
    /// <item>
    /// Evaluate R expressions and reflect over the resulting values, providing type information, children retrieval for containers etc.
    /// </item>
    /// <item>Step through R code.</item>
    /// <item>Create breakpoints in R code, and be notified when they are hit.</item>
    /// </list>
    /// </summary>
    /// <remarks>
    /// <see cref="DebugSession"/> handles certain <see cref="IRSession"/> events in a way that is non-cooperative with other instances.
    /// Thus, there should never be more than one <see cref="DebugSession"/> instance for a given <see cref="IRSession"/>. To ensure this
    /// in an environment where the session is shared, use <see cref="IDebugSessionProvider"/>.
    /// </remarks>
    public sealed class DebugSession : IDisposable {
        private Task _initializeTask;
        private readonly object _initializeLock = new object();

        private CancellationTokenSource _initialPromptCts = new CancellationTokenSource();
        private TaskCompletionSource<bool> _stepTcs;
        private DebugStackFrame _bpHitFrame;
        private volatile EventHandler<DebugBrowseEventArgs> _browse;
        private volatile DebugBrowseEventArgs _currentBrowseEventArgs;
        private readonly object _browseLock = new object();
        private Dictionary<DebugSourceLocation, DebugBreakpoint> _breakpoints = new Dictionary<DebugSourceLocation, DebugBreakpoint>();

        public IReadOnlyCollection<DebugBreakpoint> Breakpoints => _breakpoints.Values;

        public IRSession RSession { get; private set; }

        /// <summary>
        /// Raised when the associated R session is stopped at the Browse> prompt.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If a handler is subscribed when the session is already at a Browse> prompt, that handler will be
        /// invoked immediately.
        /// </para>
        /// <para>
        /// If a stepping operation is in progress that requires issuing several consecutive commands, the event is not
        /// raised for any intermediate Browse> prompts, but only for the final prompt at which the step is complete.
        /// </para>
        /// </remarks>
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
            if (session == null) {
                throw new ArgumentNullException(nameof(session));
            }

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

        /// <summary>
        /// Initializes the debug session, enabling all other operations on it.
        /// </summary>
        /// <remarks>
        /// All operations that require initialization will automatically perform it if it hasn't been performed
        /// already, so calling this method is never a requirement. However, since initialization can be potentially
        /// costly, calling it in advance at a more opportune moment can be preferable to lazy initialization.
        /// </remarks>
        public Task InitializeAsync(CancellationToken cancellationToken = default(CancellationToken)) {
            lock (_initializeLock) {
                if (_initializeTask == null) {
                    _initializeTask = InitializeWorkerAsync(cancellationToken);
                }
                return _initializeTask;
            }
        }

        private async Task InitializeWorkerAsync(CancellationToken cancellationToken = default(CancellationToken)) {
            ThrowIfDisposed();
            await TaskUtilities.SwitchToBackgroundThread();
            try {
                // Re-initialize the breakpoint table.
                foreach (var bp in _breakpoints.Values) {
                    await bp.ReapplyBreakpointAsync(cancellationToken);
                }
                await RSession.ExecuteAsync("rtvs:::reapply_breakpoints()", REvaluationKind.Mutating); // TODO: mark all breakpoints as invalid if this fails.

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

        /// <summary>
        /// Waits for the next REPL prompt, and executes the given command at it if it is a Browse> prompt.
        /// </summary>
        /// <param name="command">Command to execute. The trailing newline is appended automatically.</param>
        /// <param name="prepare">
        /// If not <see langword="null"/>, the provided delegate is invoked after getting exclusive access
        /// to the prompt, but before command is executed. Can be used to perform preparatory evaluations.</param>
        /// <returns>
        /// <see langword="true"/> if command was successfully submitted for execution.
        /// <see langword="false"/> if the next prompt was not a Browse> prompt.
        /// </returns>
        public async Task<bool> ExecuteBrowserCommandAsync(string command, Func<IRSessionInteraction, Task<bool>> prepare = null, CancellationToken cancellationToken = default(CancellationToken)) {
            ThrowIfDisposed();
            if (command == null) {
                throw new ArgumentNullException(nameof(command));
            }

            await TaskUtilities.SwitchToBackgroundThread();

            using (var inter = await RSession.BeginInteractionAsync(isVisible: true, cancellationToken: cancellationToken)) {
                if (prepare != null) {
                    if (!await prepare(inter)) {
                        return false;
                    }
                }

                if (inter.Contexts.IsBrowser()) {
                    await inter.RespondAsync(command + "\n");
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Like <see cref="EvaluateAsync(string, DebugEvaluationResultFields, int?, CancellationToken)"/>, with no limit on
        /// representation length.
        /// </summary>
        public Task<DebugEvaluationResult> EvaluateAsync(
            string expression,
            DebugEvaluationResultFields fields,
            CancellationToken cancellationToken = default(CancellationToken)
        ) =>
            EvaluateAsync(expression, fields, null, cancellationToken);

        /// <summary>
        /// Like <see cref="EvaluateAsync(string, string, string, DebugEvaluationResultFields, int?, CancellationToken)"/>,
        /// but evaluates in the global environment (<c>.GlobalEnv</c>), and the result is not named.
        /// </summary>
        public Task<DebugEvaluationResult> EvaluateAsync(
            string expression,
            DebugEvaluationResultFields fields,
            int? reprMaxLength,
            CancellationToken cancellationToken = default(CancellationToken)
        ) =>
            EvaluateAsync("base::.GlobalEnv", expression, null, fields, reprMaxLength, cancellationToken);

        /// <summary>
        /// Evaluates an R expresion in the specified environment, and returns an object describing the result.
        /// </summary>
        /// <param name="environmentExpression">
        /// R expression designating the environment in which <paramref name="expression"/> will be evaluated.
        /// </param>
        /// <param name="expression">Expression to evaluate.</param>
        /// <param name="name"><see cref="DebugEvaluationResult.Name"/> of the returned evaluation result.</param>
        /// <param name="fields">Specifies which <see cref="DebugEvaluationResult"/> properties should be present in the result.</param>
        /// <param name="reprMaxLength">
        /// If not <see langword="null"/>, trims representation (as returned by <see cref="DebugValueEvaluationResult.GetRepresentation"/>)
        /// of the resulting value to the specified length.
        /// </param>
        /// <remarks>
        /// If expression fails to evaluate, this method does <em>not</em> raise <see cref="RException"/>. Instead, an instance
        /// of <see cref="DebugErrorEvaluationResult"/> describing the error is returned.
        /// </remarks>
        public async Task<DebugEvaluationResult> EvaluateAsync(
            string environmentExpression,
            string expression,
            string name,
            DebugEvaluationResultFields fields,
            int? reprMaxLength = null,
            CancellationToken cancellationToken = default(CancellationToken)
        ) {
            ThrowIfDisposed();
            if (environmentExpression == null) {
                throw new ArgumentNullException(nameof(environmentExpression));
            }
            if (expression == null) {
                throw new ArgumentNullException(nameof(expression));
            }

            await TaskUtilities.SwitchToBackgroundThread();
            await InitializeAsync(cancellationToken);

            environmentExpression = environmentExpression ?? "NULL";
            var code = Invariant($"rtvs:::eval_and_describe({expression.ToRStringLiteral()}, ({environmentExpression}),, {fields.ToRVector()},, {reprMaxLength})");
            var result = await RSession.EvaluateAsync<JObject>(code, REvaluationKind.Json, cancellationToken);
            return DebugEvaluationResult.Parse(this, environmentExpression, name, result);
        }

        /// <summary>
        /// Force the R session to pause wherever it is currently executing, with a Browse> prompt.
        /// </summary>
        /// <returns>A task that completes when the prompt appears.</returns>
        public async Task BreakAsync(CancellationToken cancellationToken = default(CancellationToken)) {
            await TaskUtilities.SwitchToBackgroundThread();

            // Evaluation will not end until after Browse> is responded to, but this method must indicate completion
            // as soon as the prompt appears. So don't wait for this, but wait for the prompt instead.
            RSession.ExecuteAsync("browser()", REvaluationKind.Reentrant, cancellationToken)
                .SilenceException<MessageTransportException>().DoNotWait();

            // Wait until prompt appears, but don't actually respond to it.
            using (var inter = await RSession.BeginInteractionAsync(true, cancellationToken)) { }
        }

        /// <summary>
        /// When paused at a Browse> prompt, continue execution.
        /// </summary>
        public async Task ContinueAsync(CancellationToken cancellationToken = default(CancellationToken)) {
            await TaskUtilities.SwitchToBackgroundThread();
            ExecuteBrowserCommandAsync("c", null, cancellationToken)
                .SilenceException<MessageTransportException>()
                .DoNotWait();
        }

        /// <summary>
        /// When paused at a Browse> prompt, step into the next call.
        /// </summary>
        /// <returns>
        /// A task that completes when the step is either completed or interrupted (e.g. by a breakpoint).
        /// The result is <see langword="true"/> if step was completed, and <see langword="false"/> if it was abandoned.
        /// </returns>
        /// <remarks>
        /// Detailed semantics of step in are described in R documentation for
        /// <a href="https://stat.ethz.ch/R-manual/R-devel/library/base/html/browser.html"><c>browser()</c></a> "s" command.
        /// </remarks>
        public Task<bool> StepIntoAsync(CancellationToken cancellationToken = default(CancellationToken)) {
            return StepAsync(cancellationToken, "s");
        }

        /// <summary>
        /// When paused at a Browse> prompt, step over the next call.
        /// </summary>
        /// <returns>
        /// A task that completes when the step is either completed or interrupted (e.g. by a breakpoint).
        /// The result is <see langword="true"/> if step was completed, and <see langword="false"/> if it was abandoned.
        /// </returns>
        /// <remarks>
        /// Detailed semantics of step over are described in R documentation for
        /// <a href="https://stat.ethz.ch/R-manual/R-devel/library/base/html/browser.html"><c>browser()</c></a> "n" command.
        /// </remarks>
        public Task<bool> StepOverAsync(CancellationToken cancellationToken = default(CancellationToken)) {
            return StepAsync(cancellationToken, "n");
        }

        /// <summary>
        /// When paused at a Browse> prompt, step out from the current call.
        /// </summary>
        /// <returns>
        /// A task that completes when the step is either completed or interrupted (e.g. by a breakpoint).
        /// The result is <see langword="true"/> if step was completed, and <see langword="false"/> if it was abandoned.
        /// </returns>
        /// Detailed semantics of step out are described in R documentation for
        /// <a href="https://stat.ethz.ch/R-manual/R-devel/library/base/html/browserText.html"><c>browserSetDebug()</c></a> function.
        /// </remarks>
        public Task<bool> StepOutAsync(CancellationToken cancellationToken = default(CancellationToken)) {
            return StepAsync(cancellationToken, "c", async inter => {
                using (var eval = await RSession.BeginEvaluationAsync(cancellationToken)) {
                    try {
                        // EvaluateAsync will push a new toplevel context on the context stack before
                        // evaluating the expression, so tell browser_set_debug to skip 1 toplevel context
                        // before locating the target context for step-out.
                        await eval.ExecuteAsync("rtvs:::browser_set_debug(1, 1)", REvaluationKind.Normal);
                    } catch (RException) {
                        _stepTcs.TrySetResult(false);
                        return false;
                    }
                    return true;
                }
            });
        }

        /// <returns>
        /// <c>true</c> if step completed successfully, and <c>false</c> if it was interrupted midway by something
        /// else pausing the process, such as a breakpoint.
        /// </returns>
        private async Task<bool> StepAsync(CancellationToken cancellationToken, string command, Func<IRSessionInteraction, Task<bool>> prepare = null) {
            ThrowIfDisposed();
            await TaskUtilities.SwitchToBackgroundThread();

            _stepTcs = new TaskCompletionSource<bool>();
            ExecuteBrowserCommandAsync(command, prepare, cancellationToken)
                .SilenceException<MessageTransportException>()
                .DoNotWait();
            return await _stepTcs.Task;
        }

        /// <summary>
        /// If a step operation is currently in progress, cancel it.
        /// </summary>
        public bool CancelStep() {
            ThrowIfDisposed();

            if (_stepTcs == null) {
                return false;
            }

            _stepTcs.TrySetCanceled();
            _stepTcs = null;
            return true;
        }

        /// <summary>
        /// Retrieve the current call stack, in call order (i.e. the current active frame is last, the one that called it is second to last etc).
        /// </summary>
        /// <param name="skipSourceFrames">
        /// If <see langword="true"/>, excludes frames that belong to <c>source()</c> or <c>rtvs:::debug_source()</c> internal machinery at the bottom of the stack;
        /// the first reported frame will be the one with sourced code.
        /// </param>
        /// <remarks>
        /// This method has snapshot semantics for the frames and their properties - that is, the returned collection is not going to change as code runs.
        /// However, calling various methods on the returned <see cref="DebugStackFrame"/> objects, such as <see cref="DebugStackFrame.GetEnvironmentAsync"/>,
        /// will fetch fresh data, possibly from altogether different frames if the call stack has changed. Thus, it is inadvisable to retain the returned
        /// stack and use it at a later point - it should always be obtained anew at the point where it is used. 
        /// </remarks>
        public async Task<IReadOnlyList<DebugStackFrame>> GetStackFramesAsync(bool skipSourceFrames = true, CancellationToken cancellationToken = default(CancellationToken)) {
            ThrowIfDisposed();

            await TaskUtilities.SwitchToBackgroundThread();
            await InitializeAsync(cancellationToken);

            var jFrames = await RSession.EvaluateAsync<JArray>("rtvs:::describe_traceback()", REvaluationKind.Json, cancellationToken);
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

            if (skipSourceFrames) {
                var firstFrame = stackFrames.FirstOrDefault();
                if (firstFrame != null && firstFrame.IsGlobal && firstFrame.Call != null) {
                    if (firstFrame.Call.StartsWith("source(") || firstFrame.Call.StartsWith("rtvs::debug_source(")) {
                        // Skip everything until the first frame that has a line number - that will be the sourced code.
                        stackFrames = stackFrames.SkipWhile(f => f.LineNumber == null).ToList();
                    }
                }
            }

            return stackFrames;
        }

        /// <summary>
        /// Enables or disables breakpoints. When enabled, <see cref="DebugBreakpoint.BreakpointHit"/> events are raised.
        /// </summary>
        public async Task EnableBreakpointsAsync(bool enable, CancellationToken cancellationToken = default(CancellationToken)) {
            ThrowIfDisposed();
            await TaskUtilities.SwitchToBackgroundThread();
            await RSession.ExecuteAsync($"rtvs:::enable_breakpoints({(enable ? "TRUE" : "FALSE")})", REvaluationKind.Mutating);
        }

        /// <summary>
        /// Creates a new breakpoint at the specified location.
        /// </summary>
        public async Task<DebugBreakpoint> CreateBreakpointAsync(DebugSourceLocation location, CancellationToken cancellationToken = default(CancellationToken)) {
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

        private void InterruptBreakpointHitProcessing() {
            _bpHitFrame = null;
        }

        private void ProcessBrowsePrompt(IReadOnlyList<IRContext> contexts) {
            if (!contexts.IsBrowser()) {
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
                    var location = new DebugSourceLocation(lastFrame.FileName, lastFrame.LineNumber.Value);
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

            var eventArgs = new DebugBrowseEventArgs(inter, isStepCompleted, breakpointsHit);
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
}
