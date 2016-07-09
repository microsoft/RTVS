// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.ProjectSystem.Configuration;
using Microsoft.VisualStudio.R.Package.Sql;
using NSubstitute;
using Xunit;
using System.Threading.Tasks;
using FluentAssertions;
#if VS14
using Microsoft.VisualStudio.ProjectSystem.Designers;
#endif

namespace Microsoft.VisualStudio.R.Package.Test.Sql {
    [ExcludeFromCodeCoverage]
    [Collection(CollectionNames.NonParallel)]
    public class CommandTest {
        [Test]
        [Category.Sql]
        public async Task AddDbConnectionCommand() {
            var configuredProject = Substitute.For<ConfiguredProject>();
            var csp = Substitute.For<IProjectConfigurationSettingsProvider>();
            var properties = new ProjectProperties(Substitute.For<ConfiguredProject>());
            var dbcs = Substitute.For<IDbConnectionService>();
            var cmd = new AddDbConnectionCommand(configuredProject, properties, dbcs, csp);

            var result = await cmd.GetCommandStatusAsync(null, RPackageCommandId.icmdAddDabaseConnection, true, string.Empty, CommandStatus.Enabled);
            result.Status.Should().Be(CommandStatus.Enabled);

            result = await cmd.GetCommandStatusAsync(null, 1, true, string.Empty, CommandStatus.Enabled);
            result.Should().Be(CommandStatusResult.Unhandled);


        }
    }
}
