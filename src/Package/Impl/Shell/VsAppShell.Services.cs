// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.IO;
using Microsoft.Common.Core.IO;
using Microsoft.Common.Core.Logging;
using Microsoft.Common.Core.OS;
using Microsoft.Common.Core.Security;
using Microsoft.Common.Core.Services;
using Microsoft.R.Editor;
using Microsoft.R.Editor.Settings;
using Microsoft.R.Host.Client;
using Microsoft.R.Interpreters;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.R.Package.Imaging;
using Microsoft.VisualStudio.R.Package.Options.R;
using Microsoft.VisualStudio.R.Package.RClient;
using Microsoft.VisualStudio.R.Package.Telemetry;
using VsPackage = Microsoft.VisualStudio.Shell.Package;

namespace Microsoft.VisualStudio.R.Package.Shell {
    public partial class VsAppShell {
        private readonly VsServiceManager _services;
        private VsApplication _application;

        public IServiceContainer Services => _services;

        private void ConfigureServices() {
            var platformServices = new VsPlatformServices();
            var telemetry = new VsTelemetryService();
            var componentModel = (IComponentModel)VsPackage.GetGlobalService(typeof(SComponentModel));
            var loggingPermissions = new LoggingPermissions(platformServices, telemetry, new RegistryImpl());
            var settings = new RSettingsImplementation(this, new VsSettingsStorage(), loggingPermissions);
            var compositionCatalog = new CompositionCatalog(componentModel.DefaultCompositionService, componentModel.DefaultExportProvider);
            var exportProvider = componentModel.DefaultExportProvider;

            _services
                .AddService(componentModel)
                .AddService(componentModel.DefaultCompositionService)
                .AddService(exportProvider)
                .AddService(compositionCatalog)
                .AddService(new VsMainThread())
                .AddService(new VsTaskService())
                .AddService(new VsUIServices(this))
                .AddService(new SecurityService(this))
                .AddService(loggingPermissions)
                .AddService(platformServices)
                .AddService(settings)
                .AddService(new REditorSettings(this))
                .AddService(new ImageService(exportProvider.GetExportedValue<IGlyphService>()))
                .AddService(new VsEditorSupport(Services))
                .AddService(new VsEditorViewLocator())
                .AddService(telemetry)
                .AddService(new WindowsFileSystem())
                .AddService(new ProcessServices())
                .AddService(new RegistryImpl())
                .AddService(new MicrosoftRClientInstaller())
                .AddWindowsRInterpretersServices()
                .AddWindowsHostClientServices()
                .AddEditorServices();
            // TODO: add more

            _application = new VsApplication(this);

            _services
                .AddService(_application)
                .AddService(new Logger(_application.Name, Path.GetTempPath(), loggingPermissions));

            settings.LoadSettings();
        }
    }
}
