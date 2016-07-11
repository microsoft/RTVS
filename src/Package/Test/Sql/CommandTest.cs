// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.R.Components.Application.Configuration;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Host.Client;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.ProjectSystem.Configuration;
using Microsoft.VisualStudio.R.Package.Sql;
using NSubstitute;
using static System.FormattableString;
#if VS14
using Microsoft.VisualStudio.ProjectSystem.Designers;
#endif

namespace Microsoft.VisualStudio.R.Package.Test.Sql {
    [ExcludeFromCodeCoverage]
    public class CommandTest {
        [Test]
        [Category.Sql]
        public async Task AddDbConnectionCommand() {
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
            var wf = Substitute.For<IRInteractiveWorkflow>();
            wf.RSession.Returns(session);
            var wfp = Substitute.For<IRInteractiveWorkflowProvider>();
            wfp.GetOrCreate().Returns(wf);

            var cmd = new AddDbConnectionCommand(configuredProject, properties, dbcs, csp, wfp);

            var result = await cmd.GetCommandStatusAsync(null, RPackageCommandId.icmdAddDatabaseConnection, true, string.Empty, CommandStatus.Enabled);
            result.Status.Should().Be(CommandStatus.Supported | CommandStatus.Enabled);

            result = await cmd.GetCommandStatusAsync(null, 1, true, string.Empty, CommandStatus.Enabled);
            result.Should().Be(CommandStatusResult.Unhandled);

            var f = await cmd.TryHandleCommandAsync(null, RPackageCommandId.icmdAddDatabaseConnection, true, 0, IntPtr.Zero, IntPtr.Zero);
            f.Should().BeTrue();

            coll.Should().HaveCount(4);
            var s = coll.GetSetting("dbConnection3");
            s.Value.Should().Be("DSN");
            s.ValueType.Should().Be(ConfigurationSettingValueType.String);
            s.Category.Should().Be(ConnectionStringEditor.ConnectionStringEditorCategory);
            s.EditorType.Should().Be(ConnectionStringEditor.ConnectionStringEditorName);
            s.Description.Should().Be(Resources.ConnectionStringDescription);

            await session.Received(1).EvaluateAsync(Invariant($"dbConnection3 <- 'DSN'"), REvaluationKind.Mutating);
        }
    }
}
