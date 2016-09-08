// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.R.Host.Client.Host;

namespace Microsoft.R.Host.Client {
    public interface IRSession : IRExpressionEvaluator, IRBlobService, IDisposable {
        event EventHandler<RBeforeRequestEventArgs> BeforeRequest;
        event EventHandler<RAfterRequestEventArgs> AfterRequest;
        event EventHandler<EventArgs> Mutated;
        event EventHandler<ROutputEventArgs> Output;
        event EventHandler<RConnectedEventArgs> Connected;
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

        Task<IRSessionInteraction> BeginInteractionAsync(bool isVisible = true, CancellationToken cancellationToken = default(CancellationToken));
        Task<IRSessionEvaluation> BeginEvaluationAsync(CancellationToken cancellationToken = default(CancellationToken));
        Task CancelAllAsync();
        Task StartHostAsync(RHostStartupInfo startupInfo, IRSessionCallback callback, int timeout = 3000);
        Task StopHostAsync();

        IDisposable DisableMutatedOnReadConsole();

        void FlushLog();
    }
}