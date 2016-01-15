using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Actions.Utility;
using Task = System.Threading.Tasks.Task;
using static System.FormattableString;

namespace Microsoft.R.Host.Client.Session {
    internal sealed class RSession : IRSession, IRCallbacks {
        private readonly static string DefaultPrompt = "> ";
        private readonly static Task<IRSessionEvaluation> CanceledEvaluationTask;
        private readonly static Task<IRSessionInteraction> CanceledInteractionTask;

        private readonly BufferBlock<RSessionRequestSource> _pendingRequestSources = new BufferBlock<RSessionRequestSource>();
        private readonly BufferBlock<RSessionEvaluationSource> _pendingEvaluationSources = new BufferBlock<RSessionEvaluationSource>();

        public event EventHandler<RRequestEventArgs> BeforeRequest;
        public event EventHandler<RRequestEventArgs> AfterRequest;
        public event EventHandler<EventArgs> Mutated;
        public event EventHandler<ROutputEventArgs> Output;
        public event EventHandler<EventArgs> Connected;
        public event EventHandler<EventArgs> Disconnected;
        public event EventHandler<EventArgs> Disposed;
        public event EventHandler<EventArgs> DirectoryChanged;

        /// <summary>
        /// ReadConsole requires a task even if there are no pending requests
        /// </summary>
        private IReadOnlyList<IRContext> _contexts;
        private RHost _host;
        private Task _hostRunTask;
        private TaskCompletionSource<object> _initializationTcs;
        private RSessionRequestSource _currentRequestSource;
        private readonly IRHostClientApp _hostClientApp;
        private readonly Action _onDispose;
        private volatile bool _isHostRunning;

        public int Id { get; }
        public string Prompt { get; private set; } = DefaultPrompt;
        public int MaxLength { get; private set; } = 0x1000;
        public bool IsHostRunning => _isHostRunning;

        static RSession() {
            var tcs = new CancellationTokenSource();
            tcs.Cancel();
            CanceledEvaluationTask = Task.FromCanceled<IRSessionEvaluation>(tcs.Token);
            CanceledInteractionTask = Task.FromCanceled<IRSessionInteraction>(tcs.Token);
        }

        public RSession(int id, IRHostClientApp hostClientApp, Action onDispose) {
            Id = id;
            _hostClientApp = hostClientApp;
            _onDispose = onDispose;
        }

        public void Dispose() {
            _host?.Dispose();
            Disposed?.Invoke(this, EventArgs.Empty);
            _onDispose();
        }

        public Task<IRSessionInteraction> BeginInteractionAsync(bool isVisible = true) {
            if (!_isHostRunning) {
                return CanceledInteractionTask;
            }

            RSessionRequestSource requestSource = new RSessionRequestSource(isVisible, _contexts);
            _pendingRequestSources.Post(requestSource);

            return _isHostRunning ? requestSource.CreateRequestTask : CanceledInteractionTask;
        }

        public Task<IRSessionEvaluation> BeginEvaluationAsync(bool isMutating = true) {
            if (!_isHostRunning) {
                return CanceledEvaluationTask;
            }

            var source = new RSessionEvaluationSource(isMutating);
            _pendingEvaluationSources.Post(source);

            return _isHostRunning ? source.Task : CanceledEvaluationTask;
        }

        public async Task CancelAllAsync() {
            var cancelTask = _host.CancelAllAsync();

            var currentRequest = Interlocked.Exchange(ref _currentRequestSource, null);
            currentRequest?.TryCancel();
            ClearPendingRequests();

            await cancelTask;
        }

        public async Task StartHostAsync(RHostStartupInfo startupInfo) {
            if (_hostRunTask != null && !_hostRunTask.IsCompleted) {
                throw new InvalidOperationException("Another instance of RHost is running for this RSession. Stop it before starting new one.");
            }

            await TaskUtilities.SwitchToBackgroundThread();

            _host = new RHost(startupInfo.Name, this);
            _initializationTcs = new TaskCompletionSource<object>();
            ClearPendingRequests();

            _hostRunTask = _host.CreateAndRun(RInstallation.GetRInstallPath(startupInfo.RBasePath), startupInfo.RCommandLineArguments);
            ScheduleAfterHostStarted(startupInfo);

            var initializationTask = _initializationTcs.Task;
            await Task.WhenAny(initializationTask, _hostRunTask).Unwrap();
        }

        public async Task StopHostAsync() {
            if (_hostRunTask == null || _hostRunTask.IsCompleted) {
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

        private void ScheduleAfterHostStarted(RHostStartupInfo startupInfo) {
            var afterHostStartedEvaluationSource = new RSessionEvaluationSource(true);
            _pendingEvaluationSources.Post(afterHostStartedEvaluationSource);
            AfterHostStarted(afterHostStartedEvaluationSource, startupInfo).DoNotWait();
        }

        private async Task AfterHostStarted(RSessionEvaluationSource evaluationSource, RHostStartupInfo startupInfo) {
            using (var evaluation = await evaluationSource.Task) {
                // Load RTVS R package before doing anything in R since the calls
                // below calls may depend on functions exposed from the RTVS package
                await LoadRtvsPackage(evaluation);

                await evaluation.SetDefaultWorkingDirectory();
                await evaluation.SetRdHelpExtraction();

                if (_hostClientApp != null) {
                    await evaluation.SetVsGraphicsDevice();

                    string mirrorUrl = _hostClientApp.CranUrlFromName(startupInfo.CranMirrorName);
                    await evaluation.SetVsCranSelection(mirrorUrl);

                    await evaluation.SetVsHelpRedirection();
                    await evaluation.SetChangeDirectoryRedirection();
                }
            }
        }

        private static async Task LoadRtvsPackage(IRSessionEvaluation eval) {
            var libPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetAssemblyPath());
            var res = await eval.EvaluateAsync(Invariant($"base::loadNamespace('rtvs', lib.loc = {libPath.ToRStringLiteral()})"));

            if (res.ParseStatus != RParseStatus.OK) {
                throw new InvalidDataException("Failed to parse loadNamespace('rtvs'): " + res.ParseStatus);
            } else if (res.Error != null) {
                throw new InvalidDataException("Failed to execute loadNamespace('rtvs'): " + res.Error);
            }
        }

        public void FlushLog() {
            _host?.FlushLog();
        }

        Task IRCallbacks.Connected(string rVersion) {
            Prompt = DefaultPrompt;
            _isHostRunning = true;
            _initializationTcs.SetResult(null);
            Connected?.Invoke(this, EventArgs.Empty);
            Mutated?.Invoke(this, EventArgs.Empty);
            return Task.CompletedTask;
        }

        Task IRCallbacks.Disconnected() {
            _isHostRunning = false;
            Disconnected?.Invoke(this, EventArgs.Empty);

            var currentRequest = Interlocked.Exchange(ref _currentRequestSource, null);
            currentRequest?.Complete();

            ClearPendingRequests();

            return Task.CompletedTask;
        }

        private void ClearPendingRequests() {
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
                evaluationCts = new CancellationTokenSource();
                evaluationTask = EvaluateUntilCancelled(contexts, evaluationCts.Token, ct); // will raise Mutate
            } else {
                evaluationCts = null;
                evaluationTask = Task.CompletedTask;
                Mutated?.Invoke(this, EventArgs.Empty);
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
            bool mutated = true; // start with true on the assumption that the preceding interaction has mutated something
            while (!ct.IsCancellationRequested) {
                try {
                    if (await EvaluateAll(contexts, mutated, hostCancellationToken)) {
                        // EvaluateAll has raised the event already, so reset the flag.
                        mutated = false;
                    } else if (mutated) {
                        // EvaluateAll did not raise the event, but we have a pending mutate to inform about.
                        Mutated?.Invoke(this, EventArgs.Empty);
                    }

                    if (ct.IsCancellationRequested) {
                        return;
                    }

                    var evaluationSource = await _pendingEvaluationSources.ReceiveAsync(ct);
                    mutated |= evaluationSource.IsMutating;
                    await evaluationSource.BeginEvaluationAsync(contexts, _host, hostCancellationToken);
                } catch (OperationCanceledException) {
                    return;
                }
            }
        }

        private async Task<bool> EvaluateAll(IReadOnlyList<IRContext> contexts, bool mutated, CancellationToken ct) {
            TaskUtilities.AssertIsOnBackgroundThread();

            try {
                RSessionEvaluationSource source;
                while (!ct.IsCancellationRequested && _pendingEvaluationSources.TryReceive(out source)) {
                    mutated |= source.IsMutating;
                    await source.BeginEvaluationAsync(contexts, _host, ct);
                }
            } catch (OperationCanceledException) {
                // Host is being shut down, so there's no point in raising the event anymore.
                mutated = false;
            } finally {
                if (mutated) {
                    Mutated?.Invoke(this, EventArgs.Empty);
                }
            }

            return mutated;
        }

        Task IRCallbacks.WriteConsoleEx(string buf, OutputType otype, CancellationToken ct) {
            Output?.Invoke(this, new ROutputEventArgs(otype, buf));

            if (otype == OutputType.Error) {
                var currentRequest = Interlocked.Exchange(ref _currentRequestSource, null);
                currentRequest?.Fail(buf);
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Displays error message
        /// </summary>
        Task IRCallbacks.ShowMessage(string message, CancellationToken ct) {
            return _hostClientApp?.ShowErrorMessage(message);
        }

        /// <summary>
        /// Called as a result of R calling R API 'YesNoCancel' callback
        /// </summary>
        /// <returns>Codes that match constants in RApi.h</returns>
        public async Task<YesNoCancel> YesNoCancel(IReadOnlyList<IRContext> contexts, string s, bool isEvaluationAllowed, CancellationToken ct) {

            MessageButtons buttons = await ((IRCallbacks)this).ShowDialog(contexts, s, isEvaluationAllowed, MessageButtons.YesNoCancel, ct);
            switch (buttons) {
                case MessageButtons.No:
                    return Client.YesNoCancel.No;
                case MessageButtons.Cancel:
                    return Client.YesNoCancel.Cancel;
            }
            return Client.YesNoCancel.Yes;
        }

        /// <summary>
        /// Called when R wants to display generic Windows MessageBox. 
        /// Graph app may call Win32 API directly rather than going via R API callbacks.
        /// </summary>
        /// <returns>Pressed button code</returns>
        async Task<MessageButtons> IRCallbacks.ShowDialog(IReadOnlyList<IRContext> contexts, string s, bool isEvaluationAllowed, MessageButtons buttons, CancellationToken ct) {
            await TaskUtilities.SwitchToBackgroundThread();

            if (isEvaluationAllowed) {
                await EvaluateAll(contexts, true, ct);
            } else {
                Mutated?.Invoke(this, EventArgs.Empty);
            }

            if (_hostClientApp != null) {
                return await _hostClientApp.ShowMessage(s, buttons);
            }

            return MessageButtons.OK;
        }

        Task IRCallbacks.Busy(bool which, CancellationToken ct) {
            return Task.CompletedTask;
        }

        Task IRCallbacks.Plot(string filePath, CancellationToken ct) {
            return _hostClientApp?.Plot(filePath, ct);
        }

        /// <summary>
        /// Asks VS to open specified URL in the help window browser
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        Task IRCallbacks.Browser(string url) {
            return _hostClientApp?.ShowHelp(url);
        }

        void IRCallbacks.DirectoryChanged() {
            DirectoryChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}