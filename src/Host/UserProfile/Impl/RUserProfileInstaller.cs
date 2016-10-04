// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.R.Host.UserProfile {
    [RunInstaller(true)]
    public partial class RUserProfileInstaller : System.Configuration.Install.Installer {
        public RUserProfileInstaller() {
            InitializeComponent();
        }
    }
}
