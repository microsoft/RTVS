// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Common.Core;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.DataInspection;
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
    public class ViewersTest : IAsyncLifetime {
        private readonly IRSessionProvider _sessionProvider;
        private readonly BrokerFixture _broker;
        private readonly IObjectDetailsViewerAggregator _aggregator;
        private readonly IRInteractiveWorkflow _workflow;

        public ViewersTest(BrokerFixture broker) {
            _sessionProvider = VsAppShell.Current.ExportProvider.GetExportedValue<IRSessionProvider>();
            _aggregator = VsAppShell.Current.ExportProvider.GetExportedValue<IObjectDetailsViewerAggregator>();

            _workflow = VsAppShell.Current.ExportProvider.GetExportedValue<IRInteractiveWorkflowProvider>().GetOrCreate();
            _broker = broker;
        }

        public Task InitializeAsync() => _workflow.Connections.ConnectAsync(_workflow.Connections.ActiveConnection);

        public Task DisposeAsync() => Task.CompletedTask;

        [Test]
        [Category.Viewers]
        public async Task ViewLibraryTest() {
            var cb = Substitute.For<IRSessionCallback>();
            cb.ViewLibrary().Returns(Task.CompletedTask);
            using (var hostScript = new RHostScript(_sessionProvider, _broker.BrokerConnector, cb)) {
                using (var inter = await hostScript.Session.BeginInteractionAsync()) {
                    await inter.RespondAsync("library()" + Environment.NewLine);
                }
            }
            await cb.Received().ViewLibrary();
        }

        [Test]
        [Category.Viewers]
        public async Task ViewDataTest01() {
            var cb = Substitute.For<IRSessionCallback>();
            cb.When(x => x.ViewObject(Arg.Any<string>(), Arg.Any<string>())).Do(x => { });
            using (var hostScript = new RHostScript(_sessionProvider, _broker.BrokerConnector, cb)) {
                using (var inter = await hostScript.Session.BeginInteractionAsync()) {
                    await inter.RespondAsync("View(mtcars)" + Environment.NewLine);
                }
            }
            cb.Received().ViewObject("mtcars", "mtcars");
        }

        [Test]
        [Category.Viewers]
        public async Task ViewerExportTest() {
            using (var hostScript = new RHostScript(_sessionProvider, _broker.BrokerConnector)) {
                var session = hostScript.Session;

                var funcViewer = await _aggregator.GetViewer(session, REnvironments.GlobalEnv, "lm");
                funcViewer.Should().NotBeNull().And.BeOfType<CodeViewer>();

                var gridViewer = await _aggregator.GetViewer(session, REnvironments.GlobalEnv, "airmiles");
                gridViewer.Should().NotBeNull().And.BeOfType<Viewer1D>();

                gridViewer = await _aggregator.GetViewer(session, REnvironments.GlobalEnv, "mtcars");
                gridViewer.Should().NotBeNull().And.BeOfType<TableViewer>();

                gridViewer = await _aggregator.GetViewer(session, REnvironments.GlobalEnv, "AirPassengers");
                gridViewer.Should().NotBeNull().And.BeOfType<Viewer1D>();

                gridViewer = await _aggregator.GetViewer(session, REnvironments.GlobalEnv, "list(c(1:10))");
                gridViewer.Should().NotBeNull().And.BeOfType<ListViewer>();

                gridViewer = await _aggregator.GetViewer(session, REnvironments.GlobalEnv, "c(1:10)");
                gridViewer.Should().NotBeNull().And.BeOfType<VectorViewer>();

                gridViewer.Capabilities.Should().HaveFlag(ViewerCapabilities.List | ViewerCapabilities.Table);
            }
        }

        [CompositeTest]
        [Category.Viewers]
        [InlineData(null, "lm", "function(formula, data, subset, weights, na.action")]
        [InlineData("`?` <- function(a, b, c) { }", "`?`", "function(a, b, c)")]
        [InlineData("`?` <- function(a, b, c) { }; x <- `?`", "x", "function(a, b, c)")]
        public async Task FunctionViewerTest(string expression, string functionName, string expected) {
            using (var hostScript = new RHostScript(_sessionProvider, _broker.BrokerConnector)) {
                if(!string.IsNullOrEmpty(expression)) {
                    await hostScript.Session.ExecuteAsync(expression);
                }
                var funcViewer = await _aggregator.GetViewer(hostScript.Session, REnvironments.GlobalEnv, functionName) as CodeViewer;
                funcViewer.Should().NotBeNull();

                var code = await funcViewer.GetFunctionCode(functionName);
                code.StartsWithOrdinal(expected).Should().BeTrue();
            }
        }

        [Test]
        [Category.Viewers]
        public async Task FormulaViewerTest() {
            using (var hostScript = new RHostScript(_sessionProvider, _broker.BrokerConnector)) {
                string formula = "1 ~ 2";

                var funcViewer = await _aggregator.GetViewer(hostScript.Session, REnvironments.GlobalEnv, formula) as CodeViewer;
                funcViewer.Should().NotBeNull();

                var code = await funcViewer.GetFunctionCode(formula);
                code.StartsWithOrdinal(formula).Should().BeTrue();
            }
        }

        [Test]
        [Category.Viewers]
        public void TableViewerTest() {
            var e = Substitute.For<IDataObjectEvaluator>();
            var viewer = new TableViewer(_aggregator, e);
 
            var eval = Substitute.For<IRValueInfo>();
            eval.Classes.Returns(new List<string>() { "foo" });

            viewer.CanView(null).Should().BeFalse();
            viewer.CanView(eval).Should().BeFalse();

            eval.Dim.Count.Returns(0);
            viewer.CanView(eval).Should().BeFalse();

            foreach (var c in new string[] { "matrix", "data.frame", "table", "array" }) {
                eval.Classes.Returns(new List<string>() { c });
                eval.Dim.Count.Returns(3);
                viewer.CanView(eval).Should().BeFalse();
                eval.Dim.Count.Returns(2);
                viewer.CanView(eval).Should().BeTrue();
                eval.Dim.Count.Returns(1);
                viewer.CanView(eval).Should().BeFalse();
                eval.Dim.Count.Returns(0);
                viewer.CanView(eval).Should().BeFalse();
            }

            eval.Dim.Returns((IReadOnlyList<int>)null);
            foreach (var c in new string[] { "a", "b" }) {
                eval.Classes.Returns(new List<string>() { c });
                viewer.CanView(eval).Should().BeFalse();
            }

            eval.Classes.Returns(new List<string>() { "foo", "bar" });
            viewer.CanView(eval).Should().BeFalse();
        }

        [Test]
        [Category.Viewers]
        public void Viewer1DTest() {
            var e = Substitute.For<IDataObjectEvaluator>();
            var viewer = new Viewer1D(_aggregator, e);

            var eval = Substitute.For<IRValueInfo>();
            eval.Classes.Returns(new List<string>() { "environment" });

            viewer.CanView(null).Should().BeFalse();
            viewer.CanView(eval).Should().BeFalse();

            eval.Dim.Count.Returns(0);
            viewer.CanView(eval).Should().BeFalse();

            eval.Length.Returns(2);
            foreach (var c in new string[] { "ts", "array" }) {
                eval.Classes.Returns(new List<string>() { c });
                eval.Dim.Count.Returns(2);
                viewer.CanView(eval).Should().BeFalse();
                eval.Dim.Count.Returns(1);
                viewer.CanView(eval).Should().BeTrue();
                eval.Dim.Count.Returns(0);
                viewer.CanView(eval).Should().BeFalse();
            }

            eval.Dim.Returns((IReadOnlyList<int>)null);
            foreach (var c in new string[] { "ts", "array" }) {
                eval.Classes.Returns(new List<string>() { c });
                viewer.CanView(eval).Should().BeTrue();
            }

            eval.Dim.Returns((IReadOnlyList<int>)null);
            foreach (var c in new string[] { "a", "b" }) {
                eval.Classes.Returns(new List<string>() { c });
                viewer.CanView(eval).Should().BeFalse();
            }
        }

        [Test]
        [Category.Viewers]
        public async Task ViewDataTest02() {
            var cb = Substitute.For<IRSessionCallback>();
            cb.When(x => x.ViewFile(Arg.Any<string>(), "R data sets", true)).Do(x => { });
            using (var hostScript = new RHostScript(_sessionProvider, _broker.BrokerConnector, cb)) {
                using (var inter = await hostScript.Session.BeginInteractionAsync()) {
                    await inter.RespondAsync("data()" + Environment.NewLine);
                }
            }
            await cb.Received().ViewFile(Arg.Any<string>(), "R data sets", true);
        }
    }
}
