// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Common.Core.Logging;
using Microsoft.Common.Core.OS;
using Microsoft.Common.Core.Services;
using Microsoft.VisualStudio.R.Package.RClient;
using Microsoft.VisualStudio.R.Package.Telemetry;
using VsPackage = Microsoft.VisualStudio.Shell.Package;

namespace Microsoft.VisualStudio.R.Package.Shell {
    public partial class VsAppShell {
        private VsServiceManager _services;

        public IServiceContainer Services => _services;

        private void ConfigureServices() {
            _services = new VsServiceManager(this);
            var platformServices = new VsPlatformServices();
            var telemetry = new VsTelemetryService();

            _services
                .AddService(new VsUIServices(this))
                .AddService(platformServices)
                .AddService(_settings)
                .AddService(new LoggingPermissions(platformServices, telemetry, new RegistryImpl()))
                .AddService(typeof(MicrosoftRClientInstaller));
            // TODO: add more
        }
    }
}
