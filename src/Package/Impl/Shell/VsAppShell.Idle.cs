// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Microsoft.VisualStudio.R.Package.Shell {
    public sealed partial class VsAppShell {
        private readonly VsIdleTimeService _idleTimeService = new VsIdleTimeService();

        private void ConfigureIdleSource() => _idleTimeService.ApplicationClosing += OnApplicationClosing;
        private void OnApplicationClosing(object sender, EventArgs e) => Terminating?.Invoke(this, EventArgs.Empty);
    }
}
