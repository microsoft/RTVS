// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.OS;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.EventLog;
using Microsoft.R.Platform.IO;

namespace Microsoft.R.Host.UserProfile {
    partial class RUserProfileService : ServiceBase {

        private static int ServiceShutdownTimeoutMs => 5000;

        private static int ServiceReadAfterConnectTimeoutMs => 5000;
        private static int ClientResponseReadTimeoutMs => 5000;

        public RUserProfileService() {
            InitializeComponent();
        }

        private ManualResetEvent _createWorkerDone;
        private ManualResetEvent _deleteWorkerDone;
        private CancellationTokenSource _cts;
        private ILogger _logger;
        private ILoggerFactory _loggerFactory;

        protected override void OnStart(string[] args) {
            _loggerFactory = new LoggerFactory();
            _loggerFactory
                .AddDebug()
                .AddEventLog(new EventLogSettings {
                    Filter = (_, logLevel) => logLevel >= LogLevel.Trace,
                    SourceName = Resources.Text_ServiceName
                });
            _logger = _loggerFactory.CreateLogger<RUserProfileService>();

            _cts = new CancellationTokenSource();
            _createWorkerDone = new ManualResetEvent(false);
            _deleteWorkerDone = new ManualResetEvent(false);

            CreateProfileWorkerAsync(_cts.Token).DoNotWait();
            DeleteProfileWorkerAsync(_cts.Token).DoNotWait();
        }

        protected override void OnStop() {
            _cts.Cancel();
            WaitHandle.WaitAll(new WaitHandle[] { _createWorkerDone, _deleteWorkerDone }, TimeSpan.FromMilliseconds(ServiceShutdownTimeoutMs));
        }

        private async Task CreateProfileWorkerAsync(CancellationToken ct) {
            await ProfileWorkerAsync(RUserProfileServicesHelper.CreateProfileAsync, ServiceReadAfterConnectTimeoutMs, ClientResponseReadTimeoutMs, _createWorkerDone, ct, _logger);
        }

        private async Task DeleteProfileWorkerAsync(CancellationToken ct) {
            await ProfileWorkerAsync(RUserProfileServicesHelper.DeleteProfileAsync, ServiceReadAfterConnectTimeoutMs, ClientResponseReadTimeoutMs, _deleteWorkerDone, ct, _logger);
        }

        private static async Task ProfileWorkerAsync( Func<int,int, IUserProfileServices, IUserProfileNamedPipeFactory, CancellationToken, ILogger, Task> action, int serverTimeOutms, int clientTimeOutms,  ManualResetEvent workerDone, CancellationToken ct, ILogger logger) {
            while (!ct.IsCancellationRequested) {
                try {
                    await action?.Invoke(ServiceReadAfterConnectTimeoutMs, ClientResponseReadTimeoutMs, null, null, ct, logger);
                } catch (TaskCanceledException) {
                } catch (Exception ex) when (!ex.IsCriticalException()) {
                    logger?.LogError(Resources.Error_UserProfileServiceError, ex.Message);
                }
            }
            workerDone.Set();
        }
    }
}
