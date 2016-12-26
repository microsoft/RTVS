// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.R.Host.Client.API {
    public interface IRHostSession : IDisposable {
        event EventHandler<EventArgs> Connected;
        event EventHandler<EventArgs> Disconnected;

        bool IsHostRunning { get; }
        bool IsRemote { get; }

        Task StartHostAsync(IRHostSessionCallback callback, string workingDirectory = null, int codePage = 0, int timeout = 3000, CancellationToken cancellationToken = default(CancellationToken));
        Task StopHostAsync(bool waitForShutdown = true, CancellationToken cancellationToken = default(CancellationToken));
        Task CancelAllAsync(CancellationToken cancellationToken = default(CancellationToken));

        Task ExecuteAsync(string expression, CancellationToken cancellationToken = default(CancellationToken));

        Task<T> EvaluateAsync<T>(string expression, CancellationToken cancellationToken = default(CancellationToken));

        Task InvokeAsync(string function, CancellationToken cancellationToken = default(CancellationToken), params object[] args);
        Task<string> InvokeAndReturnAsync(string function, CancellationToken cancellationToken = default(CancellationToken), params object[] args);

            Task<List<object>> GetListAsync(string expression, CancellationToken cancellationToken = default(CancellationToken));
        Task<DataFrame> GetDataFrameAsync(string expression, CancellationToken cancellationToken = default(CancellationToken));
        Task<T[,]> GetMatrixAsync<T>(string expression, CancellationToken cancellationToken = default(CancellationToken));
    }
}
