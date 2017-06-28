// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.Test.Fakes.Shell;
using Microsoft.R.Components.Application.Configuration;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.ProjectSystem.Properties;
using Microsoft.VisualStudio.R.Package.ProjectSystem;
using Microsoft.VisualStudio.R.Package.ProjectSystem.Configuration;
using Microsoft.VisualStudio.R.Package.Sql;
using Microsoft.VisualStudio.R.Package.Sql.Publish;
using Microsoft.VisualStudio.Shell.Interop;
using NSubstitute;

namespace Microsoft.VisualStudio.R.Package.Test.Sql {
    [ExcludeFromCodeCoverage]
    [Category.Sql]
    public class PublishOptionsDialogModelTest {
        private readonly ICoreShell _coreShell = TestCoreShell.CreateBasic();
        private readonly IProjectSystemServices _pss = Substitute.For<IProjectSystemServices>();
        private readonly IProjectConfigurationSettingsProvider _pcsp = Substitute.For<IProjectConfigurationSettingsProvider>();
        private readonly ISettingsStorage _storage = Substitute.For<ISettingsStorage>();

        [Test(ThreadType.UI)]
        public async Task Constructor() {
            var settings = new SqlSProcPublishSettings(_storage);
            var model = await SqlPublishOptionsDialogViewModel.CreateAsync(settings, _coreShell, _pss, _pcsp);

            model.TargetTypeNames.Should().HaveCount(3);
            model.SelectedTargetTypeIndex.Should().Be(0);

            model.QuoteTypeNames.Should().HaveCount(3);
            model.SelectedQuoteTypeIndex.Should().Be(0);

            model.CodePlacementNames.Should().HaveCount(2);
            model.SelectedCodePlacementIndex.Should().Be(0);

            model.Targets.Should().BeEmpty();
            model.TargetHasName.Should().BeFalse();
            model.GenerateTable.Should().BeFalse();

            model.Settings.Should().NotBeNull();
            model.Settings.CodePlacement.Should().Be(RCodePlacement.Inline);
            model.Settings.QuoteType.Should().Be(SqlQuoteType.None);
            model.Settings.TargetType.Should().Be(PublishTargetType.Dacpac);
        }

        [Test(ThreadType.UI)]
        public async Task SelectCodePlacement() {
            var settings = new SqlSProcPublishSettings(_storage);
            var model = await SqlPublishOptionsDialogViewModel.CreateAsync(settings, _coreShell, _pss, _pcsp);

            model.SelectCodePlacement(1);
            model.Settings.CodePlacement.Should().Be(RCodePlacement.Table);
            model.GenerateTable.Should().BeTrue();

            model.SelectCodePlacement(0);
            model.Settings.CodePlacement.Should().Be(RCodePlacement.Inline);
            model.GenerateTable.Should().BeFalse();
        }

        [Test(ThreadType.UI)]
        public async Task SelectQuoteType() {
            var settings = new SqlSProcPublishSettings(_storage);
            var model = await SqlPublishOptionsDialogViewModel.CreateAsync(settings, _coreShell, _pss, _pcsp);

            model.SelectQuoteType(1);
            model.Settings.QuoteType.Should().Be(SqlQuoteType.Bracket);
            model.SelectQuoteType(2);
            model.Settings.QuoteType.Should().Be(SqlQuoteType.Quote);
            model.SelectQuoteType(0);
            model.Settings.QuoteType.Should().Be(SqlQuoteType.None);
        }

        [Test(ThreadType.UI)]
        public async Task SelectTargetType() {
            var settings = new SqlSProcPublishSettings(_storage);

            var model = await SqlPublishOptionsDialogViewModel.CreateAsync(settings, _coreShell, _pss, _pcsp);
            await model.SelectTargetTypeAsync(1);

            model.Settings.TargetType.Should().Be(PublishTargetType.Database);
            model.TargetHasName.Should().BeTrue();

            await model.SelectTargetTypeAsync(2);
            model.Settings.TargetType.Should().Be(PublishTargetType.Project);
            model.TargetHasName.Should().BeTrue();

            await model.SelectTargetTypeAsync(3);
            model.Settings.TargetType.Should().Be(PublishTargetType.File);
            model.TargetHasName.Should().BeFalse();

            await model.SelectTargetTypeAsync(0);
            model.Settings.TargetType.Should().Be(PublishTargetType.Dacpac);
            model.TargetHasName.Should().BeFalse();
        }

        [Test(ThreadType.UI)]
        public async Task NoDbProjectList() {
            _storage.GetSetting(SqlSProcPublishSettings.TargetTypeSettingName, PublishTargetType.Dacpac).Returns(PublishTargetType.Project);

            var settings = new SqlSProcPublishSettings(_storage);
            var model = await SqlPublishOptionsDialogViewModel.CreateAsync(settings, _coreShell, _pss, _pcsp);

            model.Settings.TargetType.Should().Be(PublishTargetType.Project);
            model.Targets.Should().HaveCount(1);
            model.Targets[0].Should().Be(Resources.SqlPublishDialog_NoDatabaseProjects);
        }

        [Test(ThreadType.UI)]
        public async Task ProjectList() {
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

            _storage.GetSetting(SqlSProcPublishSettings.TargetTypeSettingName, PublishTargetType.Dacpac).Returns(PublishTargetType.Project);
            _storage.GetSetting(SqlSProcPublishSettings.TargetProjectSettingName, Arg.Any<string>()).Returns(("project2"));

            var settings = new SqlSProcPublishSettings(_storage);
            var model = await SqlPublishOptionsDialogViewModel.CreateAsync(settings, _coreShell, _pss, _pcsp);

            model.Settings.TargetType.Should().Be(PublishTargetType.Project);
            model.Settings.TargetProject.Should().Be("project2");

            model.Targets.Should().HaveCount(2);
            model.Targets[0].Should().Be("project1");
            model.Targets[1].Should().Be("project2");
            model.SelectedTargetIndex.Should().Be(1);
        }

        [Test(ThreadType.UI)]
        public async Task NoDbConnections() {
            ConfigureSettingAccessMock(Enumerable.Empty<IConfigurationSetting>());

            _storage.GetSetting(SqlSProcPublishSettings.TargetTypeSettingName, PublishTargetType.Dacpac).Returns(PublishTargetType.Database);

            var settings = new SqlSProcPublishSettings(_storage);
            var model = await SqlPublishOptionsDialogViewModel.CreateAsync(settings, _coreShell, _pss, _pcsp);

            model.Settings.TargetType.Should().Be(PublishTargetType.Database);
            model.Targets.Should().HaveCount(1);
            model.Targets[0].Should().Be(Resources.SqlPublishDialog_NoDatabaseConnections);
        }

        [Test(ThreadType.UI)]
        public async Task DbConnectionsList() {
            var s1 = Substitute.For<IConfigurationSetting>();
            s1.Name.Returns("dbConn1");
            s1.Value.Returns("dbConn1_String");
            s1.EditorType.Returns(ConnectionStringEditor.ConnectionStringEditorName);

            var s2 = Substitute.For<IConfigurationSetting>();
            s2.Name.Returns("dbConn2");
            s2.Value.Returns("dbConn2_String");
            s2.EditorType.Returns(ConnectionStringEditor.ConnectionStringEditorName);

            var s3 = Substitute.For<IConfigurationSetting>();
            s3.Name.Returns("dbConn3");
            s3.Value.Returns("dbConn3_String");
            s3.EditorType.Returns(ConnectionStringEditor.ConnectionStringEditorName);

            var s4 = Substitute.For<IConfigurationSetting>();
            s4.Name.Returns("dbConn4");
            s4.Value.Returns("dbConn4_String");

            ConfigureSettingAccessMock(new IConfigurationSetting[] { s1, s4, s2, s3 });

            _storage.GetSetting(SqlSProcPublishSettings.TargetTypeSettingName, PublishTargetType.Dacpac).Returns(PublishTargetType.Database);
            _storage.GetSetting(SqlSProcPublishSettings.TargetDatabaseConnectionSettingName, Arg.Any<string>()).Returns(("dbConn2_String"));

            var settings = new SqlSProcPublishSettings(_storage);
            var model = await SqlPublishOptionsDialogViewModel.CreateAsync(settings, _coreShell, _pss, _pcsp);

            model.Settings.TargetType.Should().Be(PublishTargetType.Database);
            model.Settings.TargetDatabaseConnection.Should().Be("dbConn2_String");

            model.Targets.Should().HaveCount(3);
            model.Targets[0].Should().Be("dbConn1");
            model.Targets[1].Should().Be("dbConn2");
            model.Targets[2].Should().Be("dbConn3");
            model.SelectedTargetIndex.Should().Be(1);
        }

        private void ConfigureSettingAccessMock(IEnumerable<IConfigurationSetting> settings) {

            var confProj = Substitute.For<ConfiguredProject>();
            var unconf = Substitute.For<UnconfiguredProject>();
            unconf.LoadedConfiguredProjects.Returns((IEnumerable<ConfiguredProject>)new ConfiguredProject[] { confProj });
            var browseContext = Substitute.For<IVsBrowseObjectContext>();
            browseContext.UnconfiguredProject.Returns(unconf);

            var dteProj = Substitute.For<EnvDTE.Project>();
            dteProj.Object.Returns(browseContext);

            var access = Substitute.For<IProjectConfigurationSettingsAccess>();
            var coll = new ConfigurationSettingCollection();
            foreach (var s in settings) {
                coll.Add(s);
            }
            access.Settings.Returns(coll);
            _pcsp.OpenProjectSettingsAccessAsync(confProj).Returns(Task.FromResult(access));

            object ext;
            var hier = Substitute.For<IVsHierarchy>();
            hier.GetProperty(VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_ExtObject, out ext).Returns((c) => {
                c[2] = dteProj;
                return VSConstants.S_OK;
            });
            _pss.GetSelectedProject<IVsHierarchy>().Returns(hier);
        }
    }
}
