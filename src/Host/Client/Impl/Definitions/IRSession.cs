// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.R.Host.Client {
    public interface IRSession : IDisposable {
        event EventHandler<RRequestEventArgs> BeforeRequest;
        event EventHandler<RRequestEventArgs> AfterRequest;
        event EventHandler<EventArgs> Mutated;
        event EventHandler<ROutputEventArgs> Output;
        event EventHandler<EventArgs> Connected;
        event EventHandler<EventArgs> Disconnected;
        event EventHandler<EventArgs> Disposed;
        event EventHandler<EventArgs> DirectoryChanged;

        int Id { get; }
        string Prompt { get; }
        bool IsHostRunning { get; }
        Task HostStarted { get; }

        Task<IRSessionInteraction> BeginInteractionAsync(bool isVisible = true, CancellationToken cancellationToken = default(CancellationToken));
        Task<IRSessionEvaluation> BeginEvaluationAsync(bool isMutating = true, CancellationToken cancellationToken = default(CancellationToken));
        Task<REvaluationResult> EvaluateAsync(string expression, REvaluationKind kind = REvaluationKind.Normal, CancellationToken ct = default(CancellationToken));
        Task CancelAllAsync();
        Task StartHostAsync(RHostStartupInfo startupInfo, int timeout = 3000);
        Task StopHostAsync();

        IDisposable DisableMutatedOnReadConsole();

        void FlushLog();
    }
}