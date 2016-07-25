// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.Common.Core.IO;
using Microsoft.Common.Core.Shell;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.R.Package.ProjectSystem;
using Microsoft.VisualStudio.R.Package.Sql.Publish;
using NSubstitute;

namespace Microsoft.VisualStudio.R.Package.Test.Sql {
    [ExcludeFromCodeCoverage]
    [Category.Sql]
    public class SettingsTest {
        private readonly PackageTestFilesFixture _files;
        public SettingsTest(PackageTestFilesFixture files) {
            _files = files;
        }

        [Test]
        public void LoadSettings() {
            var coreShell = Substitute.For<ICoreShell>();
            var pss = Substitute.For<IProjectSystemServices>();
            var fs = Substitute.For<IFileSystem>();

            var settings = SqlSProcPublishSettings.LoadSettings(coreShell, pss, new FileSystem(), new string[0], _files.DestinationPath);
            settings.SProcInfoEntries.Should().BeEmpty();
            settings.TableName.Should().Be("RCodeTable");
            settings.TargetProject.Should().Be("Database");

            settings = SqlSProcPublishSettings.LoadSettings(coreShell, pss, new FileSystem(), new string[] { "sqlcode.r" }, _files.DestinationPath);
            settings.SProcInfoEntries.Should().HaveCount(1);
            settings.TableName.Should().Be("RCodeTable");
            settings.SProcInfoEntries[0].FileName.Should().Be("sqlcode.r");
            settings.SProcInfoEntries[0].SProcName.Should().Be("script");
        }
    }
}
