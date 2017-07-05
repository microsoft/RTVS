// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Microsoft.Common.Core.IO;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.Test.Utility;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.R.Package.ProjectSystem;
using Microsoft.VisualStudio.R.Package.Sql;
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
        private readonly IProjectSystemServices _pss;
        private readonly EnvDTE.Project _project;

        public SProcGeneratorTest(PackageTestFilesFixture files) {
            _files = files;
            _pss = Substitute.For<IProjectSystemServices>();
            _project = Substitute.For<EnvDTE.Project>();
            _project.FullName.Returns(Path.Combine(_files.DestinationPath, sqlProjectName));
        }

        [Test]
        public void GenerateEmpty() {
            var fs = new WindowsFileSystem();
            var g = new SProcProjectFilesGenerator(_pss, fs);
            var settings = new SqlSProcPublishSettings();
            g.Generate(settings, Enumerable.Empty<string>(), _project);
        }

        [CompositeTest]
        [InlineData("sqlcode1.r", RCodePlacement.Inline, SqlQuoteType.None, "ProcName")]
        [InlineData("sqlcode1.r", RCodePlacement.Table, SqlQuoteType.None, "ProcName")]
        [InlineData("sqlcode2.r", RCodePlacement.Inline, SqlQuoteType.Bracket, "a b")]
        [InlineData("sqlcode2.r", RCodePlacement.Table, SqlQuoteType.Quote, "a b")]
        public void Generate(string rFile, RCodePlacement codePlacement, SqlQuoteType quoteType, string sprocName) {
            var fs = new WindowsFileSystem();
            var settings = new SqlSProcPublishSettings();
            var g = new SProcProjectFilesGenerator(_pss, fs);

            var targetProjItem = Substitute.For<EnvDTE.ProjectItem>();
            var targetProjItems = Substitute.For<EnvDTE.ProjectItems>();
            targetProjItem.ProjectItems.Returns(targetProjItems);

            var rootProjItems = Substitute.For<EnvDTE.ProjectItems>();
            rootProjItems.Item("R").Returns((EnvDTE.ProjectItem)null);
            rootProjItems.AddFolder("R").Returns(targetProjItem);
            _project.ProjectItems.Returns(rootProjItems);

            var templateFile = Path.Combine(_files.DestinationPath, Path.GetFileNameWithoutExtension(rFile) + SProcFileExtensions.SProcFileExtension);

           var sprocFiles = new string[] {
                Path.Combine(_files.DestinationPath, rFile),
                Path.Combine(_files.DestinationPath, Path.GetFileNameWithoutExtension(rFile) + SProcFileExtensions.QueryFileExtension),
                templateFile };

            settings.CodePlacement = codePlacement;
            settings.QuoteType = quoteType;

            g.Generate(settings, sprocFiles, _project);
            rootProjItems.Received().AddFolder("R");

            var targetFolder = Path.Combine(_files.DestinationPath, "R\\");
            var rFilePath = Path.Combine(targetFolder, rFile);
            var sprocFile = Path.ChangeExtension(Path.Combine(targetFolder, sprocName), ".sql");

            targetProjItem.ProjectItems.Received().AddFromFile(sprocFile);
            if (codePlacement == RCodePlacement.Table) {
                targetProjItem.ProjectItems.Received().AddFromFile(Path.Combine(targetFolder, SProcProjectFilesGenerator.PostDeploymentScriptName));
                targetProjItem.ProjectItems.Received().AddFromFile(Path.Combine(targetFolder, SProcProjectFilesGenerator.CreateRCodeTableScriptName));
            }

            var mode = codePlacement == RCodePlacement.Inline ? "inline" : "table";
            var baseline = fs.ReadAllText(Path.Combine(_files.DestinationPath, Invariant($"{Path.GetFileNameWithoutExtension(rFile)}.{mode}.baseline.sql")));
            string actual = fs.ReadAllText(sprocFile);
            BaselineCompare.CompareStringLines(baseline, actual);
        }
    }
}
