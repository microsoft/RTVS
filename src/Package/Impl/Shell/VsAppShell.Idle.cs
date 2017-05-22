// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.VisualStudio.R.Package.Shell {
    public sealed partial class VsAppShell {
        private VsIdleTimeService _idleTimeService;

        private void ConfigureIdleSource() {
            _idleTimeService = new VsIdleTimeService(_services);
            _idleTimeService.ApplicationClosing += (s, e) => _application.FireTerminating();
            _services.AddService(_idleTimeService);
        }
    }
}
