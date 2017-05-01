// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.DataInspection;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Test.Script;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.UnitTests.Core.XUnit.MethodFixtures;
using Microsoft.VisualStudio.R.Package.DataInspect;
using Microsoft.VisualStudio.R.Package.DataInspect.Viewers;
using Microsoft.VisualStudio.R.Package.Shell;
using NSubstitute;
using Xunit;

namespace Microsoft.VisualStudio.R.Package.Test.DataInspect {
    [ExcludeFromCodeCoverage]
    [Collection(CollectionNames.NonParallel)]   // required for tests using R Host 
    [Category.Viewers]
    public class ViewersTest : IAsyncLifetime {
        private const REvaluationResultProperties AllFields = unchecked((REvaluationResultProperties)~0);

        private readonly TestMethodFixture _testMethod;
        private readonly IRSessionProvider _sessionProvider;
        private readonly IObjectDetailsViewerAggregator _aggregator;
        private readonly IRInteractiveWorkflow _workflow;

        public ViewersTest(TestMethodFixture testMethod) {

            _testMethod = testMethod;
            _aggregator = VsAppShell.Current.GetService<IObjectDetailsViewerAggregator>();
            _workflow = VsAppShell.Current.GetService<IRInteractiveWorkflowProvider>().GetOrCreate();
            _sessionProvider = _workflow.RSessions;
        }

        public Task InitializeAsync() => _workflow.Connections.ConnectAsync(_workflow.Connections.ActiveConnection);

        public Task DisposeAsync() => Task.CompletedTask;

        [Test]
        public async Task ViewLibraryTest() {
            var cb = Substitute.For<IRSessionCallback>();
            cb.ViewLibraryAsync().Returns(Task.CompletedTask);
            using (var hostScript = new RHostScript(_workflow.RSessions, cb)) {
                using (var inter = await hostScript.Session.BeginInteractionAsync()) {
                    await inter.RespondAsync("library()" + Environment.NewLine);
                }
            }
            await cb.Received().ViewLibraryAsync(Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task ViewDataTest01() {
            var cb = Substitute.For<IRSessionCallback>();
            cb.When(x => x.ViewObjectAsync(Arg.Any<string>(), Arg.Any<string>())).Do(x => { });
            using (var hostScript = new RHostScript(_workflow.RSessions, cb)) {
                using (var inter = await hostScript.Session.BeginInteractionAsync()) {
                    await inter.RespondAsync("View(mtcars)" + Environment.NewLine);
                }
            }
            await cb.Received().ViewObjectAsync("mtcars", "mtcars", Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task ViewerExportTest() {
            using (var hostScript = new RHostScript(_sessionProvider)) {
                var session = hostScript.Session;

                var funcViewer = await _aggregator.GetViewer(session, REnvironments.GlobalEnv, "lm");
                funcViewer.Should().BeOfType<CodeViewer>();

                var gridViewer = await _aggregator.GetViewer(session, REnvironments.GlobalEnv, "airmiles");
                gridViewer.Should().BeOfType<GridViewer>();

                gridViewer = await _aggregator.GetViewer(session, REnvironments.GlobalEnv, "mtcars");
                gridViewer.Should().BeOfType<GridViewer>();

                gridViewer = await _aggregator.GetViewer(session, REnvironments.GlobalEnv, "AirPassengers");
                gridViewer.Should().BeOfType<GridViewer>();

                gridViewer = await _aggregator.GetViewer(session, REnvironments.GlobalEnv, "as.list(c(1:10))");
                gridViewer.Should().BeOfType<GridViewer>();

                gridViewer = await _aggregator.GetViewer(session, REnvironments.GlobalEnv, "c(1:10)");
                gridViewer.Should().BeOfType<GridViewer>();

                gridViewer.Capabilities.Should().HaveFlag(ViewerCapabilities.List | ViewerCapabilities.Table);
            }
        }

        [CompositeTest]
        [InlineData(null, "lm", "function(formula, data, subset, weights, na.action")]
        [InlineData("`?` <- function(a, b, c) { }", "`?`", "function(a, b, c)")]
        [InlineData("`?` <- function(a, b, c) { }; x <- `?`", "x", "function(a, b, c)")]
        public async Task FunctionViewerTest(string expression, string functionName, string expected) {
            using (var hostScript = new RHostScript(_workflow.RSessions)) {
                if (!string.IsNullOrEmpty(expression)) {
                    await hostScript.Session.ExecuteAsync(expression);
                }
                var funcViewer = await _aggregator.GetViewer(hostScript.Session, REnvironments.GlobalEnv, functionName) as CodeViewer;
                funcViewer.Should().NotBeNull();

                var code = await funcViewer.GetFunctionCode(functionName);
                code.StartsWithOrdinal(expected).Should().BeTrue();
            }
        }

        [Test]
        public async Task FormulaViewerTest() {
            using (var hostScript = new RHostScript(_workflow.RSessions)) {
                string formula = "1 ~ 2";

                var funcViewer = await _aggregator.GetViewer(hostScript.Session, REnvironments.GlobalEnv, formula) as CodeViewer;
                funcViewer.Should().NotBeNull();

                var code = await funcViewer.GetFunctionCode(formula);
                code.StartsWithOrdinal(formula).Should().BeTrue();
            }
        }

        [CompositeTest]
        [InlineData("as.list")]
        [InlineData("as.logical")]
        [InlineData("as.integer")]
        [InlineData("as.double")]
        [InlineData("as.character")]
        [InlineData("as.complex")]
        public async Task GridViewerDimLengthTest(string cast) {
            var e = Substitute.For<IDataObjectEvaluator>();
            var viewer = new GridViewer(VsAppShell.Current, e);
            viewer.CanView(null).Should().BeFalse();

            using (var hostScript = new RHostScript(_sessionProvider)) {
                var session = hostScript.Session;

                await session.ExecuteAsync($"x <- {cast}(c())");
                var value = await session.EvaluateAndDescribeAsync("x", AllFields, null);
                viewer.CanView(value).Should().BeFalse();

                await session.ExecuteAsync($"x <- {cast}(1)");
                value = await session.EvaluateAndDescribeAsync("x", AllFields, null);
                viewer.CanView(value).Should().BeFalse();

                value = await session.EvaluateAndDescribeAsync("dim(x) <- 1", AllFields, null);
                value = await session.EvaluateAndDescribeAsync("x", AllFields, null);
                viewer.CanView(value).Should().BeFalse();

                value = await session.EvaluateAndDescribeAsync("dim(x) <- c(1, 1)", AllFields, null);
                value = await session.EvaluateAndDescribeAsync("x", AllFields, null);
                viewer.CanView(value).Should().BeTrue();

                value = await session.EvaluateAndDescribeAsync("dim(x) <- c(1, 1, 1)", AllFields, null);
                value = await session.EvaluateAndDescribeAsync("x", AllFields, null);
                viewer.CanView(value).Should().BeFalse();

                await session.ExecuteAsync($"x <- {cast}(1:100)");
                value = await session.EvaluateAndDescribeAsync("x", AllFields, null);
                viewer.CanView(value).Should().BeTrue();

                await session.ExecuteAsync($"dim(x) <- 100");
                value = await session.EvaluateAndDescribeAsync("x", AllFields, null);
                viewer.CanView(value).Should().BeTrue();

                await session.ExecuteAsync($"dim(x) <- c(10, 10)");
                value = await session.EvaluateAndDescribeAsync("x", AllFields, null);
                viewer.CanView(value).Should().BeTrue();

                await session.ExecuteAsync($"dim(x) <- c(10, 5, 2)");
                value = await session.EvaluateAndDescribeAsync("x", AllFields, null);
                viewer.CanView(value).Should().BeFalse();
            }
        }

        [CompositeTest]
        [InlineData("NULL")]
        [InlineData(".GlobalEnv")]
        [InlineData("pairlist(1)")]
        [InlineData("function() {}")]
        [InlineData("base::c")]
        [InlineData("quote(x)")]
        [InlineData("quote(1 + 2)")]
        [InlineData("parse(text = '1; 2')")]
        [InlineData("1 ~ 2")]
        [InlineData("setClass('X', representation(x = 'logical'))()")]
        public async Task GridViewerExcludeTest(string expr) {
            var e = Substitute.For<IDataObjectEvaluator>();
            var viewer = new GridViewer(VsAppShell.Current, e);
            viewer.CanView(null).Should().BeFalse();

            using (var hostScript = new RHostScript(_sessionProvider)) {
                var session = hostScript.Session;

                await session.ExecuteAsync($"x <- {expr}");
                var value = await session.EvaluateAndDescribeAsync("x", AllFields, null);
                viewer.CanView(value).Should().BeFalse();
            }
        }

        [Test]
        public async Task ViewDataTest02() {
            var cb = Substitute.For<IRSessionCallback>();
            cb.When(x => x.ViewFile(Arg.Any<string>(), "R data sets", true)).Do(x => { });
            using (var hostScript = new RHostScript(_sessionProvider, cb)) {
                using (var inter = await hostScript.Session.BeginInteractionAsync()) {
                    await inter.RespondAsync("data()" + Environment.NewLine);
                }
            }
            await cb.Received().ViewFile(Arg.Any<string>(), "R data sets", true, Arg.Any<CancellationToken>());
        }
    }
}
