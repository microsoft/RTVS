// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.R.Components.Application.Configuration;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Components.Sql;
using Microsoft.R.Host.Client;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.ProjectSystem;
using Microsoft.VisualStudio.R.Package.ProjectSystem.Configuration;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Package.Sql;
using Microsoft.VisualStudio.Shell.Interop;
using NSubstitute;
using static System.FormattableString;
using Microsoft.R.Components.Sql.Publish;

#if VS14
using Microsoft.VisualStudio.ProjectSystem.Designers;
#elif VS15
using Microsoft.VisualStudio.ProjectSystem.Properties;
#endif

namespace Microsoft.VisualStudio.R.Package.Test.Sql {
    [ExcludeFromCodeCoverage]
    [Category.Sql]
    public class CommandTest {
        private readonly IApplicationShell _appShell;
        private readonly IProjectSystemServices _pss;

        public CommandTest() {
            _appShell = Substitute.For<IApplicationShell>();
            _pss = Substitute.For<IProjectSystemServices>();
        }

        [Test(ThreadType.UI)]
        public void AddDbConnectionCommand() {
            var coll = new ConfigurationSettingCollection();
            coll.Add(new ConfigurationSetting("dbConnection1", "1", ConfigurationSettingValueType.String));
            coll.Add(new ConfigurationSetting("dbConnection2", "1", ConfigurationSettingValueType.String));
            coll.Add(new ConfigurationSetting("dbConnection4", "1", ConfigurationSettingValueType.String));

            var configuredProject = Substitute.For<ConfiguredProject>();
            var ac = Substitute.For<IProjectConfigurationSettingsAccess>();
            ac.Settings.Returns(coll);
            var csp = Substitute.For<IProjectConfigurationSettingsProvider>();
            csp.OpenProjectSettingsAccessAsync(configuredProject).Returns(ac);

            var properties = new ProjectProperties(Substitute.For<ConfiguredProject>());
            var dbcs = Substitute.For<IDbConnectionService>();
            dbcs.EditConnectionString(null).Returns("DSN");

            var session = Substitute.For<IRSession>();
            session.EvaluateAsync(null, REvaluationKind.NoResult).ReturnsForAnyArgs(Task.FromResult(new REvaluationResult()));
            session.IsHostRunning.Returns(true);
            var operations = Substitute.For<IRInteractiveWorkflowOperations>();

            var workflow = Substitute.For<IRInteractiveWorkflow>();
            workflow.RSession.Returns(session);
            workflow.Operations.Returns(operations);

            var ucp = Substitute.For<UnconfiguredProject>();
            ucp.LoadedConfiguredProjects.Returns(new ConfiguredProject[] { configuredProject });
            var vsbc = Substitute.For<IVsBrowseObjectContext>();
            vsbc.UnconfiguredProject.Returns(ucp); // IVsBrowseObjectContext.ConfiguredProject

            var dteProj = Substitute.For<EnvDTE.Project>();
            dteProj.Object.Returns(vsbc); // dteProject?.Object as IVsBrowseObjectContext

            object extObject;
            var hier = Substitute.For<IVsHierarchy>();
            hier.GetProperty(VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_ExtObject, out extObject)
                .Returns(x => {
                    x[2] = dteProj;
                    return VSConstants.S_OK;
                });

            var pss = Substitute.For<IProjectSystemServices>();
            pss.GetSelectedProject<IVsHierarchy>().Returns(hier);

            var cmd = new AddDbConnectionCommand(dbcs, pss, csp, workflow);
            cmd.Enabled.Should().BeTrue();
            cmd.Invoke(null, IntPtr.Zero, OLECMDEXECOPT.OLECMDEXECOPT_DODEFAULT);

            coll.Should().HaveCount(4);
            var s = coll.GetSetting("dbConnection3");
            s.Value.Should().Be("DSN");
            s.ValueType.Should().Be(ConfigurationSettingValueType.String);
            s.Category.Should().Be(ConnectionStringEditor.ConnectionStringEditorCategory);
            s.EditorType.Should().Be(ConnectionStringEditor.ConnectionStringEditorName);
            s.Description.Should().Be(Resources.ConnectionStringDescription);

            operations.Received(1).EnqueueExpression(Invariant($"dbConnection3 <- 'DSN'"), true);
        }

        [Test(ThreadType.UI)]
        public void PublishSProcCommandStatus() {
            var servicesProvider = Substitute.For<IDacPackageServicesProvider>();
            servicesProvider.GetDacPackageServices().Returns(Substitute.For<IDacPackageServices>());

            var cmd = new PublishSProcCommand(_appShell, _pss, servicesProvider);
            cmd.GetCommandStatus(null, 0, true, null, CommandStatus.NotSupported).Should().Be(CommandStatusResult.Unhandled);
            cmd.GetCommandStatus(null, RPackageCommandId.icmdPublishSProc, true, null, CommandStatus.NotSupported)
                .Should().Be(new CommandStatusResult(true, null, CommandStatus.Enabled | CommandStatus.Supported));
            cmd.TryHandleCommand(null, 0, false, 0, IntPtr.Zero, IntPtr.Zero).Should().BeFalse();
        }

        [Test(ThreadType.UI)]
        public void PublishSProcCommandHandle01() {
            _pss.GetSelectedProject<IVsHierarchy>().Returns((IVsHierarchy)null);

            _pss.GetProjectFiles(null).ReturnsForAnyArgs(new string[] { "file.r" });
            var hier = Substitute.For<IVsHierarchy>();

            object ext;
            hier.GetProperty(VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_ExtObject, out ext).Returns((c) => {
                c[2] = Substitute.For<EnvDTE.Project>();
                return VSConstants.S_OK;
            });
            _pss.GetSelectedProject<IVsHierarchy>().Returns(hier);

            var servicesProvider = Substitute.For<IDacPackageServicesProvider>();
            servicesProvider.GetDacPackageServices().Returns(Substitute.For<IDacPackageServices>());

            var cmd = new PublishSProcCommand(_appShell, _pss, servicesProvider);
            cmd.TryHandleCommand(null, RPackageCommandId.icmdPublishSProc, false, 0, IntPtr.Zero, IntPtr.Zero).Should().BeTrue();
            //_appShell.Received().ShowErrorMessage(Resources.SqlPublishDialog_NoDbProject);
        }

        [Test(ThreadType.UI)]
        public void PublishSProcCommandHandle02() {
            var project = Substitute.For<EnvDTE.Project>();
            project.FileName.Returns("db.sqlproj");

            _pss.GetProjectFiles(null).ReturnsForAnyArgs(new string[] { "file.r" });

            object ext;
            var hier = Substitute.For<IVsHierarchy>();
            hier.GetProperty(VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_ExtObject, out ext).Returns((c) => {
                c[2] = project;
                return VSConstants.S_OK;
            });
            _pss.GetSelectedProject<IVsHierarchy>().Returns(hier);

            var sol = Substitute.For<EnvDTE.Solution>();
            var projects = Substitute.For<EnvDTE.Projects>();
            projects.GetEnumerator().Returns((new EnvDTE.Project[] { project }).GetEnumerator());
            sol.Projects.Returns(projects);
            _pss.GetSolution().Returns(sol);

            var servicesProvider = Substitute.For<IDacPackageServicesProvider>();
            servicesProvider.GetDacPackageServices().Returns(Substitute.For<IDacPackageServices>());

            var cmd = new PublishSProcCommand(_appShell, _pss, servicesProvider);
            cmd.TryHandleCommand(null, RPackageCommandId.icmdPublishSProc, false, 0, IntPtr.Zero, IntPtr.Zero).Should().BeTrue();
            _appShell.Received().ShowErrorMessage(Resources.SqlPublishDialog_NoSProcFiles);
        }
    }
}
