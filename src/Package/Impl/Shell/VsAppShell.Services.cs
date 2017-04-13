// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.IO;
using Microsoft.Common.Core.IO;
using Microsoft.Common.Core.Logging;
using Microsoft.Common.Core.OS;
using Microsoft.Common.Core.Security;
using Microsoft.Common.Core.Services;
using Microsoft.R.Editor.Settings;
using Microsoft.R.Interpreters;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.R.Package.Options.R;
using Microsoft.VisualStudio.R.Package.RClient;
using Microsoft.VisualStudio.R.Package.Telemetry;
using VsPackage = Microsoft.VisualStudio.Shell.Package;

namespace Microsoft.VisualStudio.R.Package.Shell {
    public partial class VsAppShell {
        private readonly VsIdleTimeService _idleTimeService = new VsIdleTimeService();
        private VsServiceManager _services;

        public IServiceContainer Services => _services;

        private void ConfigureServices() {
            var platformServices = new VsPlatformServices();
            var telemetry = new VsTelemetryService();
            var componentModel = (IComponentModel)VsPackage.GetGlobalService(typeof(SComponentModel));
            var loggingPermissions = new LoggingPermissions(platformServices, telemetry, new RegistryImpl());
            var settings = new RToolsSettingsImplementation(this, new VsSettingsStorage(), loggingPermissions);
            var compositionCatalog = new CompositionCatalog(componentModel.DefaultCompositionService, componentModel.DefaultExportProvider);

            _services
                .AddService(componentModel)
                .AddService(componentModel.DefaultCompositionService)
                .AddService(componentModel.DefaultExportProvider)
                .AddService(compositionCatalog)
                .AddService(new VsMainThread())
                .AddService(new VsTaskService())
                .AddService(_idleTimeService)
                .AddService(new VsUIServices(this))
                .AddService(new SecurityService(this))
                .AddService(loggingPermissions)
                .AddService(new Logger(ApplicationName, Path.GetTempPath(), loggingPermissions))
                .AddService(platformServices)
                .AddService(settings)
                .AddService(new VsEditorSupport(this))
                .AddService(telemetry)
                .AddService(new FileSystem())
                .AddService(new ProcessServices())
                .AddService(new RegistryImpl())
                .AddService<IMicrosoftRClientInstaller>(new MicrosoftRClientInstaller())
                .AddService<IRInstallationService>(new RInstallation());
            // TODO: add more

            settings.LoadSettings();

            // TODO: get rid of static
            REditorSettings.Initialize(compositionCatalog);
        }
    }
}
