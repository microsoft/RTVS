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
        /// <summary>
        /// Fires when R Host process is connected and is about to enter R loop
        /// </summary>
        event EventHandler<RConnectedEventArgs> Connected;
        
        /// <summary>
        /// RTVS related R initialization is completed and RTVS package is loaded.
        /// </summary>
        event EventHandler<EventArgs> Interactive;

        /// <summary>
        /// Session has been disconnected (R host process terminated or network connection is lost).
        /// </summary>
        event EventHandler<EventArgs> Disconnected;

        event EventHandler<EventArgs> Disposed;
        event EventHandler<EventArgs> DirectoryChanged;
        event EventHandler<EventArgs> PackagesInstalled;
        event EventHandler<EventArgs> PackagesRemoved;

        int Id { get; }
        string Prompt { get; }
        bool IsHostRunning { get; }
        Task HostStarted { get; }

        /// <summary>
        /// The session is remote
        /// </summary>
        bool IsRemote { get; }

        /// <summary>
        /// Session is currently processing user request.
        /// </summary>
        bool IsProcessing { get; }

        /// <summary>
        /// Session is currently at the prompt from user code
        /// such as readline() or swirl()
        /// </summary>
        bool IsReadingUserInput { get; }

        bool RestartOnBrokerSwitch { get; set; }

        Task<IRSessionInteraction> BeginInteractionAsync(bool isVisible = true, CancellationToken cancellationToken = default(CancellationToken));

        Task CancelAllAsync(CancellationToken cancellationToken = default(CancellationToken));
        Task StartHostAsync(RHostStartupInfo startupInfo, IRSessionCallback callback, int timeout = 3000, CancellationToken cancellationToken = default(CancellationToken));
        Task EnsureHostStartedAsync(RHostStartupInfo startupInfo, IRSessionCallback callback, int timeout = 3000, CancellationToken cancellationToken = default(CancellationToken));
        Task StopHostAsync(bool waitForShutdown = true, CancellationToken cancellationToken = default(CancellationToken));

        IDisposable DisableMutatedOnReadConsole();

        void FlushLog();
    }
}