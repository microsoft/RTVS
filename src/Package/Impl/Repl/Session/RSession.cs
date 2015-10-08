using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Microsoft.Languages.Editor.Shell;
using Microsoft.R.Host.Client;
using Microsoft.R.Support.Settings;
using Microsoft.VisualStudio.R.Package.Plots;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace Microsoft.VisualStudio.R.Package.Repl.Session {
    internal sealed class RSession : IRSession, IRCallbacks {
        private readonly BufferBlock<RSessionRequestSource> _pendingRequestSources = new BufferBlock<RSessionRequestSource>();
        private readonly BufferBlock<RSessionEvaluationSource> _pendingEvaluationSources = new BufferBlock<RSessionEvaluationSource>();

        public event EventHandler<RRequestEventArgs> BeforeRequest;
        public event EventHandler<RRequestEventArgs> AfterRequest;
        public event EventHandler<RResponseEventArgs> Response;
        public event EventHandler<RErrorEventArgs> Error;
        public event EventHandler<EventArgs> Disconnected;

        /// <summary>
        /// ReadConsole requires a task even if there are no pending requests
        /// </summary>
        private IReadOnlyCollection<IRContext> _contexts;
        private RHost _host;
        private Task _hostRunTask;
        private TaskCompletionSource<object> _initializationTcs;
        private RSessionRequestSource _currentRequestSource;

        public string Prompt { get; private set; } = "> ";
        public int MaxLength { get; private set; } = 0x1000;
        public bool IsHostRunning => _hostRunTask != null && !_hostRunTask.IsCompleted;

        public void Dispose() {
            _host?.Dispose();
        }

        public Task<IRSessionInteraction> BeginInteractionAsync(bool isVisible = true) {
            RSessionRequestSource requestSource = new RSessionRequestSource(isVisible, _contexts);
            _pendingRequestSources.Post(requestSource);
            return requestSource.CreateRequestTask;
        }

        public Task<IRSessionEvaluation> BeginEvaluationAsync() {
            var source = new RSessionEvaluationSource();
            _pendingEvaluationSources.Post(source);
            return source.Task;
        }

        public Task StartHostAsync() {
            if (_hostRunTask != null && !_hostRunTask.IsCompleted) {
                throw new InvalidOperationException("Another instance of RHost is running for this RSession. Stop it before starting new one.");
            }

            _host = new RHost(this);
            _initializationTcs = new TaskCompletionSource<object>();

            _hostRunTask = Task.Run(() => _host.CreateAndRun(RToolsSettings.GetRVersionPath()));

            var initializationTask = _initializationTcs.Task.ContinueWith(new Func<Task, Task>(AfterInitialization)).Unwrap();

            return Task.WhenAny(initializationTask, _hostRunTask).Unwrap();
        }

        public async Task StopHostAsync() {
            if (_hostRunTask.IsCompleted) {
                return;
            }

            var request = await BeginInteractionAsync(false);
            if (_hostRunTask.IsCompleted) {
                request.Dispose();
                return;
            }

            await request.Quit();
            await _hostRunTask;
        }

        private async Task AfterInitialization(Task task) {
            using (var evaluation = await BeginEvaluationAsync()) {
                await evaluation.SetVsGraphicsDevice();
                await evaluation.SetDefaultWorkingDirectory();
            }
        }

        Task IRCallbacks.Connected(string rVersion) {
            _initializationTcs.SetResult(null);
            return Task.CompletedTask;
        }

        Task IRCallbacks.Disconnected() {

            var currentRequest = Interlocked.Exchange(ref _currentRequestSource, null);
            currentRequest?.Complete();

            IList<RSessionRequestSource> requestSources;
            if (_pendingRequestSources.TryReceiveAll(out requestSources)) {
                foreach (var requestSource in requestSources) {
                    requestSource.TryCancel();
                }
            }

            IList<RSessionEvaluationSource> evalSources;
            if (_pendingEvaluationSources.TryReceiveAll(out evalSources)) {
                foreach (var evalSource in evalSources) {
                    evalSource.TryCancel();
                }
            }

            _contexts = null;
            Prompt = string.Empty;

            OnDisconnected();
            return Task.CompletedTask;
        }

        async Task<string> IRCallbacks.ReadConsole(IReadOnlyCollection<IRContext> contexts, string prompt, string buf, int len, bool addToHistory, bool isEvaluationAllowed, CancellationToken ct) {
            var currentRequest = Interlocked.Exchange(ref _currentRequestSource, null);
            currentRequest?.Complete();

            _contexts = contexts;
            Prompt = prompt;
            MaxLength = len;

            OnBeforeRequest(contexts, prompt, len, addToHistory);

            CancellationTokenSource evaluationCts;
            Task evaluationTask;

            if (isEvaluationAllowed) {
                evaluationCts = new CancellationTokenSource();
                evaluationTask = EvaluateUntilCancelled(contexts, evaluationCts.Token, ct);
            } else {
                evaluationCts = null;
                evaluationTask = Task.CompletedTask;
            }

            string consoleInput = null;

            while (consoleInput == null) {
                ct.ThrowIfCancellationRequested();

                try {
                    consoleInput = await ReadNextRequest(prompt, len, ct);
                } catch (TaskCanceledException) {
                    // If request was canceled through means other than our token, it indicates the refusal of
                    // that requestor to respond to that particular prompt, so move on to the next requestor.
                    // If it was canceled through the token, then host itself is shutting down, and cancellation
                    // will be propagated on the entry to next iteration of this loop.
                }
            }

            // If evaluation was allowed, cancel evaluation processing but await evaluation that is in progress
            evaluationCts?.Cancel();
            await evaluationTask;

            OnAfterRequest(contexts, prompt, len, addToHistory);

            return consoleInput;
        }

        private async Task<string> ReadNextRequest(string prompt, int len, CancellationToken ct) {
            var requestSource = await _pendingRequestSources.ReceiveAsync(ct);

            TaskCompletionSource<string> requestTcs = new TaskCompletionSource<string>();
            Interlocked.Exchange(ref _currentRequestSource, requestSource);

            requestSource.Request(prompt, len, requestTcs);
            ct.Register(delegate { requestTcs.TrySetCanceled(); });

            string response = await requestTcs.Task;

            Debug.Assert(response.Length < len); // len includes null terminator
            if (response.Length >= len) {
                response = response.Substring(0, len - 1);
            }

            return response;
        }

        private async Task EvaluateUntilCancelled(IReadOnlyCollection<IRContext> contexts, CancellationToken evaluationCancellationToken, CancellationToken hostCancellationToken) {
            var ct = CancellationTokenSource.CreateLinkedTokenSource(hostCancellationToken, evaluationCancellationToken).Token;

            while (!ct.IsCancellationRequested) {
                try {
                    var evaluationSource = await _pendingEvaluationSources.ReceiveAsync(ct);
                    await evaluationSource.BeginEvaluationAsync(contexts, _host, hostCancellationToken);
                } catch (TaskCanceledException) {
                    return;
                }
            }
        }

        private async Task EvaluateAll(IReadOnlyCollection<IRContext> contexts, CancellationToken ct) {
            RSessionEvaluationSource source;
            while (!ct.IsCancellationRequested && _pendingEvaluationSources.TryReceive(out source)) {
                await source.BeginEvaluationAsync(contexts, _host, ct);
            }
        }

        Task IRCallbacks.WriteConsoleEx(IReadOnlyCollection<IRContext> contexts, string buf, OutputType otype, CancellationToken ct) {
            if (otype == OutputType.Error) {
                OnError(contexts, buf);

                var currentRequest = Interlocked.Exchange(ref _currentRequestSource, null);
                currentRequest?.Fail(buf);
            } else {
                OnResponse(contexts, buf);
            }

            return Task.CompletedTask;
        }

        async Task IRCallbacks.ShowMessage(IReadOnlyCollection<IRContext> contexts, string message, CancellationToken ct) {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(CancellationToken.None);
            EditorShell.Current.ShowErrorMessage(message);
        }

        async Task<YesNoCancel> IRCallbacks.YesNoCancel(IReadOnlyCollection<IRContext> contexts, string s, bool isEvaluationAllowed, CancellationToken ct) {
            if (isEvaluationAllowed) {
                await EvaluateAll(contexts, ct);
            }

            return YesNoCancel.Yes;
        }

        Task IRCallbacks.Busy(IReadOnlyCollection<IRContext> contexts, bool which, CancellationToken ct) {
            return Task.CompletedTask;
        }

        async Task IRCallbacks.PlotXaml(IReadOnlyCollection<IRContext> contexts, string xamlFilePath, CancellationToken ct) {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(CancellationToken.None);

            var frame = FindPlotWindow(0);
            if (frame != null) {
                object docView;
                ErrorHandler.ThrowOnFailure(frame.GetProperty((int)__VSFPROPID.VSFPROPID_DocView, out docView));
                if (docView != null) {
                    PlotWindowPane pane = (PlotWindowPane)docView;
                    pane.DisplayXamlFile(xamlFilePath);
                }
            }
        }

        private static IVsWindowFrame FindPlotWindow(__VSFINDTOOLWIN flags) {
            IVsUIShell shell = AppShell.Current.GetGlobalService<IVsUIShell>(typeof(SVsUIShell));

            // First just find. If it exists, use it. 
            IVsWindowFrame frame;
            Guid persistenceSlot = typeof(PlotWindowPane).GUID;
            shell.FindToolWindow((uint)flags, ref persistenceSlot, out frame);
            return frame;
        }

        private void OnBeforeRequest(IReadOnlyCollection<IRContext> contexts, string prompt, int maxLength, bool addToHistory) {
            var handlers = BeforeRequest;
            if (handlers != null) {
                var args = new RRequestEventArgs(contexts, prompt, maxLength, addToHistory);
                handlers(this, args);
            }
        }

        private void OnAfterRequest(IReadOnlyCollection<IRContext> contexts, string prompt, int maxLength, bool addToHistory) {
            var handlers = AfterRequest;
            if (handlers != null) {
                var args = new RRequestEventArgs(contexts, prompt, maxLength, addToHistory);
                handlers(this, args);
            }
        }

        private void OnResponse(IReadOnlyCollection<IRContext> contexts, string message) {
            var handlers = Response;
            if (handlers != null) {
                var args = new RResponseEventArgs(contexts, message);
                handlers(this, args);
            }
        }

        private void OnError(IReadOnlyCollection<IRContext> contexts, string message) {
            var handlers = Error;
            if (handlers != null) {
                var args = new RErrorEventArgs(contexts, message);
                handlers(this, args);
            }
        }

        private void OnDisconnected() {
            var handlers = Disconnected;
            if (handlers != null) {
                var args = new EventArgs();
                handlers(this, args);
            }
        }
    }
}