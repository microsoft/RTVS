// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.IO;
using FluentAssertions;
using Microsoft.Common.Core.IO;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.Test.Utility;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.R.Package.ProjectSystem;
using Microsoft.VisualStudio.R.Package.Sql.Publish;
using NSubstitute;
using static System.FormattableString;

namespace Microsoft.VisualStudio.R.Package.Test.Sql {
    [ExcludeFromCodeCoverage]
    [Category.Sql]
    public class SProcGeneratorTest {
        private const string sqlRCodeFile = "sqlcode.r";
        private const string sqlProjectName = "db.sqlproj";

        private readonly PackageTestFilesFixture _files;
        private readonly ICoreShell _coreShell;
        private readonly IProjectSystemServices _pss;
        private readonly EnvDTE.Project _project;

        public SProcGeneratorTest(PackageTestFilesFixture files) {
            _files = files;
            _coreShell = Substitute.For<ICoreShell>();
            _pss = Substitute.For<IProjectSystemServices>();

            _project = Substitute.For<EnvDTE.Project>();
            _project.FullName.Returns(Path.Combine(_files.DestinationPath, sqlProjectName));
        }

        [Test]
        public void GenerateEmpty() {
            var fs = new FileSystem();
            var settings = SqlSProcPublishSettings.LoadSettings(_coreShell, _pss, fs, new string[0], _files.DestinationPath);
            var g = new SProcGenerator(_pss, fs);
            g.Generate(settings, new string[] { sqlRCodeFile }, _files.DestinationPath, _project);
        }

        [Test]
        public void GenerateInline() {
            var fs = new FileSystem();
            var settings = SqlSProcPublishSettings.LoadSettings(_coreShell, _pss, fs, new string[] { sqlRCodeFile }, _files.DestinationPath);
            var g = new SProcGenerator(_pss, fs);

            var targetFolder = _files.DestinationPath;
            g.Generate(settings, new string[] { sqlRCodeFile }, targetFolder, _project);

            var rFile = Path.Combine(targetFolder, sqlRCodeFile);
            var rCode = fs.ReadToEnd(rFile);

            var info = settings.SProcInfoEntries[0];
            var sprocFile = Path.ChangeExtension(Path.Combine(targetFolder, "R\\", info.SProcName), ".sql");

            _project.ProjectItems.Received().AddFromFile(sprocFile);

            var baseline = fs.ReadToEnd(Path.Combine(_files.DestinationPath, "SqlCode.inline.baseline.sql"));
            string actual = fs.ReadToEnd(sprocFile);
            BaselineCompare.CompareStringLines(baseline, actual);
        }

        [Test]
        public void GenerateTable() {
            var fs = new FileSystem();
            var settings = SqlSProcPublishSettings.LoadSettings(_coreShell, _pss, fs, new string[] { sqlRCodeFile }, _files.DestinationPath);
            var g = new SProcGenerator(_pss, fs);

            settings.CodePlacement = RCodePlacement.Table;
            var targetFolder = _files.DestinationPath;
            g.Generate(settings, new string[] { sqlRCodeFile }, targetFolder, _project);

            var rFile = Path.Combine(targetFolder, sqlRCodeFile);
            var rCode = fs.ReadToEnd(rFile);

            var info = settings.SProcInfoEntries[0];
            var sprocFile = Path.ChangeExtension(Path.Combine(targetFolder, "R\\", info.SProcName), ".sql");
            _project.ProjectItems.Received().AddFromFile(sprocFile);

            var baseline = fs.ReadToEnd(Path.Combine(_files.DestinationPath, "SqlCode.table.baseline.sql"));
            string actual = fs.ReadToEnd(sprocFile);
            BaselineCompare.CompareStringLines(baseline, actual);
        }
    }
}
