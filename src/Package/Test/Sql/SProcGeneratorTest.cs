// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.IO;
using Microsoft.Common.Core.IO;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.Test.Utility;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.R.Package.ProjectSystem;
using Microsoft.VisualStudio.R.Package.Sql.Publish;
using NSubstitute;
using Xunit;
using static System.FormattableString;

namespace Microsoft.VisualStudio.R.Package.Test.Sql {
    [ExcludeFromCodeCoverage]
    [Category.Sql]
    public class SProcGeneratorTest {
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
            var g = new SProcGenerator(_coreShell, _pss, fs);
            g.Generate(new SqlSProcPublishSettings(), new string[] { "sqlcode1.r" }, _project);
        }

        [CompositeTest]
        [InlineData("sqlcode1.r", RCodePlacement.Inline)]
        [InlineData("sqlcode1.r", RCodePlacement.Table)]
        public void Generate(string rFile, RCodePlacement codePlacement) {
            var fs = new FileSystem();
            var settings = new SqlSProcPublishSettings();
            settings.Files.Add(Path.Combine(_files.DestinationPath, rFile));
            var g = new SProcGenerator(_coreShell, _pss, fs);

            settings.CodePlacement = codePlacement;

            var targetFolder = _files.DestinationPath;
            g.Generate(settings, new string[] { rFile }, _project);

            var rFilePath = Path.Combine(targetFolder, rFile);
            var rCode = fs.ReadAllText(rFilePath);
            var sprocFile = Path.ChangeExtension(Path.Combine(targetFolder, "R\\", Path.GetFileNameWithoutExtension(settings.Files[0])), ".sql");

            _project.ProjectItems.Received().AddFromFile(sprocFile);

            var mode = codePlacement == RCodePlacement.Inline ? "inline" : "table";
            var baseline = fs.ReadAllText(Path.Combine(_files.DestinationPath, Invariant($"{Path.GetFileNameWithoutExtension(rFile)}.{mode}.baseline.sql")));
            string actual = fs.ReadAllText(sprocFile);
            BaselineCompare.CompareStringLines(baseline, actual);
        }
    }
}
