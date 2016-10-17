// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections;
using System.ComponentModel;
using System.IO;

namespace Microsoft.R.Host.Broker {
    [RunInstaller(true)]
    public partial class ProjectInstaller : System.Configuration.Install.Installer {
        public ProjectInstaller() {
            InitializeComponent();
        }

        protected override void OnBeforeInstall(IDictionary savedState) {
            // Config file path goes here
            string configFilePath = Path.Combine(Path.GetDirectoryName(Context.Parameters["assemblyPath"]), "Microsoft.R.Host.Broker.Config.json");
            string commandLine = $"\"{Context.Parameters["assemblyPath"]}\" --start.as service --config \"{configFilePath}\"";
            Context.Parameters["assemblyPath"] = commandLine;
            base.OnBeforeInstall(savedState);
        }
    }
}
