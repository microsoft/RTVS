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

        public async Task Initialize() {
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
                using (var inter = await t) {
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

            var quotedExpr = expression.Replace("\\", "\\\\").Replace("'", "\'");
            var res = await EvaluateRawAsync($".rtvs.eval('{quotedExpr}', ${env})", throwOnError: false).ConfigureAwait(false);

            if (res.ParseStatus != RParseStatus.OK) {
                return new DebugFailedEvaluationResult(expression, res.ParseStatus.ToString());
            } else if (res.Error != null) {
                return new DebugFailedEvaluationResult(expression, res.Error);
            } else if (res.Result == null) {
                return new DebugFailedEvaluationResult(expression, "No result");
            }

            return new DebugSuccessfulEvaluationResult(stackFrame, expression, JObject.Parse(res.Result));
        }

        public Task StepIntoAsync() {
            return Step("s");
        }

        public Task StepOverAsync() {
            return Step("n");
        }

        public Task StepOutAsync() {
            return Step("browserSetDebug()", "c");
        }

        private async Task Step(params string[] commands) {
            Debug.Assert(commands.Length > 0);
            ThrowIfDisposed();

            _stepTcs = new TaskCompletionSource<object>();
            for (int i = 0; i < commands.Length - 1; ++i) {
                await ExecuteBrowserCommandAsync(commands[i]);
            }

            ExecuteBrowserCommandAsync(commands.Last()).DoNotWait();
            await _stepTcs.Task;
        }

        public void CancelStep() {
            ThrowIfDisposed();

            if (_stepTcs == null) {
                throw new InvalidOperationException("No step to end.");
            }

            _stepTcs.TrySetCanceled();
            _stepTcs = null;
        }

        public async Task<IReadOnlyList<DebugStackFrame>> GetStackFrames() {
            ThrowIfDisposed();

            REvaluationResult res;
            using (var eval = await RSession.BeginEvaluationAsync().ConfigureAwait(false)) {
                res = await eval.EvaluateAsync(".rtvs.traceback()", reentrant: false).ConfigureAwait(false);
            }

            if (res.ParseStatus != RParseStatus.OK || res.Error != null || res.Result == null) {
                Debug.Fail(".rtvs.traceback() failed");
                return new DebugStackFrame[0];
            }

            JArray jFrames;
            try {
                jFrames = JArray.Parse(res.Result);
            } catch (JsonException) {
                Debug.Fail("Failed to parse JSON returned by .rtvs.traceback()");
                return new DebugStackFrame[0];
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
