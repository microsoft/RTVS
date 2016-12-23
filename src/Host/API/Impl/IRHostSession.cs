// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.R.DataInspection;

namespace Microsoft.R.Host.Client {
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
        Task<REvaluationResult> EvaluateAsync(string expression, REvaluationKind kind, CancellationToken cancellationToken = default(CancellationToken));
        Task<IRValueInfo> EvaluateAndDescribeAsync(string expression, REvaluationResultProperties properties, CancellationToken cancellationToken = default(CancellationToken));
        Task<IReadOnlyList<IREvaluationResultInfo>> DescribeChildrenAsync(string expression, REvaluationResultProperties properties, int? maxCount = null, CancellationToken cancellationToken = default(CancellationToken));
    }
}
