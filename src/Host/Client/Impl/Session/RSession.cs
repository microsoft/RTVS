// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Disposables;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Host.Client.Install;
using static System.FormattableString;
using Task = System.Threading.Tasks.Task;

namespace Microsoft.R.Host.Client.Session {
    internal sealed class RSession : IRSession, IRCallbacks {
        private readonly static string DefaultPrompt = "> ";
        private readonly static Task<IRSessionEvaluation> CanceledBeginEvaluationTask;
        private readonly static Task<IRSessionInteraction> CanceledBeginInteractionTask;
        private readonly static Task<REvaluationResult> CanceledEvaluateTask;

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
        private TaskCompletionSource<object> _afterHostStartedTcs;
        private TaskCompletionSource<object> _initializationTcs;
        private RSessionRequestSource _currentRequestSource;
        private readonly Action _onDispose;
        private readonly CountdownDisposable _disableMutatingOnReadConsole;
        private readonly DisposeToken _disposeToken;
        private volatile bool _isHostRunning;
        private volatile bool _delayedMutatedOnReadConsole;
        private volatile IRSessionCallback _callback;

        public int Id { get; }
        public string Prompt { get; private set; } = DefaultPrompt;
        public int MaxLength { get; private set; } = 0x1000;
        public bool IsHostRunning => _isHostRunning;
        public Task HostStarted => _initializationTcs?.Task ?? Task.FromCanceled(new CancellationToken(true));

        public int? ProcessId => _host?.ProcessId;

        /// <summary>
        /// For testing purpose only
        /// Do not expose this property to the IRSession interface
        /// </summary>
        internal RHost RHost => _host;

        static RSession() {
            var tcs = new CancellationTokenSource();
            tcs.Cancel();
            CanceledBeginEvaluationTask = Task.FromCanceled<IRSessionEvaluation>(tcs.Token);
            CanceledBeginInteractionTask = Task.FromCanceled<IRSessionInteraction>(tcs.Token);
            CanceledEvaluateTask = Task.FromCanceled<REvaluationResult>(tcs.Token);
        }

        public RSession(int id, Action onDispose) {
            Id = id;
            _onDispose = onDispose;
            _disposeToken = DisposeToken.Create<RSession>();
            _disableMutatingOnReadConsole = new CountdownDisposable(() => {
                if (!_delayedMutatedOnReadConsole) {
                    return;
                }

                _delayedMutatedOnReadConsole = false;
                Task.Run(() => Mutated?.Invoke(this, EventArgs.Empty));
            });
        }

        private void OnMutated() {
            if (_disableMutatingOnReadConsole.Count == 0) {
                Mutated?.Invoke(this, EventArgs.Empty);
            } else {
                _delayedMutatedOnReadConsole = true;
            }
        }

        public void Dispose() {
            if (!_disposeToken.TryMarkDisposed()) {
                return;
            }

            _host?.Dispose();
            Disposed?.Invoke(this, EventArgs.Empty);
            _onDispose();
        }

        public Task<IRSessionInteraction> BeginInteractionAsync(bool isVisible = true, CancellationToken cancellationToken = default(CancellationToken)) {
            if (!_isHostRunning) {
                return CanceledBeginInteractionTask;
            }

            RSessionRequestSource requestSource = new RSessionRequestSource(isVisible, cancellationToken);
            _pendingRequestSources.Post(requestSource);

            return _isHostRunning ? requestSource.CreateRequestTask : CanceledBeginInteractionTask;
        }

        public Task<IRSessionEvaluation> BeginEvaluationAsync(CancellationToken cancellationToken = default(CancellationToken)) {
            if (!_isHostRunning) {
                return CanceledBeginEvaluationTask;
            }

            var source = new RSessionEvaluationSource(cancellationToken);
            _pendingEvaluationSources.Post(source);

            return _isHostRunning ? source.Task : CanceledBeginEvaluationTask;
        }

        public async Task<REvaluationResult> EvaluateAsync(string expression, REvaluationKind kind = REvaluationKind.Normal, CancellationToken ct = default(CancellationToken)) {
            if (!IsHostRunning) {
                return await CanceledEvaluateTask;
            }

            await _afterHostStartedTcs.Task;

            try {
                var result = await _host.EvaluateAsync(expression, kind, ct);
                if (kind.HasFlag(REvaluationKind.Mutating)) {
                    OnMutated();
                }
                return result;
            } catch (MessageTransportException) when (!IsHostRunning) {
                return await CanceledEvaluateTask;
            }
        }

        public async Task CancelAllAsync() {
            var cancelTask = _host.CancelAllAsync();

            var currentRequest = Interlocked.Exchange(ref _currentRequestSource, null);
            currentRequest?.Cancel();
            ClearPendingRequests();

            await cancelTask;
        }

        public async Task EnsureHostStartedAsync(RHostStartupInfo startupInfo, IRSessionCallback callback, int timeout = 3000) {
            var existingInitializationTcs = Interlocked.CompareExchange(ref _initializationTcs, new TaskCompletionSource<object>(), null);
            if (existingInitializationTcs == null) {
                await StartHostAsyncBackground(startupInfo, callback, timeout);
            } else {
                await existingInitializationTcs.Task;
            }
        }

        public async Task StartHostAsync(RHostStartupInfo startupInfo, IRSessionCallback callback, int timeout = 3000) {
            if (Interlocked.CompareExchange(ref _initializationTcs, new TaskCompletionSource<object>(), null) != null) {
                throw new InvalidOperationException("Another instance of RHost is running for this RSession. Stop it before starting new one.");
            }

            Interlocked.Exchange(ref _afterHostStartedTcs, new TaskCompletionSource<object>());

            await StartHostAsyncBackground(startupInfo, callback, timeout);
        }

        private async Task StartHostAsyncBackground(RHostStartupInfo startupInfo, IRSessionCallback callback, int timeout) {
            await TaskUtilities.SwitchToBackgroundThread();

            _callback = callback;
            _host = new RHost(startupInfo != null ? startupInfo.Name : "Empty", this);
            ClearPendingRequests();

            var initializationTask = _initializationTcs.Task;
            _hostRunTask = CreateAndRunHost(startupInfo, timeout);

            ScheduleAfterHostStarted(startupInfo);

            await initializationTask;
        }

        public async Task StopHostAsync() {
            if (_initializationTcs == null) {
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
                        await Task.WhenAny(_hostRunTask, inter.QuitAsync(), Task.Delay(500)).Unwrap();
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

        public IDisposable DisableMutatedOnReadConsole() {
            return _disableMutatingOnReadConsole.Increment();
        }

        private async Task CreateAndRunHost(RHostStartupInfo startupInfo, int timeout) {
            try {
                await _host.CreateAndRun(RInstallation.GetRInstallPath(startupInfo.RBasePath, new SupportedRVersionRange()), startupInfo.RHostDirectory, startupInfo.RHostCommandLineArguments, timeout);
            } catch (OperationCanceledException oce) {
                _initializationTcs.TrySetCanceled(oce.CancellationToken);
            } catch (Exception ex) {
                _initializationTcs.TrySetException(ex);
            } finally {
                Interlocked.Exchange(ref _initializationTcs, null);
            }
        }

        private void ScheduleAfterHostStarted(RHostStartupInfo startupInfo) {
            var afterHostStartedEvaluationSource = new RSessionEvaluationSource(CancellationToken.None);
            _pendingEvaluationSources.Post(afterHostStartedEvaluationSource);
            AfterHostStarted(afterHostStartedEvaluationSource, startupInfo).DoNotWait();
        }

        private async Task AfterHostStarted(RSessionEvaluationSource evaluationSource, RHostStartupInfo startupInfo) {
            try {
                using (var evaluation = await evaluationSource.Task) {
                    // Load RTVS R package before doing anything in R since the calls
                    // below calls may depend on functions exposed from the RTVS package
                    await LoadRtvsPackage(evaluation);
                    if (startupInfo.WorkingDirectory != null) {
                        await evaluation.SetWorkingDirectoryAsync(startupInfo.WorkingDirectory);
                    } else {
                        await evaluation.SetDefaultWorkingDirectoryAsync();
                    }

                    var callback = _callback;
                    if (callback != null) {
                        await evaluation.SetVsGraphicsDeviceAsync();

                        string mirrorUrl = callback.CranUrlFromName(startupInfo.CranMirrorName);
                        await evaluation.SetVsCranSelectionAsync(mirrorUrl);

                        await evaluation.SetCodePageAsync(startupInfo.CodePage);
                        await evaluation.SetROptionsAsync();
                        await evaluation.OverrideFunctionAsync("setwd", "base");
                        await evaluation.SetFunctionRedirectionAsync();
                        await evaluation.OptionsSetWidthAsync(startupInfo.TerminalWidth);
                    }

                    _afterHostStartedTcs.SetResult(null);
                }
            } catch (OperationCanceledException oce) {
                _afterHostStartedTcs.TrySetCanceled(oce.CancellationToken);
            } catch (Exception ex) {
                _afterHostStartedTcs.TrySetException(ex);
            }
        }

        private static async Task LoadRtvsPackage(IRSessionEvaluation eval) {
            var libPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetAssemblyPath());
            await eval.ExecuteAsync(Invariant($"base::loadNamespace('rtvs', lib.loc = {libPath.ToRStringLiteral()})"));
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
            currentRequest?.CompleteResponse();

            ClearPendingRequests();

            return Task.CompletedTask;
        }

        private void ClearPendingRequests() {
            RSessionRequestSource requestSource;
            while (_pendingRequestSources.TryReceive(out requestSource)) {
                requestSource.Cancel();
            }

            RSessionEvaluationSource evalSource;
            while (_pendingEvaluationSources.TryReceive(out evalSource)) {
                evalSource.TryCancel();
            }

            _contexts = null;
            Prompt = string.Empty;
        }

        async Task<string> IRCallbacks.ReadConsole(IReadOnlyList<IRContext> contexts, string prompt, int len, bool addToHistory, CancellationToken ct) {
            await TaskUtilities.SwitchToBackgroundThread();

            var callback = _callback;
            if (!addToHistory && callback != null) {
                return await callback.ReadUserInput(prompt, len, ct);
            }

            var currentRequest = Interlocked.Exchange(ref _currentRequestSource, null);

            _contexts = contexts;
            Prompt = prompt;
            MaxLength = len;

            var requestEventArgs = new RRequestEventArgs(contexts, prompt, len, addToHistory);
            BeforeRequest?.Invoke(this, requestEventArgs);

            var evaluationCts = new CancellationTokenSource();
            var evaluationTask = EvaluateUntilCancelled(contexts, evaluationCts.Token, ct);

            currentRequest?.CompleteResponse();

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

            consoleInput = consoleInput.EnsureLineBreak();

            // If evaluation was allowed, cancel evaluation processing but await evaluation that is in progress
            evaluationCts.Cancel();
            await evaluationTask;

            AfterRequest?.Invoke(this, requestEventArgs);

            return consoleInput;
        }

        private async Task<string> ReadNextRequest(string prompt, int len, CancellationToken ct) {
            TaskUtilities.AssertIsOnBackgroundThread();

            var requestSource = await _pendingRequestSources.ReceiveAsync(ct);
            TaskCompletionSource<string> requestTcs = new TaskCompletionSource<string>();
            Interlocked.Exchange(ref _currentRequestSource, requestSource);

            requestSource.Request(_contexts, prompt, len, requestTcs);
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
                        OnMutated();
                    }

                    if (ct.IsCancellationRequested) {
                        return;
                    }

                    var evaluationSource = await _pendingEvaluationSources.ReceiveAsync(ct);
                    mutated |= await evaluationSource.BeginEvaluationAsync(contexts, _host, hostCancellationToken);
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
                    mutated |= await source.BeginEvaluationAsync(contexts, _host, ct);
                }
            } catch (OperationCanceledException) {
                // Host is being shut down, so there's no point in raising the event anymore.
                mutated = false;
            } finally {
                if (mutated) {
                    OnMutated();
                }
            }

            return mutated;
        }

        Task IRCallbacks.WriteConsoleEx(string buf, OutputType otype, CancellationToken ct) {
            Output?.Invoke(this, new ROutputEventArgs(otype, buf));
            return Task.CompletedTask;
        }

        /// <summary>
        /// Displays error message
        /// </summary>
        Task IRCallbacks.ShowMessage(string message, CancellationToken ct) {
            var callback = _callback;
            return callback != null ? callback.ShowErrorMessage(message) : Task.CompletedTask;
        }

        /// <summary>
        /// Called as a result of R calling R API 'YesNoCancel' callback
        /// </summary>
        /// <returns>Codes that match constants in RApi.h</returns>
        public async Task<YesNoCancel> YesNoCancel(IReadOnlyList<IRContext> contexts, string s, CancellationToken ct) {

            MessageButtons buttons = await ((IRCallbacks)this).ShowDialog(contexts, s, MessageButtons.YesNoCancel, ct);
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
        async Task<MessageButtons> IRCallbacks.ShowDialog(IReadOnlyList<IRContext> contexts, string s, MessageButtons buttons, CancellationToken ct) {
            await TaskUtilities.SwitchToBackgroundThread();

            await EvaluateAll(contexts, true, ct);

            var callback = _callback;
            if (callback != null) {
                return await callback.ShowMessage(s, buttons);
            }

            return MessageButtons.OK;
        }

        Task IRCallbacks.Busy(bool which, CancellationToken ct) {
            return Task.CompletedTask;
        }

        Task IRCallbacks.Plot(PlotMessage plot, CancellationToken ct)
        {
            var callback = _callback;
            return callback != null ? callback.Plot(plot, ct) : Task.CompletedTask;
        }

        Task<LocatorResult> IRCallbacks.Locator(Guid deviceId, CancellationToken ct)
        {
            var callback = _callback;
            return callback != null ? callback.Locator(deviceId, ct) : Task.FromResult(LocatorResult.CreateNotClicked());
        }

        Task<PlotDeviceProperties> IRCallbacks.PlotDeviceCreate(Guid deviceId, CancellationToken ct)
        {
            var callback = _callback;
            return callback != null ? callback.PlotDeviceCreate(deviceId, ct) : Task.FromResult(PlotDeviceProperties.Default);
        }

        Task IRCallbacks.PlotDeviceDestroy(Guid deviceId, CancellationToken ct)
        {
            var callback = _callback;
            return callback != null ? callback.PlotDeviceDestroy(deviceId, ct) : Task.CompletedTask;
        }

        /// <summary>
        /// Asks VS to open specified URL in the help window browser
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        Task IRCallbacks.Browser(string url) {
            var callback = _callback;
            return callback != null ? callback.ShowHelp(url) : Task.CompletedTask;
        }

        Task IRCallbacks.ViewLibrary() {
            var callback = _callback;
            return callback?.ViewLibrary();
        }

        Task IRCallbacks.ShowFile(string fileName, string tabName, bool deleteFile) {
            var callback = _callback;
            return callback?.ViewFile(fileName, tabName, deleteFile);
        }

        void IRCallbacks.DirectoryChanged() {
            DirectoryChanged?.Invoke(this, EventArgs.Empty);
        }

        void IRCallbacks.ViewObject(string obj, string title) {
            var callback = _callback;
            callback?.ViewObject(obj, title);
        }
    }
}