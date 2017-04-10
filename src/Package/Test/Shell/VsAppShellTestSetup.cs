// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Common.Core.Extensions;
using Microsoft.Common.Core.IO;
using Microsoft.Common.Core.Logging;
using Microsoft.Common.Core.OS;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.Test.Fakes.Shell;
using Microsoft.Common.Core.Test.Logging;
using Microsoft.Common.Core.Test.Stubs.Shell;
using Microsoft.Language.Editor.Test.Settings;
using Microsoft.R.Editor.Settings;
using Microsoft.R.Support.Test.Utility;
using Microsoft.UnitTests.Core.Threading;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Package.Test.Mocks;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell.Mocks;
using Microsoft.VisualStudio.TextManager.Interop;
using NSubstitute;

namespace Microsoft.VisualStudio.R.Package.Test.Shell {
    /// <summary>
    /// Setup is VsAppShell in unit tests. Created via reflection 
    /// in <see cref="VsAppShell"/> when test first accesses 
    /// <see cref="VsAppShell.Current"/>.
    /// </summary>
    [ExcludeFromCodeCoverage]
    static class VsAppShellTestSetup {
        public static void Setup(VsAppShell instance) {
            var serviceManager = instance.Services as VsServiceManager;
            Debug.Assert(!serviceManager.AllServices.Any(), "Test VsAppShell service container must be empty at init time");

            var catalog = VsTestCompositionCatalog.Current;

            var batch = new CompositionBatch().AddValue<ICoreShell>(instance);
            VsTestCompositionCatalog.Current.Container.Compose(batch);

            serviceManager.CompositionService = catalog.CompositionService;
            serviceManager.ExportProvider = catalog.ExportProvider;
            serviceManager
                // ICoreShell
                .AddService(instance)
                // MEF
                .AddService(catalog)
                .AddService(catalog.CompositionService)
                .AddService(catalog.ExportProvider)
                // IMainThread and basic services
                .AddService(UIThreadHelper.Instance)
                .AddService(Substitute.For<IActionLog>())
                .AddService(new SecurityServiceStub())
                .AddService(new MaxLoggingPermissions())
                .AddService(new FileSystem())
                .AddService(new RegistryImpl())
                .AddService(new ProcessServices())
                .AddService(new TestUIServices())
                .AddService(new TestTaskService())
                .AddService(new TestPlatformServices())
                .AddService(new TestRToolsSettings())
                .AddService(new REditorSettings(new TestSettingsStorage()))
                // OLE and VS specifics
                .AddService(new VsRegisterProjectGeneratorsMock(), typeof(SVsRegisterProjectTypes))
                .AddService(VsRegisterEditorsMock.Create(), typeof(SVsRegisterEditors))
                .AddService(new MenuCommandServiceMock(), typeof(IMenuCommandService))
                .AddService(new ComponentModelMock(VsTestCompositionCatalog.Current), typeof(SComponentModel))
                .AddService(new TextManagerMock(), typeof(SVsTextManager))
                .AddService(VsImageServiceMock.Create(), typeof(SVsImageService))
                .AddService(new VsUiShellMock(), typeof(SVsUIShell))
                .AddService(OleComponentManagerMock.Create(), typeof(SOleComponentManager))
                .AddService(VsSettingsManagerMock.Create(), typeof(SVsSettingsManager));
        }
    }
}
