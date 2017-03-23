// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ServiceProcess;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.R.Host.Broker.Startup;

namespace Microsoft.R.Host.Broker {
    partial class BrokerService : ServiceBase {
        private readonly IConfigurationRoot _configuration;
        public BrokerService(IConfigurationRoot configuration) {
            _configuration = configuration;
            InitializeComponent();
        }

        protected override void OnStart(string[] args) {
            Task.Run(() => {
                CommonStartup.CreateAndRunWebHostForService(_configuration);
                Stop();
            }).DoNotWait();
        }
    }
}
