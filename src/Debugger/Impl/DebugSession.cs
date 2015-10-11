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

namespace Microsoft.R.Debugger {
    public sealed class DebugSession : IDisposable {
        private TaskCompletionSource<object> _stepTcs;
        private CancellationTokenSource _initialPromptCts = new CancellationTokenSource();

        // Key is filename + line number; value is the count of breakpoints set for that line.
        private Dictionary<Tuple<string, int>, int> _breakpoints = new Dictionary<Tuple<string, int>, int>();

        public IRSession RSession { get; set; }

        public event EventHandler Browse;

        public DebugSession(IRSession session) {
            RSession = session;
            RSession.BeforeRequest += RSession_BeforeRequest;
            RSession.AfterRequest += RSession_AfterRequest;
        }

        public void Dispose() {
            RSession.BeforeRequest -= RSession_BeforeRequest;
            RSession.AfterRequest -= RSession_AfterRequest;
            RSession = null;
        }

        private void ThrowIfDisposed() {
            if (RSession == null) {
                throw new ObjectDisposedException(nameof(DebugSession));
            }
        }

        public async Task InitializeAsync() {
            ThrowIfDisposed();

            string helpers;
            using (var stream = typeof(DebugSession).Assembly.GetManifestResourceStream(typeof(DebugSession).Namespace + ".DebugHelpers.R"))
            using (var reader = new StreamReader(stream)) {
                helpers = await reader.ReadToEndAsync().ConfigureAwait(false);
            }
            helpers = helpers.Replace("\r", "");

            REvaluationResult res;
            using (var eval = await RSession.BeginEvaluationAsync().ConfigureAwait(false)) {
                res = await eval.EvaluateAsync(helpers, reentrant: false).ConfigureAwait(false);
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
                using (var inter = await t.ConfigureAwait(false)) {
                    // If we got AfterRequest before we got here, then that has already taken care of
                    // the prompt; or if it's not a Browse prompt, will do so in a future event. Bail out.
                    _initialPromptCts.Token.ThrowIfCancellationRequested();
                    // Otherwise, treat it the same as if AfterRequest just happened.
                    CheckForBrowse(inter.Contexts);
                }
            }).DoNotWait();
        }

        public async Task ExecuteBrowserCommandAsync(string command) {
            ThrowIfDisposed();
            using (var inter = await RSession.BeginInteractionAsync(isVisible: true).ConfigureAwait(false)) {
                if (IsInBrowseMode(inter.Contexts)) {
                    await inter.RespondAsync(command + "\n").ConfigureAwait(false);
                }
            }
        }

        internal async Task<REvaluationResult> EvaluateRawAsync(string expression, bool throwOnError = true) {
            ThrowIfDisposed();
            using (var eval = await RSession.BeginEvaluationAsync().ConfigureAwait(false)) {
                var res = await eval.EvaluateAsync(expression, reentrant: false).ConfigureAwait(false);
                if (throwOnError && (res.ParseStatus != RParseStatus.OK || res.Error != null || res.Result == null)) {
                    throw new REvaluationException(res);
                }
                return res;
            }
        }

        public async Task<DebugEvaluationResult> EvaluateAsync(DebugStackFrame stackFrame, string expression, string env = "NULL") {
            ThrowIfDisposed();

            var res = await EvaluateRawAsync($".rtvs.eval({expression.ToRStringLiteral()}, {env})", throwOnError: false).ConfigureAwait(false);

            if (res.ParseStatus != RParseStatus.OK) {
                Debug.Fail("Malformed .rtvs.eval");
                return new DebugFailedEvaluationResult(expression, "RParseStatus." + res.ParseStatus);
            } else if (res.Error != null) {
                return new DebugFailedEvaluationResult(expression, res.Error);
            } else if (res.Result == null) {
                return new DebugFailedEvaluationResult(expression, "No result");
            }

            return new DebugSuccessfulEvaluationResult(stackFrame, expression, JObject.Parse(res.Result));
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
            Debug.Assert(commands.Length > 0);
            ThrowIfDisposed();

            _stepTcs = new TaskCompletionSource<object>();
            for (int i = 0; i < commands.Length - 1; ++i) {
                await ExecuteBrowserCommandAsync(commands[i]).ConfigureAwait(false);
            }

            ExecuteBrowserCommandAsync(commands.Last()).DoNotWait();
            await _stepTcs.Task.ConfigureAwait(false);
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

            REvaluationResult res;
            using (var eval = await RSession.BeginEvaluationAsync().ConfigureAwait(false)) {
                res = await eval.EvaluateAsync(".rtvs.traceback()", reentrant: false).ConfigureAwait(false);
            }

            if (res.ParseStatus != RParseStatus.OK || res.Error != null || res.Result == null) {
                throw new InvalidDataException(".rtvs.traceback() failed");
            }

            JArray jFrames;
            try {
                jFrames = JArray.Parse(res.Result);
            } catch (JsonException ex) {
                throw new InvalidDataException("Failed to parse JSON returned by .rtvs.traceback()", ex);
            }

            var stackFrames = new List<DebugStackFrame>();

            string callingExpression = null;
            int i = 0;
            foreach (JObject jFrame in jFrames) {
                DebugStackFrame stackFrame;
                try {
                    string fileName = (string)jFrame["filename"];
                    int? lineNumber = (int?)(double?)jFrame["linenum"];
                    bool isGlobal = (bool)jFrame["is_global"];

                    stackFrame = new DebugStackFrame(this, i, fileName, lineNumber, callingExpression, isGlobal);

                    callingExpression = (string)jFrame["call"];
                } catch (JsonException ex) {
                    Debug.Fail(ex.ToString());
                    stackFrame = new DebugStackFrame(this, i, null, null, null, false);
                    callingExpression = null;
                }

                stackFrames.Add(stackFrame);
                ++i;
            }

            stackFrames.Reverse();
            return stackFrames;
        }

        public async Task<int> AddBreakpointAsync(string fileName, int lineNumber) {
            var res = await EvaluateAsync(null, $"setBreakpoint({fileName.ToRStringLiteral()}, {lineNumber})").ConfigureAwait(false);
            if (res is DebugFailedEvaluationResult) {
                throw new InvalidOperationException($"{res.Expression}: {res}");
            }

            var location = Tuple.Create(fileName, lineNumber);
            if (!_breakpoints.ContainsKey(location)) {
                _breakpoints.Add(location, 0);
            }

            return ++_breakpoints[location];
        }

        public async Task<int> RemoveBreakpointAsync(string fileName, int lineNumber) {
            var location = Tuple.Create(fileName, lineNumber);
            if (!_breakpoints.ContainsKey(location)) {
                return 0;
            }

            var res = await EvaluateAsync(null, $"setBreakpoint({fileName.ToRStringLiteral()}, {lineNumber}, clear=TRUE)").ConfigureAwait(false);
            if (res is DebugFailedEvaluationResult) {
                throw new InvalidOperationException($"{res.Expression}: {res}");
            }

            int count = --_breakpoints[location];
            Debug.Assert(count >= 0);
            if (count == 0) {
                _breakpoints.Remove(location);
            }

            return count;
        }

        private bool IsInBrowseMode(IReadOnlyList<IRContext> contexts) {
            return contexts.SkipWhile(context => context.CallFlag.HasFlag(RContextType.Restart))
                .FirstOrDefault()?.CallFlag.HasFlag(RContextType.Browser) == true;
        }

        private void CheckForBrowse(IReadOnlyList<IRContext> contexts) {
            if (IsInBrowseMode(contexts)) {
                if (_stepTcs != null) {
                    var stepTcs = _stepTcs;
                    _stepTcs = null;
                    stepTcs.TrySetResult(null);
                }

                Browse?.Invoke(this, EventArgs.Empty);
            }
        }

        private void RSession_BeforeRequest(object sender, RRequestEventArgs e) {
            _initialPromptCts.Cancel();
            CheckForBrowse(e.Contexts);
        }

        private void RSession_AfterRequest(object sender, RRequestEventArgs e) {
        }
    }

    public class REvaluationException : Exception {
        public REvaluationResult Result { get; }

        public REvaluationException(REvaluationResult result) {
            Result = result;
        }
    }
}
