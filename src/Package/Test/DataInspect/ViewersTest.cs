// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Shell;
using Microsoft.R.DataInspection;
using Microsoft.R.Host.Client;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.UnitTests.Core.XUnit.MethodFixtures;
using Microsoft.VisualStudio.R.Package.DataInspect;
using Microsoft.VisualStudio.R.Package.DataInspect.Viewers;
using NSubstitute;
using Xunit;

namespace Microsoft.VisualStudio.R.Package.Test.DataInspect {
    [ExcludeFromCodeCoverage]
    [Collection(CollectionNames.NonParallel)]   // required for tests using R Host 
    [Category.Viewers]
    public class ViewersTest : HostBasedInteractiveTest {
        private const REvaluationResultProperties AllFields = unchecked((REvaluationResultProperties)~0);

        private readonly TestMethodFixture _testMethod;
        private readonly IObjectDetailsViewerAggregator _aggregator;
        private readonly IRSessionCallback _callback = Substitute.For<IRSessionCallback>();

        public ViewersTest(TestMethodFixture testMethod, IServiceContainer services) : base(services, true) {
            _testMethod = testMethod;
            _aggregator = Services.GetService<IObjectDetailsViewerAggregator>();
        }

        [Test]
        public async Task ViewLibraryTest() {
            _callback.ViewLibraryAsync().Returns(Task.CompletedTask);
            await HostScript.InitializeAsync(_callback);

            using (var inter = await HostScript.Session.BeginInteractionAsync()) {
                await inter.RespondAsync("library()" + Environment.NewLine);
            }
            await _callback.Received().ViewLibraryAsync(Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task ViewDataTest01() {
            _callback.When(x => x.ViewObjectAsync(Arg.Any<string>(), Arg.Any<string>())).Do(x => { });
            await HostScript.InitializeAsync(_callback);

            using (var inter = await HostScript.Session.BeginInteractionAsync()) {
                await inter.RespondAsync("View(mtcars)" + Environment.NewLine);
            }
            await _callback.Received().ViewObjectAsync("mtcars", "mtcars", Arg.Any<CancellationToken>());
        }

        [Test]
        public async Task ViewerExportTest() {
            await HostScript.InitializeAsync();
            var session = HostScript.Session;

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

        [CompositeTest]
        [InlineData(null, "lm", "function(formula, data, subset, weights, na.action")]
        [InlineData("`?` <- function(a, b, c) { }", "`?`", "function(a, b, c)")]
        [InlineData("`?` <- function(a, b, c) { }; x <- `?`", "x", "function(a, b, c)")]
        public async Task FunctionViewerTest(string expression, string functionName, string expected) {
            await HostScript.InitializeAsync();

            if (!string.IsNullOrEmpty(expression)) {
                await HostScript.Session.ExecuteAsync(expression);
            }
            var funcViewer = await _aggregator.GetViewer(HostScript.Session, REnvironments.GlobalEnv, functionName) as CodeViewer;
            funcViewer.Should().NotBeNull();

            var code = await funcViewer.GetFunctionCode(functionName);
            code.StartsWithOrdinal(expected).Should().BeTrue();
        }

        [Test]
        public async Task FormulaViewerTest() {
            var formula = "1 ~ 2";

            await HostScript.InitializeAsync();
            var funcViewer = await _aggregator.GetViewer(HostScript.Session, REnvironments.GlobalEnv, formula) as CodeViewer;
            funcViewer.Should().NotBeNull();

            var code = await funcViewer.GetFunctionCode(formula);
            code.StartsWithOrdinal(formula).Should().BeTrue();
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
            var viewer = new GridViewer(Services.GetService<ICoreShell>(), e);
            viewer.CanView(null).Should().BeFalse();

            await HostScript.InitializeAsync();
            var session = HostScript.Session;

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
            await HostScript.InitializeAsync();

            var e = Substitute.For<IDataObjectEvaluator>();
            var viewer = new GridViewer(Services.GetService<ICoreShell>(), e);
            viewer.CanView(null).Should().BeFalse();
            var session = HostScript.Session;

            await session.ExecuteAsync($"x <- {expr}");
            var value = await session.EvaluateAndDescribeAsync("x", AllFields, null);
            viewer.CanView(value).Should().BeFalse();
        }

        [Test]
        public async Task ViewDataTest02() {
            _callback.When(x => x.ViewFile(Arg.Any<string>(), "R data sets", true)).Do(x => { });
            await HostScript.InitializeAsync(_callback);

            using (var inter = await HostScript.Session.BeginInteractionAsync()) {
                await inter.RespondAsync("data()" + Environment.NewLine);
            }
            await _callback.Received().ViewFile(Arg.Any<string>(), "R data sets", true, Arg.Any<CancellationToken>());
        }
    }
}
