// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.Common.Core.Shell;
using Microsoft.Languages.Core.Settings;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.R.Package.ProjectSystem;
using Microsoft.VisualStudio.R.Package.ProjectSystem.Configuration;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Package.Sql;
using Microsoft.VisualStudio.R.Package.Sql.Publish;
using NSubstitute;

namespace Microsoft.VisualStudio.R.Package.Test.Sql {
    [ExcludeFromCodeCoverage]
    [Category.Sql]
    public class PublishOptionsDialogModelTest {
        private readonly ICoreShell _coreShell;
        private readonly IProjectSystemServices _pss;
        private readonly IProjectConfigurationSettingsProvider _pcsp;
        private readonly IWritableSettingsStorage _storage;

        public PublishOptionsDialogModelTest() {
            _coreShell = Substitute.For<ICoreShell>();
            _pss = Substitute.For<IProjectSystemServices>();
            _pcsp = Substitute.For<IProjectConfigurationSettingsProvider>();
            _storage = Substitute.For<IWritableSettingsStorage>();
        }

        [Test]
        public void Constructor() {
            var model = new SqlPublishOptionsDialogViewModel(_coreShell, _pss, _storage, _pcsp);

            model.TargetTypeNames.Should().HaveCount(3);
            model.SelectedTargetTypeIndex.Should().Be(0);

            model.QuoteTypeNames.Should().HaveCount(3);
            model.SelectedQuoteTypeIndex.Should().Be(0);

            model.CodePlacementNames.Should().HaveCount(2);
            model.SelectedCodePlacementIndex.Should().Be(0);

            model.Targets.Should().HaveCount(0);
            model.TargetHasName.Should().BeFalse();
            model.GenerateTable.Should().BeFalse();

            model.Settings.Should().NotBeNull();
            model.Settings.CodePlacement.Should().Be(RCodePlacement.Inline);
            model.Settings.QuoteType.Should().Be(SqlQuoteType.None);
            model.Settings.TargetType.Should().Be(PublishTargetType.Dacpac);
        }

        [Test]
        public void SelectCodePlacement() {
            var model = new SqlPublishOptionsDialogViewModel(_coreShell, _pss, _storage, _pcsp);
            model.SelectedCodePlacementIndex = 1;
            model.Settings.CodePlacement.Should().Be(RCodePlacement.Table);
            model.GenerateTable.Should().BeTrue();

            model.SelectedCodePlacementIndex = 0;
            model.Settings.CodePlacement.Should().Be(RCodePlacement.Inline);
            model.GenerateTable.Should().BeFalse();
        }

        [Test]
        public void SelectQuoteType() {
            var model = new SqlPublishOptionsDialogViewModel(_coreShell, _pss, _storage, _pcsp);
            model.SelectedQuoteTypeIndex = 1;
            model.Settings.QuoteType.Should().Be(SqlQuoteType.Bracket);
            model.SelectedQuoteTypeIndex = 2;
            model.Settings.QuoteType.Should().Be(SqlQuoteType.Quote);
            model.SelectedQuoteTypeIndex = 0;
            model.Settings.QuoteType.Should().Be(SqlQuoteType.None);
        }

        [Test]
        public void NoDbProjectList() {
            _storage.GetInteger(SqlSProcPublishSettings.TargetTypeSettingName, (int)PublishTargetType.Dacpac).Returns((int)PublishTargetType.Project);
            var model = new SqlPublishOptionsDialogViewModel(_coreShell, _pss, _storage, _pcsp);
            model.Settings.TargetType.Should().Be(PublishTargetType.Project);
            model.Targets.Should().HaveCount(1);
            model.Targets[0].Should().Be(Resources.SqlPublishDialog_NoDatabaseProjects);
        }

        [Test]
        public void ProjectList() {
            var projects = Substitute.For<EnvDTE.Projects>();
            var p1 = Substitute.For<EnvDTE.Project>();
            p1.FileName.Returns("project1.sqlproj");
            p1.Name.Returns("project1");
            var p2 = Substitute.For<EnvDTE.Project>();
            p2.FileName.Returns("project2.sqlproj");
            p2.Name.Returns("project2");
            var p3 = Substitute.For<EnvDTE.Project>();
            p3.FileName.Returns("project3.csproj");
            p3.Name.Returns("project3");
            projects.GetEnumerator().Returns((new EnvDTE.Project[] { p1, p2, p3 }).GetEnumerator());

            var sol = Substitute.For<EnvDTE.Solution>();
            sol.Projects.Returns(projects);
            _pss.GetSolution().Returns(sol);

            _storage.GetInteger(SqlSProcPublishSettings.TargetTypeSettingName, (int)PublishTargetType.Dacpac).Returns((int)PublishTargetType.Project);
            _storage.GetString(SqlSProcPublishSettings.TargetProjectSettingName, Arg.Any<string>()).Returns(("project2"));

            var model = new SqlPublishOptionsDialogViewModel(_coreShell, _pss, _storage, _pcsp);

            model.Settings.TargetType.Should().Be(PublishTargetType.Project);
            model.Settings.TargetProject.Should().Be("project2");

            model.Targets.Should().HaveCount(2);
            model.Targets[0].Should().Be("project1");
            model.Targets[1].Should().Be("project2");
            model.SelectedTargetIndex.Should().Be(1);
         }
    }
}
