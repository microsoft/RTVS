// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ServiceProcess;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.R.Host.Broker {
    partial class BrokerService : ServiceBase {
        private readonly IWebHost _webHost;
        private volatile bool _stopRequestedByWindows;

        public BrokerService(IWebHost webHost) {
            InitializeComponent();
            _webHost = webHost;
        }

        protected sealed override void OnStart(string[] args) {
            _webHost.Services.GetRequiredService<IApplicationLifetime>().ApplicationStopped.Register(() => {
                if (!_stopRequestedByWindows) {
                    Stop();
                }
            });

            try {
                _webHost.Start();
            } catch (Exception ex) {
                ex.HandleWebHostStartExceptions(_webHost.Services, false);
            }
        }

        protected sealed override void OnStop() {
            _stopRequestedByWindows = true;
            _webHost?.Dispose();
        }
    }
}
