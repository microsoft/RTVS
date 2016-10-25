// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Extensions.Logging;
using Microsoft.R.Host.Protocol;

namespace Microsoft.R.Host.UserProfile {
    partial class RUserProfileService : ServiceBase {

        private static int ServiceShutdownTimeoutMs => 5000;
        private static int ClientResponseReadTimeoutMs => 3000;

        public RUserProfileService() {
            InitializeComponent();
        }

        private ManualResetEvent _workerdone;
        private CancellationTokenSource _cts;
        private ILogger _logger;
        private ILoggerFactory _loggerFactory;
        protected override void OnStart(string[] args) {
            _loggerFactory = new LoggerFactory();
            _loggerFactory
                .AddDebug()
                .AddProvider(new ServiceLoggerProvider());
            _logger = _loggerFactory.CreateLogger<RUserProfileService>();

            _cts = new CancellationTokenSource();
            _workerdone = new ManualResetEvent(false);
            CreateProfileWorkerAsync(_cts.Token).DoNotWait();
        }

        protected override void OnStop() {
            _cts.Cancel();
            _workerdone.WaitOne(TimeSpan.FromMilliseconds(ServiceShutdownTimeoutMs));
        }

        async Task CreateProfileWorkerAsync(CancellationToken ct) {
            while (!ct.IsCancellationRequested) {
                try {
                    await RUserProfileCreator.CreateProfileAsync(ct: ct, logger: _logger);
                } catch (Exception ex) when (!ex.IsCriticalException()) {
                    _logger?.LogError(Resources.Error_UserProfileCreationError, ex.Message);
                }
            }
            _workerdone.Set();
        }
    }
}
