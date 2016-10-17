// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel;
using System.ServiceProcess;
using System.Threading.Tasks;
using Microsoft.Common.Core;

namespace Microsoft.R.Host.Broker.Startup {
    public class BrokerService : ServiceBase {
        private IContainer _components = null;

        public BrokerService() {
            InitializeComponent();
        }

        /// <summary>
        /// Executes when a Start command is sent to the service by the Service Control 
        /// Manager (SCM) or when the operating system starts (for a service that starts 
        /// automatically). Specifies actions to take when the service starts.
        /// </summary>
        /// <param name="args">Data passed by the start command.</param>
        protected override void OnStart(string[] args) {
            Task.Run(() => CommonStartup.CreateAndRunWebHostForService()).DoNotWait();
        }

        /// <summary>
        /// Executes when a Stop command is sent to the service by the Service Control Manager 
        /// (SCM). Specifies actions to take when a service stops running.
        /// </summary>
        protected override void OnStop() {
            CommonStartup.Exit();
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (_components != null)) {
                _components.Dispose();
            }
            base.Dispose(disposing);
        }

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            _components = new Container();
            ServiceName = "R Host Broker Service";
        }
    }
}
