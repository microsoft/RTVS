// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel;
using System.ServiceProcess;

namespace Microsoft.R.Host.UserProfile {
    partial class RUserProfileInstaller {
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
            rUserProfileProcessInstaller = new ServiceProcessInstaller();
            rUserProfileServiceInstaller = new ServiceInstaller();

            rUserProfileProcessInstaller.Account = ServiceAccount.LocalSystem;
            rUserProfileProcessInstaller.Password = null;
            rUserProfileProcessInstaller.Username = null;

            // If you make changes here make sure you edit the product.wxs and RUserProfileService.Designer.cs
            rUserProfileServiceInstaller.ServiceName = Resources.Text_ServiceName;
            rUserProfileServiceInstaller.DisplayName = Resources.Text_ServiceDisplayName;
            rUserProfileServiceInstaller.Description = Resources.Text_ServiceDescription;

            Installers.AddRange(new System.Configuration.Install.Installer[] {
            rUserProfileProcessInstaller,
            rUserProfileServiceInstaller});
        }

        #endregion

        private ServiceProcessInstaller rUserProfileProcessInstaller;
        private ServiceInstaller rUserProfileServiceInstaller;
    }
}