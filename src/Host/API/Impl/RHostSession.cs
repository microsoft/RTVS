// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core.Disposables;
using Microsoft.Common.Core.IO;
using Microsoft.Common.Core.Logging;
using Microsoft.Common.Core.OS;
using Microsoft.Common.Core.Threading;
using Microsoft.R.Interpreters;
using Microsoft.R.Host.Client.Host;
using Microsoft.R.Host.Client.Session;
using System.Collections.Generic;
using Microsoft.R.DataInspection;

namespace Microsoft.R.Host.Client.API {
    public sealed class RHostSession : IRHostSession {
        private readonly IRSession _session;
        private readonly DisposableBag _disposableBag;

        public bool IsHostRunning => _session.IsHostRunning;
        public bool IsRemote => false;

        public event EventHandler<EventArgs> Connected;
        public event EventHandler<EventArgs> Disconnected;

        public static IRHostSession Create(string name, string url = null) {
            if(string.IsNullOrEmpty(url)) {
                var engine = new RInstallation().GetCompatibleEngines().FirstOrDefault();
                if(engine == null) {
                    throw new InvalidOperationException("No R engines installed");
                }
                url = engine.InstallPath;
            }

            var ci = BrokerConnectionInfo.Create(url);
            var bc = new LocalBrokerClient(name, ci, new FileSystem(), new ProcessServices(), new NullLog(), new NullConsole());
            return new RHostSession(new RSession(0, name, bc, new NullLock(), () => { }));
        }

        public RHostSession(IRSession session) {
            _session = session;

            _session.Connected += OnSessionConnected;
            _session.Disconnected += OnSessionDisconnected;

            _disposableBag = DisposableBag.Create<RHostSession>()
                .Add(() => _session.Connected -= OnSessionConnected)
                .Add(() => _session.Disconnected -= OnSessionDisconnected);
        }

        public void Dispose() {
            _disposableBag.TryDispose();
            _session?.Dispose();
        }

        private void OnSessionConnected(object sender, RConnectedEventArgs e) => Connected?.Invoke(this, EventArgs.Empty);
        private void OnSessionDisconnected(object sender, EventArgs e) => Connected?.Invoke(this, EventArgs.Empty);

        #region IRHostSession
        public Task CancelAllAsync(CancellationToken cancellationToken = default(CancellationToken))
            => _session.CancelAllAsync(cancellationToken);

        public Task StartHostAsync(IRHostSessionCallback callback, string workingDirectory = null, int codePage = 0, int timeout = 3000, CancellationToken cancellationToken = default(CancellationToken))
            => _session.StartHostAsync(new RHostStartupInfo(null, workingDirectory, codePage), new RSessionSimpleCallback(callback), timeout, cancellationToken);

        public Task StopHostAsync(bool waitForShutdown = true, CancellationToken cancellationToken = default(CancellationToken))
            => _session.StopHostAsync(waitForShutdown, cancellationToken);

        public Task<REvaluationResult> EvaluateAsync(string expression, REvaluationKind kind, CancellationToken cancellationToken = default(CancellationToken))
            => _session.EvaluateAsync(expression, kind, cancellationToken);

        public Task ExecuteAsync(string expression, CancellationToken cancellationToken = default(CancellationToken))
            => _session.EvaluateAsync(expression, REvaluationKind.Normal, cancellationToken);

        public Task<T> EvaluateAsync<T>(string expression, CancellationToken cancellationToken = default(CancellationToken))
            => _session.EvaluateAsync<T>(expression, REvaluationKind.Normal, cancellationToken);

        public Task<IRValueInfo> EvaluateAndDescribeAsync(string expression, REvaluationResultProperties properties, CancellationToken cancellationToken = default(CancellationToken))
            => _session.EvaluateAndDescribeAsync(expression, properties, null, cancellationToken);

        public Task<IReadOnlyList<IREvaluationResultInfo>> DescribeChildrenAsync(string expression, REvaluationResultProperties properties, int? maxCount = null, CancellationToken cancellationToken = default(CancellationToken))
            => _session.DescribeChildrenAsync(REnvironments.GlobalEnv, expression, properties, null, maxCount, cancellationToken);
        #endregion

        private class NullLog : IActionLog {
            public LogVerbosity LogVerbosity => LogVerbosity.None;
            public void Flush() { }
            public Task WriteAsync(LogVerbosity verbosity, MessageCategory category, string message) => Task.CompletedTask;
            public Task WriteFormatAsync(LogVerbosity verbosity, MessageCategory category, string format, params object[] arguments) => Task.CompletedTask;
            public Task WriteLineAsync(LogVerbosity verbosity, MessageCategory category, string message) => Task.CompletedTask;
        }

        private sealed class NullLock : IExclusiveReaderLock {
            public Task<IAsyncReaderWriterLockToken> WaitAsync(CancellationToken cancellationToken = default(CancellationToken))
                => Task.FromResult<IAsyncReaderWriterLockToken>(new NullToken());
        }

        private sealed class NullToken : IAsyncReaderWriterLockToken {
            public ReentrancyToken Reentrancy => default(ReentrancyToken);
            public void Dispose() { }
        }
    }
}
