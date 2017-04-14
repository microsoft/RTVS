// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel;
using System.Configuration.Install;

namespace Microsoft.R.Host.Broker {
    [RunInstaller(true)]
    public partial class RBrokerServiceInstaller : Installer {
        public RBrokerServiceInstaller() {
            InitializeComponent();
        }
    }
}
