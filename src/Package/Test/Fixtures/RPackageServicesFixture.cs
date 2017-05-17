// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.ComponentModel.Design;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Common.Core.Services;
using Microsoft.Markdown.Editor.Test;
using Microsoft.R.Components.StatusBar;
using Microsoft.R.Components.Test.Fakes.StatusBar;
using Microsoft.UnitTests.Core.Mef;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.R.Package.Test.Mocks;
using Microsoft.VisualStudio.R.Package.Test.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell.Mocks;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Microsoft.VisualStudio.R.Package.Test.Fixtures {
    [ExcludeFromCodeCoverage]
    public class RPackageServicesFixture : MarkdownEditorServicesFixture {
        protected override IEnumerable<string> GetAssemblyNames() => base.GetAssemblyNames().Concat(new[] {
            "Microsoft.VisualStudio.Shell.Mocks.dll",
            "Microsoft.VisualStudio.R.Package.dll",
            "Microsoft.VisualStudio.R.Package.Test.dll",
            "Microsoft.VisualStudio.ProjectSystem.dll",
            "Microsoft.VisualStudio.ProjectSystem.VS.dll",
            "Microsoft.VisualStudio.ProjectSystem.Implementation.dll",
            "Microsoft.VisualStudio.ProjectSystem.VS.Implementation.dll"
        });

        protected override ComposablePartCatalog FilterCatalog(ComposablePartCatalog catalog)
            => catalog.Filter(cpd => !cpd.ContainsPartMetadataWithKey(PartMetadataAttributeNames.SkipInEditorTestCompositionCatalog));

        protected override void SetupServices(IServiceManager serviceManager, ITestInput testInput) {
            base.SetupServices(serviceManager, testInput);
            serviceManager
                .AddService<IVsEditorAdaptersFactoryService, VsEditorAdaptersFactoryServiceMock>()
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