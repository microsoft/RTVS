// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Linq;
using System.Text;
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
using static System.FormattableString;

namespace Microsoft.R.Host.Client {
    /// <summary>
    /// Represents running R session
    /// </summary>
    public sealed partial class RHostSession : IRHostSession {
        private readonly IRSession _session;
        private readonly DisposableBag _disposableBag;
        private IRHostSessionCallback _userSessionCallback;
        private RSessionCallback _rSessionCallback;
        private StringBuilder _output;
        private StringBuilder _errors;

        /// <summary>
        /// Awaitable task that completes when R host process has started
        /// </summary>
        public Task HostStarted => _session.HostStarted;

        /// <summary>
        /// Indicates of R host is running
        /// </summary>
        public bool IsHostRunning => _session.IsHostRunning;

        /// <summary>
        /// Tells if R session is local or remote
        /// </summary>
        public bool IsRemote => false;

        /// <summary>
        /// Fires when R session is connected
        /// </summary>
        public event EventHandler<EventArgs> Connected;

        /// <summary>
        /// Fires when R session has disconnected
        /// </summary>
        public event EventHandler<EventArgs> Disconnected;

        /// <summary>
        /// Creates R session
        /// </summary>
        /// <param name="name">Session name</param>
        /// <param name="url">Path to local R interpreter (folder with R.dll) or URL to the remote machine</param>
        /// <returns>R session</returns>
        public static IRHostSession Create(string name, string url = null) {
            if (string.IsNullOrEmpty(url)) {
                var engine = new RInstallation().GetCompatibleEngines().FirstOrDefault();
                if (engine == null) {
                    throw new InvalidOperationException("No R engines installed");
                }
                url = engine.InstallPath;
            }

            var ci = BrokerConnectionInfo.Create(null, name, url);
            var bc = new LocalBrokerClient(name, ci, new CoreServices(), new NullConsole());
            return new RHostSession(new RSession(0, name, bc, new NullLock(), () => { }));
        }

        private RHostSession(IRSession session) {
            _session = session;

            _session.Connected += OnSessionConnected;
            _session.Disconnected += OnSessionDisconnected;
            _session.Output += OnSessionOutput;

            _disposableBag = DisposableBag.Create<RHostSession>()
                .Add(() => _session.Output -= OnSessionOutput)
                .Add(() => _session.Connected -= OnSessionConnected)
                .Add(() => _session.Disconnected -= OnSessionDisconnected);
        }

        /// <summary>
        /// Terminates and disposes R session
        /// </summary>
        public void Dispose() {
            _disposableBag.TryDispose();
            _session?.Dispose();
        }

        private void OnSessionOutput(object sender, ROutputEventArgs e) {
            if (_output != null && _errors != null) {
                if (e.OutputType == OutputType.Error) {
                    _errors.Append(e.Message);
                } else {
                    _output.Append(e.Message);
                }
            }
        }

        private void OnSessionConnected(object sender, RConnectedEventArgs e)
            => Connected?.Invoke(this, EventArgs.Empty);
        private void OnSessionDisconnected(object sender, EventArgs e)
            => Disconnected?.Invoke(this, EventArgs.Empty);

        #region IRHostSession
        /// <summary>
        /// Attempts to cancel all running tasks in the R Host. 
        /// This is similar to 'Interrupt R' command.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <exception cref="OperationCanceledException" />
        /// <exception cref="RHostDisconnectedException" />
        public Task CancelAllAsync(CancellationToken cancellationToken = default(CancellationToken))
            => _session.CancelAllAsync(cancellationToken);

        /// <summary>
        /// Starts R host process.
        /// </summary>
        /// <param name="callback">
        /// A set of callbacks that are called when R engine requests certain operation
        /// that are usually provided by the application
        /// </param>
        /// <param name="workingDirectory">R working directory</param>
        /// <param name="codePage">R code page to set</param>
        /// <param name="timeout">Timeout to wait for the host process to start</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <exception cref="OperationCanceledException" />
        /// <exception cref="RHostDisconnectedException" />
        public Task StartHostAsync(IRHostSessionCallback callback, string workingDirectory = null, int codePage = 0, int timeout = 3000, CancellationToken cancellationToken = default(CancellationToken)) {
            _userSessionCallback = callback;
            _rSessionCallback = new RSessionCallback(_userSessionCallback);
            var startupInfo = new RHostStartupInfo(null, workingDirectory, codePage, isInteractive:true);
            return _session.StartHostAsync(startupInfo, _rSessionCallback, timeout, cancellationToken);
        }

        /// <summary>
        /// Stops R host process
        /// </summary>
        /// <param name="waitForShutdown">
        /// If true, the method will wait for the R Host process to exit.
        /// If false, the process will receive termination request and the call will return immediately.
        /// </param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <exception cref="OperationCanceledException" />
        /// <exception cref="RHostDisconnectedException" />
        public Task StopHostAsync(bool waitForShutdown = true, CancellationToken cancellationToken = default(CancellationToken))
            => _session.StopHostAsync(waitForShutdown, cancellationToken);

        /// <summary>
        /// Executes R code
        /// </summary>
        /// <param name="expression">Expression or block of R code to execute</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="REvaluationException" />
        /// <exception cref="OperationCanceledException" />
        /// <exception cref="RHostDisconnectedException" />
        public Task ExecuteAsync(string expression, CancellationToken cancellationToken = default(CancellationToken)) {
            Check.ArgumentNull(nameof(expression), expression);
            return _session.ExecuteAsync(expression, cancellationToken);
        }

        /// <summary>
        /// Executes R code and returns output as it would appear in the interactive window.
        /// </summary>
        /// <param name="expression">Expression or block of R code to execute</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="REvaluationException" />
        /// <exception cref="OperationCanceledException" />
        /// <exception cref="RHostDisconnectedException" />
        public async Task<RSessionOutput> ExecuteAndOutputAsync(string expression, CancellationToken cancellationToken = default(CancellationToken)) {
            Check.ArgumentNull(nameof(expression), expression);
            try {
                _output = new StringBuilder();
                _errors = new StringBuilder();

                using (var inter = await _session.BeginInteractionAsync(isVisible: true, cancellationToken: cancellationToken)) {
                    await inter.RespondAsync(expression);
                }

                var o = _output.ToString();
                var e = _errors.ToString();

                return new RSessionOutput(o, e);
            } finally {
                _output = _errors = null;
            }
        }

        /// <summary>
        /// Evaluates the provided expression and returns the result.
        /// This method is typically used to fetch variable value and return it to .NET code.
        /// </summary>
        /// <typeparam name="T">
        /// Type of the variable expected. This must be a simple type.
        /// To return collections use <see cref="GetListAsync"/> and <see cref="GetDataFrameAsync"/>
        /// </typeparam>
        /// <param name="expression">Expression or block of R code to execute</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The variable or expression value</returns>
        /// <exception cref="ArgumentException" />
        /// <exception cref="REvaluationException" />
        /// <exception cref="OperationCanceledException" />
        /// <exception cref="RHostDisconnectedException" />
        public Task<T> EvaluateAsync<T>(string expression, CancellationToken cancellationToken = default(CancellationToken)) {
            Check.ArgumentNull(nameof(expression), expression);
            return _session.EvaluateAsync<T>(expression, REvaluationKind.Normal, cancellationToken);
        }

        /// <summary>
        /// Passes expression the the R plot function and returns plot image data.
        /// </summary>
        /// <param name="expression">Expression or variable name to plot</param>
        /// <param name="width">Image width</param>
        /// <param name="height">Image height</param>
        /// <param name="dpi">Image resolution</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Image data</returns>
        /// <exception cref="ArgumentException" />
        /// <exception cref="REvaluationException" />
        /// <exception cref="OperationCanceledException" />
        /// <exception cref="RHostDisconnectedException" />
        public async Task<byte[]> PlotAsync(string expression, int width, int height, int dpi, CancellationToken cancellationToken = default(CancellationToken)) {
            Check.ArgumentNull(nameof(expression), expression);
            _rSessionCallback.PlotDeviceProperties = new PlotDeviceProperties(width, height, dpi);
            await ExecuteAndOutputAsync(Invariant($"plot({expression})"), cancellationToken);
            return _rSessionCallback.PlotResult;
        }
        #endregion

        private Task<IRValueInfo> EvaluateAndDescribeAsync(string expression, REvaluationResultProperties properties, CancellationToken cancellationToken = default(CancellationToken))
            => _session.EvaluateAndDescribeAsync(expression, properties, RValueRepresentations.Str(), cancellationToken);

        private class CoreServices : ICoreServices {
            public IFileSystem FileSystem => new FileSystem();
            public IActionLog Log => new NullLog();
            public ILoggingPermissions LoggingPermissions => null;
            public IMainThread MainThread => null;
            public IProcessServices Process => new ProcessServices();
            public ISecurityService Security => null;
            public ITaskService Tasks => null;
            public ITelemetryService Telemetry => null;
        }
    }
}
