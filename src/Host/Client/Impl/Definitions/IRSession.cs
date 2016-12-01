// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.R.Host.Client {
    public interface IRSession : IRExpressionEvaluator, IRBlobService, IDisposable {
        event EventHandler<RBeforeRequestEventArgs> BeforeRequest;
        event EventHandler<RAfterRequestEventArgs> AfterRequest;
        event EventHandler<EventArgs> Mutated;
        event EventHandler<ROutputEventArgs> Output;
        event EventHandler<RConnectedEventArgs> Connected;
        event EventHandler<EventArgs> Interactive;
        event EventHandler<EventArgs> Disconnected;
        event EventHandler<EventArgs> Disposed;
        event EventHandler<EventArgs> DirectoryChanged;
        event EventHandler<EventArgs> PackagesInstalled;
        event EventHandler<EventArgs> PackagesRemoved;

        int Id { get; }
        string Prompt { get; }
        bool IsHostRunning { get; }
        Task HostStarted { get; }
        bool IsRemote { get; }
        bool RestartOnBrokerSwitch { get; set; }

        Task<IRSessionInteraction> BeginInteractionAsync(bool isVisible = true, CancellationToken cancellationToken = default(CancellationToken));
        Task<IRSessionEvaluation> BeginEvaluationAsync(CancellationToken cancellationToken = default(CancellationToken));
        Task CancelAllAsync(CancellationToken cancellationToken = default(CancellationToken));
        Task StartHostAsync(RHostStartupInfo startupInfo, IRSessionCallback callback, int timeout = 3000, CancellationToken cancellationToken = default(CancellationToken));
        Task EnsureHostStartedAsync(RHostStartupInfo startupInfo, IRSessionCallback callback, int timeout = 3000, CancellationToken cancellationToken = default(CancellationToken));
        Task StopHostAsync(CancellationToken cancellationToken = default(CancellationToken));

        IDisposable DisableMutatedOnReadConsole();

        void FlushLog();
    }
}