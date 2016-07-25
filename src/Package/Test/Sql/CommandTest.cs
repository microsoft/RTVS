// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Common.Core.Shell;
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
using Microsoft.VisualStudio.R.Package.Sql;
using Microsoft.VisualStudio.Shell.Interop;
using NSubstitute;
using static System.FormattableString;
#if VS14
using Microsoft.VisualStudio.ProjectSystem.Designers;
#elif VS15
using Microsoft.VisualStudio.ProjectSystem.Properties;
#endif

namespace Microsoft.VisualStudio.R.Package.Test.Sql {
    [ExcludeFromCodeCoverage]
    [Category.Sql]
    public class CommandTest {
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

        [Test]
        public void PublishSProcCommandStatus() {
            var coreShell = Substitute.For<ICoreShell>();
            var pss = Substitute.For<IProjectSystemServices>();
            var uc = Substitute.For<UnconfiguredProject>();

            var cmd = new PublishSProcCommand(coreShell, pss, uc);
            var emptySet = ImmutableArray.Create<IProjectTree>().ToImmutableHashSet();
            cmd.GetCommandStatus(emptySet, 0, true, string.Empty, CommandStatus.NotSupported)
                .Should().Be(CommandStatusResult.Unhandled);
            cmd.GetCommandStatus(emptySet, RPackageCommandId.icmdPublishSProc, true, string.Empty, CommandStatus.NotSupported)
                .Should().Be(CommandStatusResult.Unhandled);

            var node = Substitute.For<IProjectTree>();
            node.IsFolder.Returns(true);
            var oneNode = ImmutableArray.Create<IProjectTree>(node).ToImmutableHashSet();
            cmd.GetCommandStatus(oneNode, RPackageCommandId.icmdPublishSProc, true, string.Empty, CommandStatus.NotSupported)
                .Should().Be(CommandStatusResult.Unhandled);

            node = Substitute.For<IProjectTree>();
            node.IsFolder.Returns(false);
            node.FilePath.Returns(@"C:\file.r");
            oneNode = ImmutableArray.Create<IProjectTree>(node).ToImmutableHashSet();
            cmd.GetCommandStatus(oneNode, RPackageCommandId.icmdPublishSProc, true, string.Empty, CommandStatus.NotSupported)
                .Status.Should().Be(CommandStatus.Enabled | CommandStatus.Supported);

            var node1 = Substitute.For<IProjectTree>();
            node1.IsFolder.Returns(false);
            node1.FilePath.Returns(@"C:\file.r");

            var node2 = Substitute.For<IProjectTree>();
            node2.IsFolder.Returns(false);
            node2.FilePath.Returns(@"C:\file.r");

            var node3 = Substitute.For<IProjectTree>();
            node3.IsFolder.Returns(true);

            var nodes = ImmutableArray.Create<IProjectTree>(new IProjectTree[] { node1, node2, node3 }).ToImmutableHashSet();
            cmd.GetCommandStatus(nodes, RPackageCommandId.icmdPublishSProc, true, string.Empty, CommandStatus.NotSupported)
                .Status.Should().Be(CommandStatus.Enabled | CommandStatus.Supported);
        }
    }
}
