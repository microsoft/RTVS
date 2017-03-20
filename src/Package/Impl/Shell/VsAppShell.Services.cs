// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Common.Core.IO;
using Microsoft.Common.Core.Logging;
using Microsoft.Common.Core.OS;
using Microsoft.Common.Core.Services;
using Microsoft.R.Components.Settings;
using Microsoft.VisualStudio.ComponentModelHost;
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
            var componentModel = (IComponentModel)VsPackage.GetGlobalService(typeof(SComponentModel));

            var settings = componentModel.DefaultExportProvider.GetExportedValue<IRSettings>();
            settings.LoadSettings();

            _services
                .AddService(componentModel)
                .AddService(componentModel.DefaultCompositionService)
                .AddService(componentModel.DefaultExportProvider)
                .AddService(new CompositionCatalog(componentModel.DefaultCompositionService, componentModel.DefaultExportProvider))
                .AddService(new VsTaskService())
                .AddService(new VsUIServices(this))
                .AddService(platformServices)
                .AddService(settings)
                .AddService(new VsEditorSupport(this))
                .AddService(telemetry)
                .AddService(new LoggingPermissions(platformServices, telemetry, new RegistryImpl()))
                .AddService(new FileSystem())
                .AddService(new ProcessServices())
                .AddService(new RegistryImpl())
                .AddService(typeof(MicrosoftRClientInstaller));
            // TODO: add more
        }
    }
}
