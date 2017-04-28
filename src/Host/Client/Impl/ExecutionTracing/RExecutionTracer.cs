// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Diagnostics;
using Microsoft.R.Host.Client;
using Microsoft.R.StackTracing;
using static System.FormattableString;

namespace Microsoft.R.ExecutionTracing {
    internal sealed class RExecutionTracer : IRExecutionTracer {
        private Task _initializeTask;
        private readonly object _initializeLock = new object();

        private CancellationTokenSource _initialPromptCts = new CancellationTokenSource();
        private TaskCompletionSource<bool> _stepTcs;
        private volatile EventHandler<RBrowseEventArgs> _browse;
        private volatile RBrowseEventArgs _currentBrowseEventArgs;
        private readonly object _browseLock = new object();
        private Dictionary<RSourceLocation, RBreakpoint> _breakpoints = new Dictionary<RSourceLocation, RBreakpoint>();

        public IReadOnlyCollection<IRBreakpoint> Breakpoints => _breakpoints.Values;

        public IRSession Session { get; private set; }

        public event EventHandler<RBrowseEventArgs> Browse {
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

        internal RExecutionTracer(IRSession session) {
            Check.ArgumentNull(nameof(session), session);

            Session = session;
            Session.Connected += RSession_Connected;
            Session.BeforeRequest += RSession_BeforeRequest;
            Session.AfterRequest += RSession_AfterRequest;
        }

        internal void Detach() {
            Session.Connected -= RSession_Connected;
            Session.BeforeRequest -= RSession_BeforeRequest;
            Session.AfterRequest -= RSession_AfterRequest;
            Session = null;
        }

        private void ThrowIfDisposed() {
            if (Session == null) {
                throw new ObjectDisposedException(nameof(RExecutionTracer));
            }
        }

        /// <summary>
        /// Initializes the tracer, enabling all other operations on it.
        /// </summary>
        /// <remarks>
        /// All operations that require initialization will automatically perform it if it hasn't been performed
        /// already, so calling this method is never a requirement. However, since initialization can be potentially
        /// costly, calling it in advance at a more opportune moment can be preferable to lazy initialization.
        /// </remarks>
        internal Task InitializeAsync(CancellationToken cancellationToken = default(CancellationToken)) {
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
                await Session.ExecuteAsync("rtvs:::reapply_breakpoints()"); // TODO: mark all breakpoints as invalid if this fails.

                // Attach might happen when session is already at the Browse prompt, in which case we have
                // missed the corresponding BeginRequest event, but we want to raise Browse anyway. So
                // grab an interaction and check the prompt.
                Session.BeginInteractionAsync(cancellationToken: cancellationToken).ContinueWith(async t => {
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
                Detach();
                throw;
            }
        }

        public async Task<bool> ExecuteBrowserCommandAsync(string command, Func<IRSessionInteraction, Task<bool>> prepare = null, CancellationToken cancellationToken = default(CancellationToken)) {
            ThrowIfDisposed();
            if (command == null) {
                throw new ArgumentNullException(nameof(command));
            }

            await TaskUtilities.SwitchToBackgroundThread();

            using (var inter = await Session.BeginInteractionAsync(isVisible: true, cancellationToken: cancellationToken)) {
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

        public async Task BreakAsync(CancellationToken cancellationToken = default(CancellationToken)) {
            await TaskUtilities.SwitchToBackgroundThread();

            // Evaluation will not end until after Browse> is responded to, but this method must indicate completion
            // as soon as the prompt appears. So don't wait for this, but wait for the prompt instead.
            // If evaluation is canceled (e.g. because user does Cancel All), ExecuteAsync will throw RException -
            // suppress that.
            Session.ExecuteAsync("browser()", REvaluationKind.Reentrant, cancellationToken)
                .SilenceException<RException>()
                .DoNotWait();

            // Wait until prompt appears, but don't actually respond to it.
            using (var inter = await Session.BeginInteractionAsync(true, cancellationToken)) { }
        }

        public async Task ContinueAsync(CancellationToken cancellationToken = default(CancellationToken)) {
            await TaskUtilities.SwitchToBackgroundThread();
            ExecuteBrowserCommandAsync("c", null, cancellationToken).DoNotWait();
        }

        public Task<bool> StepIntoAsync(CancellationToken cancellationToken = default(CancellationToken)) =>
            StepAsync(cancellationToken, "s");

        public Task<bool> StepOverAsync(CancellationToken cancellationToken = default(CancellationToken)) =>
            StepAsync(cancellationToken, "n");

        public Task<bool> StepOutAsync(CancellationToken cancellationToken = default(CancellationToken)) =>
            StepAsync(cancellationToken, "c", async inter => {
                try {
                    // EvaluateAsync will push a new toplevel context on the context stack before
                    // evaluating the expression, so tell browser_set_debug to skip 1 toplevel context
                    // before locating the target context for step-out.
                    await Session.ExecuteAsync("rtvs:::browser_set_debug(1, 1)", REvaluationKind.Normal, cancellationToken);
                } catch (RException) {
                    _stepTcs.TrySetResult(false);
                    return false;
                }
                return true;
            });

        /// <returns>
        /// <c>true</c> if step completed successfully, and <c>false</c> if it was interrupted midway by something
        /// else pausing the process, such as a breakpoint.
        /// </returns>
        private async Task<bool> StepAsync(CancellationToken cancellationToken, string command, Func<IRSessionInteraction, Task<bool>> prepare = null) {
            ThrowIfDisposed();
            await TaskUtilities.SwitchToBackgroundThread();

            _stepTcs = new TaskCompletionSource<bool>();
            ExecuteBrowserCommandAsync(command, prepare, cancellationToken).DoNotWait();
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

        public async Task EnableBreakpointsAsync(bool enable, CancellationToken cancellationToken = default(CancellationToken)) {
            ThrowIfDisposed();
            await TaskUtilities.SwitchToBackgroundThread();
            await Session.ExecuteAsync($"rtvs:::enable_breakpoints({(enable ? "TRUE" : "FALSE")})");
        }

        public async Task<IRBreakpoint> CreateBreakpointAsync(RSourceLocation location, CancellationToken cancellationToken = default(CancellationToken)) {
            ThrowIfDisposed();

            await TaskUtilities.SwitchToBackgroundThread();
            await InitializeAsync(cancellationToken);

            RBreakpoint bp;
            if (!_breakpoints.TryGetValue(location, out bp)) {
                bp = new RBreakpoint(this, location);
                _breakpoints.Add(location, bp);
            }

            await bp.SetBreakpointAsync(cancellationToken);
            return bp;
        }

        internal void RemoveBreakpoint(RBreakpoint breakpoint) {
            Trace.Assert(breakpoint.Tracer == this);
            _breakpoints.Remove(breakpoint.Location);
        }

        private void ProcessBrowsePrompt(IReadOnlyList<IRContext> contexts) {
            if (!contexts.IsBrowser()) {
                return;
            }

            Session.BeginInteractionAsync().ContinueWith(async t => {
                using (var inter = await t) {
                    if (inter.Contexts != contexts) {
                        // Someone else has already responded to this interaction.
                        return;
                    } else {
                        await ProcessBrowsePromptWorker(inter);
                    }
                }
            }).DoNotWait();
        }

        private static readonly Regex _doTraceRegex = new Regex(
            @"^\.doTrace\(.*rtvs:::is_breakpoint\((?<filename>.*),\s*(?<line_number>\d+)\).*\)$",
            RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture | RegexOptions.Singleline);

        private static bool IsDoTrace(IRStackFrame frame) =>
            frame.Call != null && _doTraceRegex.IsMatch(frame.Call);

        private async Task ProcessBrowsePromptWorker(IRSessionInteraction inter) {
            var frames = await Session.TracebackAsync();

            // If there's .doTrace(rtvs:::breakpoint) anywhere on the stack, we're inside the internal machinery
            // that triggered Browse> prompt when hitting a breakpoint. We need to step out of it until we're
            // back at the frame where the breakpoint was actually set, so that those internal frames do not show
            // on the call stack, and further stepping does not try to step through them. 
            // Since browserSetDebug-based step out is not reliable in the presence of loops, we'll just keep
            // stepping over with "n" until we're all the way out. Every step will trigger a new prompt, and
            // we will come back to this method again.
            var doTraceFrame = frames.FirstOrDefault(frame => IsDoTrace(frame));
            if (doTraceFrame != null) {
                await inter.RespondAsync(Invariant($"n\n"));
                return;
            }

            IReadOnlyCollection<RBreakpoint> breakpointsHit = null;
            var lastFrame = frames.LastOrDefault();
            if (lastFrame != null) {
                // Report breakpoints first, so that by the time step completion is reported, all actions associated
                // with breakpoints (e.g. printing messages for tracepoints) have already been completed.
                if (lastFrame.FileName != null && lastFrame.LineNumber != null) {
                    var location = new RSourceLocation(lastFrame.FileName, lastFrame.LineNumber.Value);
                    RBreakpoint bp;
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

            EventHandler<RBrowseEventArgs> browse;
            lock (_browseLock) {
                browse = _browse;
           }

            var eventArgs = new RBrowseEventArgs(inter, isStepCompleted, breakpointsHit);
            _currentBrowseEventArgs = eventArgs;
            browse?.Invoke(this, eventArgs);
        }

        private void RSession_Connected(object sender, EventArgs e) {
            lock (_initializeLock) {
                _initializeTask = null;
            }
            InitializeAsync().DoNotWait();
        }

        private void RSession_BeforeRequest(object sender, RBeforeRequestEventArgs e) {
            _initialPromptCts.Cancel();
            ProcessBrowsePrompt(e.Contexts);
        }

        private void RSession_AfterRequest(object sender, RAfterRequestEventArgs e) {
            _currentBrowseEventArgs = null;
        }
    }
}
