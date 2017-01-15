// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core.Diagnostics;
using Microsoft.Common.Core.Disposables;
using Microsoft.Common.Core.IO;
using Microsoft.Common.Core.Logging;
using Microsoft.Common.Core.OS;
using Microsoft.Common.Core.Security;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Tasks;
using Microsoft.Common.Core.Telemetry;
using Microsoft.Common.Core.Threading;
using Microsoft.R.DataInspection;
using Microsoft.R.Host.Client.Host;
using Microsoft.R.Host.Client.Session;
using Microsoft.R.Interpreters;

namespace Microsoft.R.Host.Client {
    public sealed partial class RHostSession : IRHostSession {
        private readonly IRSession _session;
        private readonly DisposableBag _disposableBag;
        private IRHostSessionCallback _callback;

        public Task HostStarted => _session.HostStarted;
        public bool IsHostRunning => _session.IsHostRunning;
        public bool IsRemote => false;

        public event EventHandler<EventArgs> Connected;
        public event EventHandler<EventArgs> Disconnected;

        public static IRHostSession Create(string name, string url = null) {
            if (string.IsNullOrEmpty(url)) {
                var engine = new RInstallation().GetCompatibleEngines().FirstOrDefault();
                if (engine == null) {
                    throw new InvalidOperationException("No R engines installed");
                }
                url = engine.InstallPath;
            }

            var ci = BrokerConnectionInfo.Create(name, url);
            var bc = new LocalBrokerClient(name, ci, new CoreServices(), new NullConsole());
            return new RHostSession(new RSession(0, name, bc, new NullLock(), () => { }));
        }

        public RHostSession(IRSession session) {
            _session = session;

            _session.Connected += OnSessionConnected;
            _session.Disconnected += OnSessionDisconnected;
            _session.Output += OnSessionOutput;

            _disposableBag = DisposableBag.Create<RHostSession>()
                .Add(() => _session.Output -= OnSessionOutput)
                .Add(() => _session.Connected -= OnSessionConnected)
                .Add(() => _session.Disconnected -= OnSessionDisconnected);
        }

        public void Dispose() {
            _disposableBag.TryDispose();
            _session?.Dispose();
        }

        private void OnSessionOutput(object sender, ROutputEventArgs e) 
            => _callback.Output(e.Message, e.OutputType == OutputType.Error);

        private void OnSessionConnected(object sender, RConnectedEventArgs e)
            => Connected?.Invoke(this, EventArgs.Empty);
        private void OnSessionDisconnected(object sender, EventArgs e)
            => Disconnected?.Invoke(this, EventArgs.Empty);

        #region IRHostSession
        public Task CancelAllAsync(CancellationToken cancellationToken = default(CancellationToken))
            => _session.CancelAllAsync(cancellationToken);

        public Task StartHostAsync(IRHostSessionCallback callback, string workingDirectory = null, int codePage = 0, int timeout = 3000, CancellationToken cancellationToken = default(CancellationToken)) {
            _callback = callback;
            var startupInfo = new RHostStartupInfo(null, workingDirectory, codePage);
            return _session.StartHostAsync(startupInfo, new RSessionSimpleCallback(_callback), timeout, cancellationToken);
        }

        public Task StopHostAsync(bool waitForShutdown = true, CancellationToken cancellationToken = default(CancellationToken))
            => _session.StopHostAsync(waitForShutdown, cancellationToken);

        public Task ExecuteAsync(string expression, CancellationToken cancellationToken = default(CancellationToken)) {
            Check.ArgumentNull(nameof(expression), expression);
            return _session.ExecuteAsync(expression, cancellationToken);
        }

        public Task<T> EvaluateAsync<T>(string expression, CancellationToken cancellationToken = default(CancellationToken)) {
            Check.ArgumentNull(nameof(expression), expression);
            return _session.EvaluateAsync<T>(expression, REvaluationKind.Normal, cancellationToken);
        }
        #endregion

        private Task<IRValueInfo> EvaluateAndDescribeAsync(string expression, REvaluationResultProperties properties, CancellationToken cancellationToken = default(CancellationToken))
            => _session.EvaluateAndDescribeAsync(expression, properties, RValueRepresentations.Str(), cancellationToken);

        private class CoreServices : ICoreServices {
            public IFileSystem FileSystem => new FileSystem();
            public IActionLog Log => new NullLog();
            public ILoggingServices LoggingServices => null;
            public IMainThread MainThread => null;
            public IProcessServices ProcessServices => new ProcessServices();
            public IRegistry Registry => new RegistryImpl();
            public ISecurityService Security => null;
            public ITaskService Tasks => null;
            public ITelemetryService Telemetry => null;
        }
    }
}
