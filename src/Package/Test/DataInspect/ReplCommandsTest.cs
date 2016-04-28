// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Test.Script;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.R.Package.DataInspect;
using Microsoft.VisualStudio.R.Package.DataInspect.Viewers;
using Microsoft.VisualStudio.R.Package.Shell;
using NSubstitute;
using Xunit;

namespace Microsoft.VisualStudio.R.Package.Test.DataInspect {
    [ExcludeFromCodeCoverage]
    [Collection(CollectionNames.NonParallel)]   // required for tests using R Host 
    public class ReplCommandsTest {
        [Test]
        [Category.Variable.Explorer]
        public async Task ViewLibraryTest() {
            var provider = VsAppShell.Current.ExportProvider.GetExportedValue<IRSessionProvider>();
            var cb = Substitute.For<IRSessionCallback>();
            cb.ViewLibrary().Returns(Task.CompletedTask);
            using (var hostScript = new RHostScript(provider, cb)) {
                using (var inter = await hostScript.Session.BeginInteractionAsync()) {
                    await inter.RespondAsync("library()" + Environment.NewLine);
                }
            }
            await cb.Received().ViewLibrary();
        }

        [Test]
        [Category.Variable.Explorer]
        public async Task ViewDataTest01() {
            var provider = VsAppShell.Current.ExportProvider.GetExportedValue<IRSessionProvider>();
            var cb = Substitute.For<IRSessionCallback>();
            cb.When(x => x.ViewObject(Arg.Any<string>(), Arg.Any<string>())).Do(x => { });
            using (var hostScript = new RHostScript(provider, cb)) {
                using (var inter = await hostScript.Session.BeginInteractionAsync()) {
                    await inter.RespondAsync("View(mtcars)" + Environment.NewLine);
                }
            }
            cb.Received().ViewObject("mtcars", "");
        }

        [Test]
        [Category.Variable.Explorer]
        public async Task ViewerExportTest() {
            var provider = VsAppShell.Current.ExportProvider.GetExportedValue<IRSessionProvider>();
            using (var hostScript = new RHostScript(provider)) {
                var aggregator = VsAppShell.Current.ExportProvider.GetExportedValue<IObjectDetailsViewerAggregator>();
                aggregator.Should().NotBeNull();

                var funcViewer = await aggregator.GetViewer("lm");
                funcViewer.Should().NotBeNull().And.BeOfType<FunctionViewer>();

                var gridViewer = await aggregator.GetViewer("airmiles");
                gridViewer.Should().NotBeNull().And.BeOfType<GridViewer>();

                gridViewer = await aggregator.GetViewer("mtcars");
                gridViewer.Should().NotBeNull().And.BeOfType<GridViewer>();

                gridViewer = await aggregator.GetViewer("AirPassengers");
                gridViewer.Should().NotBeNull().And.BeOfType<GridViewer>();
            }
        }
    }
}
