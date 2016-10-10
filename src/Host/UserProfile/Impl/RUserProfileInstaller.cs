// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel;
using System.Configuration.Install;

namespace Microsoft.R.Host.UserProfile {
    [RunInstaller(true)]
    public partial class RUserProfileInstaller : Installer {
        public RUserProfileInstaller() {
            InitializeComponent();
        }
    }
}
