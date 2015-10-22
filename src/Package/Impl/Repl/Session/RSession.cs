using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Microsoft.Common.Core;
using Microsoft.Languages.Editor.Shell;
using Microsoft.R.Host.Client;
using Microsoft.R.Support.Utility;
using Microsoft.VisualStudio.R.Package.Plots;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace Microsoft.VisualStudio.R.Package.Repl.Session {
    internal sealed class RSession : IRSession, IRCallbacks {
        private static string DefaultPrompt = "> ";

        private readonly BufferBlock<RSessionRequestSource> _pendingRequestSources = new BufferBlock<RSessionRequestSource>();
        private readonly BufferBlock<RSessionEvaluationSource> _pendingEvaluationSources = new BufferBlock<RSessionEvaluationSource>();

        public event EventHandler<RRequestEventArgs> BeforeRequest;
        public event EventHandler<RRequestEventArgs> AfterRequest;
        public event EventHandler<ROutputEventArgs> Output;
        public event EventHandler<EventArgs> Disconnected;
        public event EventHandler<EventArgs> Disposed;

        /// <summary>
        /// ReadConsole requires a task even if there are no pending requests
        /// </summary>
        private IReadOnlyList<IRContext> _contexts;
        private RHost _host;
        private Task _hostRunTask;
        private TaskCompletionSource<object> _initializationTcs;
        private RSessionRequestSource _currentRequestSource;

        public int Id { get; }
        public string Prompt { get; private set; } = DefaultPrompt;
        public int MaxLength { get; private set; } = 0x1000;
        public bool IsHostRunning => _hostRunTask != null && !_hostRunTask.IsCompleted;

        public RSession(int id) {
            Id = id;
        }

        public void Dispose() {
            _host?.Dispose();
            Disposed?.Invoke(this, EventArgs.Empty);
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

        public Task CancelAllAsync() {
            return _host.CancelAllAsync();
        }

        public async Task StartHostAsync() {
            if (_hostRunTask != null && !_hostRunTask.IsCompleted) {
                throw new InvalidOperationException("Another instance of RHost is running for this RSession. Stop it before starting new one.");
            }

            await TaskUtilities.SwitchToBackgroundThread();

            _host = new RHost(this);
            _initializationTcs = new TaskCompletionSource<object>();

            _hostRunTask = _host.CreateAndRun(RInstallation.GetRInstallPath());
            this.ScheduleEvaluation(async e => {
                //await e.SetVsGraphicsDevice();
                await e.SetDefaultWorkingDirectory();
                await e.PrepareDataInspect();
            });

            var initializationTask = _initializationTcs.Task;
            await Task.WhenAny(initializationTask, _hostRunTask).Unwrap();
        }

        public async Task StopHostAsync() {
            if (_hostRunTask.IsCompleted) {
                return;
            }

            await TaskUtilities.SwitchToBackgroundThread();

            var requestTask = BeginInteractionAsync(false);
            await Task.WhenAny(requestTask, Task.Delay(200)).Unwrap();

            if (_hostRunTask.IsCompleted) {
                requestTask
                    .ContinueWith(t => t.Result.Dispose(), TaskContinuationOptions.OnlyOnRanToCompletion)
                    .DoNotWait();
                return;
            }

            if (requestTask.Status == TaskStatus.RanToCompletion) {
                using (var inter = await requestTask) {
                    // Try graceful shutdown with q() first.
                    try {
                        await Task.WhenAny(_hostRunTask, inter.Quit(), Task.Delay(500)).Unwrap();
                    } catch (Exception) {
                    }

                    if (_hostRunTask.IsCompleted) {
                        return;
                    }

                    // If that doesn't work, then try sending the disconnect packet to the host -
                    // it will call R_Suicide, which is not graceful, but at least it's cooperative.
                    await Task.WhenAny(_host.DisconnectAsync(), Task.Delay(500)).Unwrap();

                    if (_hostRunTask.IsCompleted) {
                        return;
                    }
                }
            }

            // If nothing worked, then just kill the host process.
            _host?.Dispose();
            await _hostRunTask;
        }

        Task IRCallbacks.Connected(string rVersion) {
            Prompt = DefaultPrompt;
            _initializationTcs.SetResult(null);
            return Task.CompletedTask;
        }

        Task IRCallbacks.Disconnected() {
            var currentRequest = Interlocked.Exchange(ref _currentRequestSource, null);
            currentRequest?.Complete();

            RSessionRequestSource requestSource;
            while (_pendingRequestSources.TryReceive(out requestSource)) {
                requestSource.TryCancel();
            }

            RSessionEvaluationSource evalSource;
            while (_pendingEvaluationSources.TryReceive(out evalSource)) {
                evalSource.TryCancel();
            }

            _contexts = null;
            Prompt = string.Empty;

            Disconnected?.Invoke(this, EventArgs.Empty);
            return Task.CompletedTask;
        }

        async Task<string> IRCallbacks.ReadConsole(IReadOnlyList<IRContext> contexts, string prompt, int len, bool addToHistory, bool isEvaluationAllowed, CancellationToken ct) {
            await TaskUtilities.SwitchToBackgroundThread();

            var currentRequest = Interlocked.Exchange(ref _currentRequestSource, null);

            _contexts = contexts;
            Prompt = prompt;
            MaxLength = len;

            var requestEventArgs = new RRequestEventArgs(contexts, prompt, len, addToHistory);
            BeforeRequest?.Invoke(this, requestEventArgs);

            CancellationTokenSource evaluationCts;
            Task evaluationTask;

            if (isEvaluationAllowed) {
                await EvaluateAll(contexts, ct);
                evaluationCts = new CancellationTokenSource();
                evaluationTask = EvaluateUntilCancelled(contexts, evaluationCts.Token, ct);
            } else {
                evaluationCts = null;
                evaluationTask = Task.CompletedTask;
            }

            currentRequest?.Complete();

            string consoleInput = null;

            do {
                ct.ThrowIfCancellationRequested();
                try {
                    consoleInput = await ReadNextRequest(prompt, len, ct);
                } catch (OperationCanceledException) {
                    // If request was canceled through means other than our token, it indicates the refusal of
                    // that requestor to respond to that particular prompt, so move on to the next requestor.
                    // If it was canceled through the token, then host itself is shutting down, and cancellation
                    // will be propagated on the entry to next iteration of this loop.
                }
            } while (consoleInput == null);

            // If evaluation was allowed, cancel evaluation processing but await evaluation that is in progress
            evaluationCts?.Cancel();
            await evaluationTask;

            AfterRequest?.Invoke(this, requestEventArgs);

            return consoleInput;
        }

        private async Task<string> ReadNextRequest(string prompt, int len, CancellationToken ct) {
            TaskUtilities.AssertIsOnBackgroundThread();

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

        private async Task EvaluateUntilCancelled(IReadOnlyList<IRContext> contexts, CancellationToken evaluationCancellationToken, CancellationToken hostCancellationToken) {
            TaskUtilities.AssertIsOnBackgroundThread();

            var ct = CancellationTokenSource.CreateLinkedTokenSource(hostCancellationToken, evaluationCancellationToken).Token;
            while (!ct.IsCancellationRequested) {
                try {
                    var evaluationSource = await _pendingEvaluationSources.ReceiveAsync(ct);
                    await evaluationSource.BeginEvaluationAsync(contexts, _host, hostCancellationToken);
                } catch (OperationCanceledException) {
                    return;
                }
            }
        }

        private async Task EvaluateAll(IReadOnlyList<IRContext> contexts, CancellationToken ct) {
            TaskUtilities.AssertIsOnBackgroundThread();

            RSessionEvaluationSource source;
            while (!ct.IsCancellationRequested && _pendingEvaluationSources.TryReceive(out source)) {
                await source.BeginEvaluationAsync(contexts, _host, ct);
            }
        }

        Task IRCallbacks.WriteConsoleEx(string buf, OutputType otype, CancellationToken ct) {
            Output?.Invoke(this, new ROutputEventArgs(otype, buf));

            if (otype == OutputType.Error) {
                var currentRequest = Interlocked.Exchange(ref _currentRequestSource, null);
                currentRequest?.Fail(buf);
            }

            return Task.CompletedTask;
        }

        async Task IRCallbacks.ShowMessage(string message, CancellationToken ct) {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(CancellationToken.None);
            EditorShell.Current.ShowErrorMessage(message);
        }

        async Task<YesNoCancel> IRCallbacks.YesNoCancel(IReadOnlyList<IRContext> contexts, string s, bool isEvaluationAllowed, CancellationToken ct) {
            await TaskUtilities.SwitchToBackgroundThread();

            if (isEvaluationAllowed) {
                await EvaluateAll(contexts, ct);
            }

            return YesNoCancel.Yes;
        }

        Task IRCallbacks.Busy(bool which, CancellationToken ct) {
            return Task.CompletedTask;
        }

        async Task IRCallbacks.PlotXaml(string xamlFilePath, CancellationToken ct) {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(CancellationToken.None);

            var frame = FindPlotWindow(__VSFINDTOOLWIN.FTW_fFindFirst | __VSFINDTOOLWIN.FTW_fForceCreate);  // TODO: acquire plot content provider through service
            if (frame != null) {
                object docView;
                ErrorHandler.ThrowOnFailure(frame.GetProperty((int)__VSFPROPID.VSFPROPID_DocView, out docView));
                if (docView != null) {
                    PlotWindowPane pane = (PlotWindowPane)docView;
                    pane.PlotContentProvider.LoadFileOnIdle(xamlFilePath);

                    frame.ShowNoActivate();
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

        private void OnBeforeRequest(IReadOnlyList<IRContext> contexts, string prompt, int maxLength, bool addToHistory) {
            var handlers = BeforeRequest;
            if (handlers != null) {
                var args = new RRequestEventArgs(contexts, prompt, maxLength, addToHistory);
                handlers(this, args);
            }
        }

        private void OnAfterRequest(IReadOnlyList<IRContext> contexts, string prompt, int maxLength, bool addToHistory) {
            var handlers = AfterRequest;
            if (handlers != null) {
                var args = new RRequestEventArgs(contexts, prompt, maxLength, addToHistory);
                handlers(this, args);
            }
        }
    }
}