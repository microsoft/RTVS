// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Common.Core;
using Microsoft.R.Debugger;
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
        private readonly IRSessionProvider _sessionProvider;
        private readonly IObjectDetailsViewerAggregator _aggregator;

        public ReplCommandsTest() {
            _sessionProvider = VsAppShell.Current.ExportProvider.GetExportedValue<IRSessionProvider>();
            _aggregator = VsAppShell.Current.ExportProvider.GetExportedValue<IObjectDetailsViewerAggregator>();
        }

        [Test]
        [Category.Variable.Explorer]
        public async Task ViewLibraryTest() {
            var cb = Substitute.For<IRSessionCallback>();
            cb.ViewLibrary().Returns(Task.CompletedTask);
            using (var hostScript = new RHostScript(_sessionProvider, cb)) {
                using (var inter = await hostScript.Session.BeginInteractionAsync()) {
                    await inter.RespondAsync("library()" + Environment.NewLine);
                }
            }
            await cb.Received().ViewLibrary();
        }

        [Test]
        [Category.Variable.Explorer]
        public async Task ViewDataTest01() {
            var cb = Substitute.For<IRSessionCallback>();
            cb.When(x => x.ViewObject(Arg.Any<string>(), Arg.Any<string>())).Do(x => { });
            using (var hostScript = new RHostScript(_sessionProvider, cb)) {
                using (var inter = await hostScript.Session.BeginInteractionAsync()) {
                    await inter.RespondAsync("View(mtcars)" + Environment.NewLine);
                }
            }
            cb.Received().ViewObject("mtcars", "");
        }

        [Test]
        [Category.Variable.Explorer]
        public async Task ViewerExportTest() {
            using (var hostScript = new RHostScript(_sessionProvider)) {
                var funcViewer = await _aggregator.GetViewer("lm");
                funcViewer.Should().NotBeNull().And.BeOfType<FunctionViewer>();

                var gridViewer = await _aggregator.GetViewer("airmiles");
                gridViewer.Should().NotBeNull().And.BeOfType<Viewer1D>();

                gridViewer = await _aggregator.GetViewer("mtcars");
                gridViewer.Should().NotBeNull().And.BeOfType<TableViewer>();

                gridViewer = await _aggregator.GetViewer("AirPassengers");
                gridViewer.Should().NotBeNull().And.BeOfType<Viewer1D>();

                gridViewer = await _aggregator.GetViewer("list(c(1:10))");
                gridViewer.Should().NotBeNull().And.BeOfType<ListViewer>();

                gridViewer = await _aggregator.GetViewer("c(1:10)");
                gridViewer.Should().NotBeNull().And.BeOfType<VectorViewer>();

                gridViewer.Capabilities.Should().HaveFlag(ViewerCapabilities.List | ViewerCapabilities.Table);
            }
        }

        [Test]
        [Category.Variable.Explorer]
        public void ViewerTest02() {
            var odv = _aggregator as ObjectDetailsViewerAggregator;

            var result1 = Substitute.For<IDebugValueEvaluationResult>();
            var result2 = Substitute.For<IDebugValueEvaluationResult>();
            var result3 = Substitute.For<IDebugValueEvaluationResult>();

            var viewer1 = Substitute.For<IObjectDetailsViewer>();
            viewer1.CanView(result1).Returns(true);

            var viewer2 = Substitute.For<IObjectDetailsViewer>();
            viewer2.CanView(result2).Returns(true);

            var viewers = new List<Lazy<IObjectDetailsViewer>>();
            viewers.Add(new Lazy<IObjectDetailsViewer>(() => viewer1));
            viewers.Add(new Lazy<IObjectDetailsViewer>(() => viewer2));

            var old = odv.Viewers;
            odv.Viewers = viewers;

            IObjectDetailsViewer v1, v2, v3;

            v1 = _aggregator.GetViewer(result1);
            v2 = _aggregator.GetViewer(result2);
            v3 = _aggregator.GetViewer(result3);

            odv.Viewers = old;

            v1.Should().Be(viewer1);
            v2.Should().Be(viewer2);
            v3.Should().BeNull();
        }

        [Test]
        [Category.Variable.Explorer]
        public async Task ViewerTest03() {
            var viewer = Substitute.For<IObjectDetailsViewer>();
            viewer.ViewAsync(Arg.Any<string>(), Arg.Any<string>()).Returns(Task.CompletedTask);

            var evaluator = Substitute.For<IDataObjectEvaluator>();

            var aggregator = Substitute.For<IObjectDetailsViewerAggregator>();
            aggregator.GetViewer(Arg.Any<string>()).Returns(viewer);

            var provider = new ObjectDetailsViewerProvider(aggregator, evaluator);
            await provider.ViewObjectDetails("mtcars", null);

            await viewer.Received().ViewAsync("mtcars", null);
        }

        [Test]
        [Category.Variable.Explorer]
        public async Task FunctionViewerTest() {
            using (var hostScript = new RHostScript(_sessionProvider)) {
                var funcViewer = await _aggregator.GetViewer("lm") as FunctionViewer;
                funcViewer.Should().NotBeNull();

                funcViewer.GetFileName("lm", null).Should().Be(Path.Combine(Path.GetTempPath(), "~lm.r"));
                funcViewer.GetFileName("lm", "abc").Should().Be(Path.Combine(Path.GetTempPath(), "~abc.r"));

                var code = await funcViewer.GetFunctionCode("lm");
                code.StartsWithOrdinal("function(formula, data, subset, weights, na.action").Should().BeTrue();
            }
        }

        [Test]
        [Category.Variable.Explorer]
        public async Task TableViewerTest() {
            using (var hostScript = new RHostScript(_sessionProvider)) {
                var gridViewer = await _aggregator.GetViewer("mtcars") as TableViewer;
                gridViewer.Should().NotBeNull();

                var eval = Substitute.For<IDebugValueEvaluationResult>();
                eval.Classes.Returns(new List<string>() { "foo" });

                gridViewer.CanView(null).Should().BeFalse();
                gridViewer.CanView(eval).Should().BeFalse();

                eval.Dim.Count.Returns(0);
                gridViewer.CanView(eval).Should().BeFalse();

                foreach (var c in new string[] { "matrix", "data.frame", "table", "array" }) {
                    eval.Classes.Returns(new List<string>() { c });
                    eval.Dim.Count.Returns(3);
                    gridViewer.CanView(eval).Should().BeFalse();
                    eval.Dim.Count.Returns(2);
                    gridViewer.CanView(eval).Should().BeTrue();
                    eval.Dim.Count.Returns(1);
                    gridViewer.CanView(eval).Should().BeFalse();
                    eval.Dim.Count.Returns(0);
                    gridViewer.CanView(eval).Should().BeFalse();
                }

                eval.Dim.Returns((IReadOnlyList<int>)null);
                foreach (var c in new string[] { "a", "b" }) {
                    eval.Classes.Returns(new List<string>() { c });
                    gridViewer.CanView(eval).Should().BeFalse();
                }

                eval.Classes.Returns(new List<string>() { "foo", "bar" });
                gridViewer.CanView(eval).Should().BeFalse();
            }
        }

        [Test]
        [Category.Variable.Explorer]
        public async Task Viewer1DTest() {
            using (var hostScript = new RHostScript(_sessionProvider)) {
                var viewer = await _aggregator.GetViewer("airmiles") as Viewer1D;
                viewer.Should().NotBeNull();

                var eval = Substitute.For<IDebugValueEvaluationResult>();
                eval.Classes.Returns(new List<string>() { "foo" });

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
        }
    }
}
