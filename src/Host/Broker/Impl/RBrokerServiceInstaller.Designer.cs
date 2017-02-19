// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel;
using System.ServiceProcess;

namespace Microsoft.R.Host.Broker {
    partial class RBrokerServiceInstaller {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.brokerServiceProcessInstaller = new ServiceProcessInstaller();
            this.brokerServiceInstaller = new ServiceInstaller();
            // 
            // brokerServiceProcessInstaller
            // 
            this.brokerServiceProcessInstaller.Account = ServiceAccount.NetworkService;
            this.brokerServiceProcessInstaller.Password = null;
            this.brokerServiceProcessInstaller.Username = null;
            // 
            // brokerServiceInstaller
            // 
            this.brokerServiceInstaller.ServiceName = Resources.Text_ServiceName;
            this.brokerServiceInstaller.DisplayName = Resources.Text_ServiceDisplayName;
            this.brokerServiceInstaller.DisplayName = Resources.Text_ServiceDescription;
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.brokerServiceProcessInstaller,
            this.brokerServiceInstaller});

        }

        #endregion

        private ServiceProcessInstaller brokerServiceProcessInstaller;
        private ServiceInstaller brokerServiceInstaller;
    }
}