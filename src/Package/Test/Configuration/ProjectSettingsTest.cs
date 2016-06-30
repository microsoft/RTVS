// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.Common.Core.IO;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Components.Application.Configuration;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.R.Package.ProjectSystem;
using Microsoft.VisualStudio.R.Package.ProjectSystem.PropertyPages.Settings;
using NSubstitute;

namespace Microsoft.VisualStudio.R.Package.Test.Configuration {
    [ExcludeFromCodeCoverage]
    public class ProjectSettingsTest {
        [Test]
        [Category.Configuration]
        public void Test01() {
            var css = Substitute.For<IConfigurationSettingsService>();
            var shell = Substitute.For<ICoreShell>();
            var fs = Substitute.For<IFileSystem>();
            var pss = Substitute.For<IProjectSystemServices>();
            pss.GetActiveProject().Returns((EnvDTE.Project)null);
            var model = new SettingsPageViewModel(css, shell, fs, pss);
        }
    }
}
