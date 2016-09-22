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
using Microsoft.Common.Core.Tasks;
using Microsoft.Common.Core.Threading;
using Microsoft.R.Host.Client.Host;
using static System.FormattableString;
using Task = System.Threading.Tasks.Task;

namespace Microsoft.R.Host.Client.Session {
    internal sealed class RSession : IRSession, IRCallbacks {
        private static readonly string DefaultPrompt = "> ";
        private static readonly Task<IRSessionEvaluation> CanceledBeginEvaluationTask;
        private static readonly Task<IRSessionInteraction> CanceledBeginInteractionTask;

        private readonly BufferBlock<RSessionRequestSource> _pendingRequestSources = new BufferBlock<RSessionRequestSource>();
        private readonly BufferBlock<RSessionEvaluationSource> _pendingEvaluationSources = new BufferBlock<RSessionEvaluationSource>();

        public event EventHandler<RBeforeRequestEventArgs> BeforeRequest;
        public event EventHandler<RAfterRequestEventArgs> AfterRequest;
        public event EventHandler<EventArgs> Mutated;
        public event EventHandler<ROutputEventArgs> Output;
        public event EventHandler<RConnectedEventArgs> Connected;
        public event EventHandler<EventArgs> Disconnected;
        public event EventHandler<EventArgs> Disposed;
        public event EventHandler<EventArgs> DirectoryChanged;
        public event EventHandler<EventArgs> PackagesInstalled;
        public event EventHandler<EventArgs> PackagesRemoved;

        /// <summary>
        /// ReadConsole requires a task even if there are no pending requests
        /// </summary>
        private IReadOnlyList<IRContext> _contexts;
        private RHost _host;
        private Task _hostRunTask;
        private Task _afterHostStartedTask;
        private TaskCompletionSourceEx<object> _initializationTcs;
        private RSessionRequestSource _currentRequestSource;
        private readonly BinaryAsyncLock _initializationLock;
        private readonly Action _onDispose;
        private readonly CountdownDisposable _disableMutatingOnReadConsole;
        private readonly DisposeToken _disposeToken;
        private volatile bool _isHostRunning;
        private volatile bool _delayedMutatedOnReadConsole;
        private volatile IRSessionCallback _callback;
        private volatile RHostStartupInfo _startupInfo;

        public int Id { get; }
        internal IBrokerClient BrokerClient { get; }
        public string Prompt { get; private set; } = DefaultPrompt;
        public int MaxLength { get; private set; } = 0x1000;
        public bool IsHostRunning => _isHostRunning;
        public Task HostStarted => _initializationTcs.Task;
        public bool IsRemote => BrokerClient.IsRemote;

        /// <summary>
        /// For testing purpose only
        /// Do not expose this property to the IRSession interface
        /// </summary> 
        internal RHost RHost => _host;

        static RSession() {
            CanceledBeginEvaluationTask = TaskUtilities.CreateCanceled<IRSessionEvaluation>(new RHostDisconnectedException());
            CanceledBeginInteractionTask = TaskUtilities.CreateCanceled<IRSessionInteraction>(new RHostDisconnectedException());
        }

        public RSession(int id, IBrokerClient brokerClient, Action onDispose) {
            Id = id;
            BrokerClient = brokerClient;
            _onDispose = onDispose;
            _disposeToken = DisposeToken.Create<RSession>();
            _disableMutatingOnReadConsole = new CountdownDisposable(() => {
                if (!_delayedMutatedOnReadConsole) {
                    return;
                }

                _delayedMutatedOnReadConsole = false;
                Task.Run(() => Mutated?.Invoke(this, EventArgs.Empty));
            });

            _initializationLock = new BinaryAsyncLock();
            _initializationTcs = new TaskCompletionSourceEx<object>();
            _afterHostStartedTask = TaskUtilities.CreateCanceled(new RHostDisconnectedException());
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
            _disposeToken.ThrowIfDisposed();

            if (!_isHostRunning) {
                return CanceledBeginInteractionTask;
            }

            RSessionRequestSource requestSource = new RSessionRequestSource(isVisible, cancellationToken);
            _pendingRequestSources.Post(requestSource);

            return _isHostRunning ? requestSource.CreateRequestTask : CanceledBeginInteractionTask;
        }

        public Task<IRSessionEvaluation> BeginEvaluationAsync(CancellationToken cancellationToken = default(CancellationToken)) {
            _disposeToken.ThrowIfDisposed();

            if (!_isHostRunning) {
                return CanceledBeginEvaluationTask;
            }

            var source = new RSessionEvaluationSource(cancellationToken);
            _pendingEvaluationSources.Post(source);

            return _isHostRunning ? source.Task : CanceledBeginEvaluationTask;
        }

        public async Task<REvaluationResult> EvaluateAsync(string expression, REvaluationKind kind = REvaluationKind.Normal, CancellationToken ct = default(CancellationToken)) {
            if (!IsHostRunning) {
                throw new RHostDisconnectedException();
            }

            await _afterHostStartedTask;

            try {
                var result = await _host.EvaluateAsync(expression, kind, ct);
                if (kind.HasFlag(REvaluationKind.Mutating)) {
                    OnMutated();
                }
                return result;
            } catch (MessageTransportException) when (!IsHostRunning) {
                throw new RHostDisconnectedException();
            }
        }


        public Task<ulong> CreateBlobAsync(CancellationToken ct = default(CancellationToken)) =>
            DoBlobServiceAsync(_host?.CreateBlobAsync(ct));

        public Task DestroyBlobsAsync(IEnumerable<ulong> blobIds, CancellationToken ct = default(CancellationToken)) => 
            DoBlobServiceAsync(new Lazy<Task<long>>(async () => {
                await _host.DestroyBlobsAsync(blobIds, ct);
                return 0;
            }).Value);

        public Task<byte[]> BlobReadAllAsync(ulong blobId, CancellationToken ct = default(CancellationToken)) =>
            DoBlobServiceAsync(_host?.BlobReadAllAsync(blobId, ct));

        public Task<byte[]> BlobReadAsync(ulong blobId, long position, long count, CancellationToken ct = default(CancellationToken)) =>
            DoBlobServiceAsync(_host?.BlobReadAsync(blobId, position, count, ct));

        public Task<long> BlobWriteAsync(ulong blobId, byte[] data, long position, CancellationToken ct = default(CancellationToken)) =>
            DoBlobServiceAsync(_host?.BlobWriteAsync(blobId, data, position, ct));

        public Task<long> GetBlobSizeAsync(ulong blobId, CancellationToken ct = default(CancellationToken)) =>
            DoBlobServiceAsync(_host?.GetBlobSizeAsync(blobId, ct));

        public Task<long> SetBlobSizeAsync(ulong blobId, long size, CancellationToken ct = default(CancellationToken)) =>
            DoBlobServiceAsync(_host?.SetBlobSizeAsync(blobId, size, ct));
        
        private async Task<T> DoBlobServiceAsync<T>(Task<T> work) {
            if (!IsHostRunning) {
                throw new RHostDisconnectedException();
            }

            await _afterHostStartedTask;

            try {
                return await work;
            } catch (MessageTransportException) when (!IsHostRunning) {
                throw new RHostDisconnectedException();
            }
        }

        public async Task CancelAllAsync(CancellationToken cancellationToken = default(CancellationToken)) {
            _disposeToken.ThrowIfDisposed();

            var cancelTask = _host.CancelAllAsync(cancellationToken);

            var currentRequest = Interlocked.Exchange(ref _currentRequestSource, null);
            var exception = new OperationCanceledException();
            currentRequest?.TryCancel(exception);
            ClearPendingRequests(exception);

            await cancelTask;
        }

        public async Task EnsureHostStartedAsync(RHostStartupInfo startupInfo, IRSessionCallback callback, int timeout = 3000) {
            _disposeToken.ThrowIfDisposed();
            var lockToken = await _initializationLock.WaitAsync();
            if (!lockToken.IsSet) {
                await StartHostAsyncBackground(startupInfo, callback, lockToken, timeout);
            }
        }

        public async Task StartHostAsync(RHostStartupInfo startupInfo, IRSessionCallback callback, int timeout = 3000) {
            _disposeToken.ThrowIfDisposed();
            var isStartedTask = _initializationLock.WaitAsync();
            if (isStartedTask.IsCompleted && !isStartedTask.Result.IsSet) {
                var lockToken = isStartedTask.Result;
                await StartHostAsyncBackground(startupInfo, callback, lockToken, timeout);
            } else {
                throw new InvalidOperationException("Another instance of RHost is running for this RSession. Stop it before starting new one.");
            }
        }

        private async Task StartHostAsyncBackground(RHostStartupInfo startupInfo, IRSessionCallback callback, IBinaryAsyncLockToken lockToken, int timeout) {
            await TaskUtilities.SwitchToBackgroundThread();

            _callback = callback;
            _startupInfo = startupInfo;
            RHost host;
            try {
                host = await BrokerClient.ConnectAsync(startupInfo.Name, this, startupInfo.RHostCommandLineArguments, timeout);
            } catch (OperationCanceledException ex) {
                _initializationTcs.TrySetCanceled(ex);
                lockToken.Reset();
                throw;
            } catch (Exception ex) {
                _initializationTcs.TrySetException(ex);
                lockToken.Reset();
                throw;
            }

            await StartHostAsyncBackground(host, lockToken);
        }

        private async Task StartHostAsyncBackground(RHost host, IBinaryAsyncLockToken lockToken, CancellationToken cancellationToken = default(CancellationToken)) {
            await TaskUtilities.SwitchToBackgroundThread();

            ResetInitializationTcs();
            ClearPendingRequests(new RHostDisconnectedException());

            Interlocked.Exchange(ref _host, host);
            var initializationCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            var hostRunTask = RunHost(lockToken, initializationCts.Token);
            Interlocked.Exchange(ref _hostRunTask, hostRunTask)?.DoNotWait();

            await _initializationTcs.Task;
            initializationCts.Dispose();
            lockToken.Set();
        }

        public async Task<IRSessionSwitchBrokerTransaction> StartSwitchingBrokerAsync(CancellationToken cancellationToken) {
            _disposeToken.ThrowIfDisposed();
            // reset and acquire _initializationLock, but don't interrupt existing initialization
            var lockToken = await _initializationLock.ResetAsync(cancellationToken);
            return new SwitchBrokerTransaction(this, lockToken, cancellationToken);
        }

        public async Task<IRSessionReconnectTransaction> StartReconnectingAsync(CancellationToken cancellationToken) {
            _disposeToken.ThrowIfDisposed();
            // reset and acquire _initializationLock, but don't interrupt existing initialization
            var lockToken = await _initializationLock.ResetAsync(cancellationToken);
            return new SwitchBrokerTransaction(this, lockToken, cancellationToken);
        }

        public async Task StopHostAsync() {
            _disposeToken.ThrowIfDisposed();
            await TaskUtilities.SwitchToBackgroundThread();

            var lockToken = await _initializationLock.WaitAsync();

            // Host wasn't started yet or host is already stopped - nothing to stop, just pass acquired lock to the next awaiter
            if (!lockToken.IsSet) {
                lockToken.Reset();
                return;
            }

            ResetInitializationTcs();

            Task<IRSessionInteraction> requestTask;
            try {
                requestTask = BeginInteractionAsync(false);
                await Task.WhenAny(requestTask, Task.Delay(200)).Unwrap();
            } catch (RHostDisconnectedException) {
                // BeginInteractionAsync will fail with RHostDisconnectedException if RHost isn't running. Nothing to stop.
                return;
            }

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
                    } catch (Exception) { }

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

        private async Task RunHost(IBinaryAsyncLockToken lockToken, CancellationToken initializationCt) {
            try {
                ScheduleAfterHostStarted(_startupInfo);
                await _host.Run(initializationCt);
            } catch (OperationCanceledException oce) {
                _initializationTcs.TrySetCanceled(oce);
            } catch (MessageTransportException mte) {
                _initializationTcs.TrySetCanceled(new RHostDisconnectedException(string.Empty, mte));
            } catch (Exception ex) {
                _initializationTcs.TrySetException(ex);
            } finally {
                lockToken.Reset();
            }
        }

        private void ResetInitializationTcs() {
            while (true) {
                var tcs = _initializationTcs;
                if (!tcs.Task.IsCompleted) {
                    return;
                }

                if (Interlocked.CompareExchange(ref _initializationTcs, new TaskCompletionSourceEx<object>(), tcs) == tcs) {
                    return;
                }
            }
        }

        private void ScheduleAfterHostStarted(RHostStartupInfo startupInfo) {
            var afterHostStartedEvaluationSource = new RSessionEvaluationSource(CancellationToken.None);
            _pendingEvaluationSources.Post(afterHostStartedEvaluationSource);
            Interlocked.Exchange(ref _afterHostStartedTask, AfterHostStarted(afterHostStartedEvaluationSource, startupInfo));
        }

        private async Task AfterHostStarted(RSessionEvaluationSource evaluationSource, RHostStartupInfo startupInfo) {
            using (var evaluation = await evaluationSource.Task) {
                // Load RTVS R package before doing anything in R since the calls
                // below calls may depend on functions exposed from the RTVS package
                var libPath = IsRemote ? "." : Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetAssemblyPath());

                await LoadRtvsPackage(evaluation, libPath);

                if (!IsRemote && startupInfo.WorkingDirectory != null) {
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
            }
        }

        private static async Task LoadRtvsPackage(IRSessionEvaluation eval, string libPath) {
            await eval.ExecuteAsync(Invariant($"base::loadNamespace('rtvs', lib.loc = {libPath.ToRStringLiteral()})"));
        }

        public void FlushLog() {
            _host?.FlushLog();
        }

        Task IRCallbacks.Connected(string rVersion) {
            Prompt = DefaultPrompt;
            _isHostRunning = true;
            _initializationTcs.SetResult(null);
            Connected?.Invoke(this, new RConnectedEventArgs(rVersion));
            Mutated?.Invoke(this, EventArgs.Empty);
            return Task.CompletedTask;
        }

        async Task IRCallbacks.Disconnected() {
            _isHostRunning = false;
            var lockToken = await _initializationLock.ResetAsync();
            Disconnected?.Invoke(this, EventArgs.Empty);

            var currentRequest = Interlocked.Exchange(ref _currentRequestSource, null);
            var exception = new RHostDisconnectedException();
            currentRequest?.TryCancel(exception);

            ClearPendingRequests(exception);
            lockToken.Reset();
        }

        private void ClearPendingRequests(OperationCanceledException exception) {
            RSessionRequestSource requestSource;
            while (_pendingRequestSources.TryReceive(out requestSource)) {
                requestSource.TryCancel(exception);
            }

            RSessionEvaluationSource evalSource;
            while (_pendingEvaluationSources.TryReceive(out evalSource)) {
                evalSource.TryCancel(exception);
            }

            _contexts = null;
            Prompt = DefaultPrompt;
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

            var requestEventArgs = new RBeforeRequestEventArgs(contexts, prompt, len, addToHistory);
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

            AfterRequest?.Invoke(this, new RAfterRequestEventArgs(contexts, prompt, consoleInput, addToHistory, _currentRequestSource.IsVisible));

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

        private async Task<bool> EvaluateAll(IReadOnlyList<IRContext> contexts, bool mutated, CancellationToken hostCancellationToken) {
            TaskUtilities.AssertIsOnBackgroundThread();

            try {
                RSessionEvaluationSource source;
                while (!hostCancellationToken.IsCancellationRequested && _pendingEvaluationSources.TryReceive(out source)) {
                    mutated |= await source.BeginEvaluationAsync(contexts, _host, hostCancellationToken);
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
        async Task<MessageButtons> IRCallbacks.ShowDialog(IReadOnlyList<IRContext> contexts, string s, MessageButtons buttons, CancellationToken hostCancellationToken) {
            await TaskUtilities.SwitchToBackgroundThread();

            await EvaluateAll(contexts, true, hostCancellationToken);

            var callback = _callback;
            if (callback != null) {
                return await callback.ShowMessage(s, buttons);
            }

            return MessageButtons.OK;
        }

        Task IRCallbacks.Busy(bool which, CancellationToken ct) {
            return Task.CompletedTask;
        }

        Task IRCallbacks.Plot(PlotMessage plot, CancellationToken ct) {
            var callback = _callback;
            return callback != null ? callback.Plot(plot, ct) : Task.CompletedTask;
        }

        Task<LocatorResult> IRCallbacks.Locator(Guid deviceId, CancellationToken ct) {
            var callback = _callback;
            return callback != null ? callback.Locator(deviceId, ct) : Task.FromResult(LocatorResult.CreateNotClicked());
        }

        Task<PlotDeviceProperties> IRCallbacks.PlotDeviceCreate(Guid deviceId, CancellationToken ct) {
            var callback = _callback;
            return callback != null ? callback.PlotDeviceCreate(deviceId, ct) : Task.FromResult(PlotDeviceProperties.Default);
        }

        Task IRCallbacks.PlotDeviceDestroy(Guid deviceId, CancellationToken ct) {
            var callback = _callback;
            return callback != null ? callback.PlotDeviceDestroy(deviceId, ct) : Task.CompletedTask;
        }

        /// <summary>
        /// Asks VS to open specified URL in the help window browser
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        Task IRCallbacks.WebBrowser(string url) {
            var newUrl = BrokerClient.HandleUrl(url, CancellationToken.None);
            var callback = _callback;
            return callback != null ? callback.ShowHelp(newUrl) : Task.CompletedTask;
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

        void IRCallbacks.PackagesInstalled() {
            PackagesInstalled?.Invoke(this, EventArgs.Empty);
        }

        void IRCallbacks.PackagesRemoved() {
            PackagesRemoved?.Invoke(this, EventArgs.Empty);
        }

        Task<string> IRCallbacks.SaveFileAsync(string filename, byte[] data) {
            var callback = _callback;
            return callback != null ? callback.SaveFileAsync(filename, data) : Task.FromResult(string.Empty);
        }
        
        private class SwitchBrokerTransaction : IRSessionSwitchBrokerTransaction, IRSessionReconnectTransaction {
            private readonly RSession _session;
            private readonly IBinaryAsyncLockToken _lockToken;
            private readonly CancellationToken _cancellationToken;
            private RHost _hostToSwitch;

            public SwitchBrokerTransaction(RSession session, IBinaryAsyncLockToken lockToken, CancellationToken cancellationToken) {
                _session = session;
                _lockToken = lockToken;
                _cancellationToken = cancellationToken;
            }

            public async Task ConnectToNewBrokerAsync() {
                _session._disposeToken.ThrowIfDisposed();
                var startupInfo = _session._startupInfo;
                if (startupInfo != null) {
                    // host requires _startupInfo, so proceed only if session was started at least once
                    _hostToSwitch = await _session.BrokerClient.ConnectAsync(startupInfo.Name, _session, startupInfo.RHostCommandLineArguments, cancellationToken: _cancellationToken);
                }
            }

            public async Task CompleteSwitchingBrokerAsync() {
                _session._disposeToken.ThrowIfDisposed();

                if (_session._startupInfo == null) {
                    // Session never started. No need to restart it.
                    // Reset _initializationLock so that next awaiter can proceed.
                    _lockToken.Reset();
                    throw new RHostDisconnectedException(Resources.RHostDisconnected);
                }

                var host = _session._host;
                var hostRunTask = _session._hostRunTask;

                // host may be null if previous attempts to start it have failed
                if (host != null) {
                    // Detach RHost from RSession
                    host.DetachCallback();

                    // Cancel all current requests
                    await _session.CancelAllAsync(_cancellationToken);
                }

                // Start new RHost
                await _session.StartHostAsyncBackground(_hostToSwitch, _lockToken, _cancellationToken);

                // Don't send stop notification to broker - just dispose host and wait for old hostRunTask to exit;
                host?.Dispose();
                if (hostRunTask != null) {
                    await hostRunTask;
                }

                _hostToSwitch = null;
            }

            public async Task ReconnectAsync() {
                _session._disposeToken.ThrowIfDisposed();

                if (_session._startupInfo == null) {
                    // Session never started. No need to restart it.
                    // Reset _initializationLock so that next awaiter can proceed.
                    _lockToken.Reset();
                    return;
                }

                var host = _session._host;
                // host may be null if previous attempts to start it have failed
                if (host != null) {
                    // Detach RHost from RSession
                    host.DetachCallback();

                    // Cancel all current requests (if any)
                    await _session.CancelAllAsync(_cancellationToken);

                    host.Dispose();
                    await _session._hostRunTask;
                }

                host = await _session.BrokerClient.ConnectAsync(_session._startupInfo.Name, _session, _session._startupInfo.RHostCommandLineArguments, cancellationToken: _cancellationToken);
                await _session.StartHostAsyncBackground(host, _lockToken, _cancellationToken);
            }

            public void Dispose() {
                _lockToken.Reset();
                _hostToSwitch?.Dispose();
            }
        }
    }
}