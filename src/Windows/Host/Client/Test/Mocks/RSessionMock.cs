// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core.Disposables;

namespace Microsoft.R.Host.Client.Test.Mocks {
    public sealed class RSessionMock : IRSession {
        private IRSessionInteraction _inter;

        public string LastExpression { get; private set; }

        public int Id { get; set; }
        public int? ProcessId { get; set; }
        public bool IsHostRunning { get; set; }
        public bool IsRemote { get; set; }
        public bool IsProcessing { get; set; }
        public bool IsReadingUserInput { get; set; }

        public bool RestartOnBrokerSwitch { get; set; }

        public Task HostStarted => IsHostRunning ? Task.FromResult(0) : Task.FromCanceled(new CancellationToken(true));

        public string Prompt { get; set; } = ">";

        public Task<ulong> CreateBlobAsync(CancellationToken ct = default(CancellationToken)) => 
            Task.FromResult(0ul);
        
        public Task DestroyBlobsAsync(IEnumerable<ulong> blobIds, CancellationToken ct = default(CancellationToken)) =>
            Task.CompletedTask;

        public Task<byte[]> BlobReadAllAsync(ulong blobId, CancellationToken cancellationToken = default(CancellationToken)) =>
            Task.FromResult(new byte[0]);

        public Task<byte[]> BlobReadAsync(ulong blobId, long position, long count, CancellationToken cancellationToken = default(CancellationToken)) =>
            Task.FromResult(new byte[0]);

        public Task<long> BlobWriteAsync(ulong blobId, byte[] data, long position, CancellationToken cancellationToken = default(CancellationToken)) =>
            Task.FromResult(0L);

        public Task<long> GetBlobSizeAsync(ulong blobId, CancellationToken cancellationToken = default(CancellationToken)) =>
            Task.FromResult(0L);

        public Task<long> SetBlobSizeAsync(ulong blobId, long size, CancellationToken cancellationToken = default(CancellationToken)) =>
            Task.FromResult(0L);

        public Task<REvaluationResult> EvaluateAsync(string expression, REvaluationKind kind, CancellationToken ct = default(CancellationToken)) {
            LastExpression = expression;
            if (kind.HasFlag(REvaluationKind.Mutating)) {
                Mutated?.Invoke(this, EventArgs.Empty);
            }
            return Task.FromResult(new REvaluationResult());
        }

        public Task<IRSessionInteraction> BeginInteractionAsync(bool isVisible = true, CancellationToken cancellationToken = default (CancellationToken)) {
            _inter = new RSessionInteractionMock();
            BeforeRequest?.Invoke(this, new RBeforeRequestEventArgs(_inter.Contexts, Prompt, 4096, addToHistoty: true));
            return Task.FromResult(_inter);
        }

        public Task CancelAllAsync(CancellationToken сancellationToken = default(CancellationToken)) {
            if (_inter != null) {
                AfterRequest?.Invoke(this, new RAfterRequestEventArgs(_inter.Contexts, Prompt, string.Empty, addToHistory: true, isVisible: true));
                _inter = null;
            }
            return Task.CompletedTask;
        }

        public void Dispose() {
            StopHostAsync(true).Wait(5000);
            Disposed?.Invoke(this, EventArgs.Empty);
        }

        public IDisposable DisableMutatedOnReadConsole() => Disposable.Empty;

        public void FlushLog() {
        }

        public Task StartHostAsync(RHostStartupInfo startupInfo, IRSessionCallback callback, int timeout = 3000, CancellationToken cancellationToken = default(CancellationToken)) {
            IsHostRunning = true;
            Connected?.Invoke(this, new RConnectedEventArgs(string.Empty));
            return Task.CompletedTask;
        }

        public Task EnsureHostStartedAsync(RHostStartupInfo startupInfo, IRSessionCallback callback, int timeout = 3000, CancellationToken cancellationToken = default(CancellationToken)) {
            IsHostRunning = true;
            Connected?.Invoke(this, new RConnectedEventArgs(string.Empty));
            return Task.CompletedTask;
        }

        public Task RestartHostAsync() {
            IsHostRunning = false;
            Disconnected?.Invoke(this, EventArgs.Empty);
            IsHostRunning = true;
            Connected?.Invoke(this, new RConnectedEventArgs(string.Empty));
            return Task.CompletedTask;
        }

        public Task StopHostAsync(bool waitForShutdown, CancellationToken cancellationToken = default(CancellationToken)) {
            IsHostRunning = false;
            Disconnected?.Invoke(this, EventArgs.Empty);
            return Task.CompletedTask;
        }

#pragma warning disable 67
        public event EventHandler<RAfterRequestEventArgs> AfterRequest;
        public event EventHandler<RBeforeRequestEventArgs> BeforeRequest;
        public event EventHandler<RConnectedEventArgs> Connected;
        public event EventHandler<EventArgs> Interactive;
        public event EventHandler<EventArgs> DirectoryChanged;
        public event EventHandler<EventArgs> Disconnected;
        public event EventHandler<EventArgs> Disposed;
        public event EventHandler<EventArgs> Mutated;
        public event EventHandler<ROutputEventArgs> Output;
        public event EventHandler<EventArgs> PackagesInstalled;
        public event EventHandler<EventArgs> PackagesRemoved;
    }
}
