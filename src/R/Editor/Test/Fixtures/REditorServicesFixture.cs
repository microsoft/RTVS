// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Common.Core.Imaging;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Test.Fakes.Shell;
using Microsoft.Common.Core.Test.Fixtures;
using Microsoft.Language.Editor.Test.Settings;
using Microsoft.Languages.Editor.Shell;
using Microsoft.R.Editor.Settings;
using Microsoft.R.Editor.Test.Mocks;
using Microsoft.R.Host.Client;
using Microsoft.R.Interpreters;
using Microsoft.R.Support.Test.Utility;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.R.Editor.Test.Fixtures {
    [ExcludeFromCodeCoverage]
    public class REditorServicesFixture : ServiceManagerWithMefFixture {
        protected override IEnumerable<string> GetAssemblyNames() => new[] {
            "Microsoft.VisualStudio.CoreUtility.dll",
            "Microsoft.VisualStudio.Text.Data.dll",
            "Microsoft.VisualStudio.Text.Logic.dll",
            "Microsoft.VisualStudio.Text.UI.dll",
            "Microsoft.VisualStudio.Text.UI.Wpf.dll",
            "Microsoft.VisualStudio.InteractiveWindow.dll",
            "Microsoft.VisualStudio.Editor.dll",
            "Microsoft.VisualStudio.Language.Intellisense.dll",
            "Microsoft.VisualStudio.Platform.VSEditor.dll",
            "Microsoft.R.Components.dll",
            "Microsoft.R.Components.Test.dll",
            "Microsoft.R.Support.dll",
            "Microsoft.R.Editor.dll",
            "Microsoft.R.Editor.Test.dll"
        };

        protected override void SetupServices(IServiceManager serviceManager, ITestInput testInput) {
            base.SetupServices(serviceManager, testInput);
            serviceManager
                .AddWindowsRInterpretersServices()
                .AddWindowsHostClientServices()
                .AddService<IApplicationEditorSupport, ApplicationEditorSupportMock>()
                .AddService<IImageService, TestImageService>()
                .AddService(new TestRToolsSettings(testInput.FileSytemSafeName))
                .AddService(new REditorSettings(new TestSettingsStorage()));
        }
    }
}