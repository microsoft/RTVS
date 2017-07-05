// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using FluentAssertions;
using Microsoft.Common.Core.Services;
using Microsoft.R.Host.Client.Session;
using Microsoft.R.Host.Client.Test.Script;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.UnitTests.Core.XUnit.MethodFixtures;
using Xunit;

namespace Microsoft.R.Host.Client.Test {
    [ExcludeFromCodeCoverage]
    [Collection(CollectionNames.NonParallel)]
    public class XamlGraphicsDeviceTest {
        private const string Ns = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
        private const string GridPrefixCode = "rtvs:::graphics.xaml(\"{0}\", {1}, {2});library(grid);grid.newpage();\n";
        private const string GridSuffixCode = "dev.off()\n";

        private const int DefaultWidth = 360;
        private const int DefaultHeight = 360;

        private readonly IServiceContainer _services;
        private readonly MethodInfo _testMethod;

        public XamlGraphicsDeviceTest(IServiceContainer services, TestMethodFixture testMethod) {
            _services = services;
            _testMethod = testMethod.MethodInfo;
        }

        private double X(double percentX) => DefaultWidth * percentX;

        private double Y(double percentY) => DefaultHeight - DefaultHeight * percentY;

        private double W(double percentX) => DefaultWidth * percentX;

        private double H(double percentY) => DefaultHeight * percentY;

        [Test]
        [Category.Plots]
        public async Task Line() {
            var code = @"grid.segments(.01, .1, .99, .1)";
            var doc = await GridTest(code);
            var shapes = doc.Descendants(XName.Get("Line", Ns)).ToList();
            shapes.Should().ContainSingle();

            CheckX1Y1X2Y2(shapes[0], X(0.01), Y(0.1), X(0.99), Y(0.1));
            CheckStrokeThickness(shapes[0], 1);
            CheckStroke(shapes[0], "#FF000000");
            CheckStrokeDashArray(shapes[0], null);
        }

        [Test]
        [Category.Plots]
        public async Task LineCustomLineType() {
            var code = @"grid.segments(.01, .1, .99, .1, gp=gpar(lty='4812',lwd=5,col='Blue'))";
            var doc = await GridTest(code);
            var shapes = doc.Descendants(XName.Get("Line", Ns)).ToList();
            shapes.Should().ContainSingle();
            CheckX1Y1X2Y2(shapes[0], X(0.01), Y(0.1), X(0.99), Y(0.1));
            CheckStrokeThickness(shapes[0], 5);
            CheckStroke(shapes[0], "#FF0000FF");
            CheckStrokeDashArray(shapes[0], "4 8 1 2");
        }

        [Test(Skip = "https://github.com/Microsoft/RTVS/issues/3667")]
        [Category.Plots]
        public async Task LineSolidLineType() {
            var code = @"grid.segments(.01, .1, .99, .1, gp=gpar(lty=1))";
            var doc = await GridTest(code);
            var shapes = doc.Descendants(XName.Get("Line", Ns)).ToList();
            shapes.Should().ContainSingle();
            CheckX1Y1X2Y2(shapes[0], X(0.01), Y(0.1), X(0.99), Y(0.1));
            CheckStrokeThickness(shapes[0], 1);
            CheckStroke(shapes[0], "#FF000000");
            CheckStrokeDashArray(shapes[0], null);
        }

        [Test]
        [Category.Plots]
        public async Task LineDashedLineType() {
            var code = @"grid.segments(.01, .1, .99, .1, gp=gpar(lty=2))";
            var doc = await GridTest(code);
            var shapes = doc.Descendants(XName.Get("Line", Ns)).ToList();
            shapes.Should().ContainSingle();
            CheckX1Y1X2Y2(shapes[0], X(0.01), Y(0.1), X(0.99), Y(0.1));
            CheckStrokeThickness(shapes[0], 1);
            CheckStroke(shapes[0], "#FF000000");
            CheckStrokeDashArray(shapes[0], "4 4");
        }

        [Test]
        [Category.Plots]
        public async Task Polygon() {
            var code = @"grid.polygon(x=c(0,0.5,1,0.5),y=c(0.5,1,0.5,0))";
            var doc = await GridTest(code);
            var shapes = doc.Descendants(XName.Get("Polygon", Ns)).ToList();
            shapes.Should().ContainSingle();
            CheckPoints(shapes[0], X(0), Y(0.5), X(0.5), Y(1.0), X(1.0), Y(0.5), X(0.5), Y(0));
            CheckStrokeThickness(shapes[0], 1);
            CheckStroke(shapes[0], "#FF000000");
            CheckStrokeDashArray(shapes[0], null);
        }

        [Test]
        [Category.Plots]
        public async Task Circle() {
            var code = @"grid.circle(0.5, 0.5, 0.2)";
            var doc = await GridTest(code);
            var shapes = doc.Descendants(XName.Get("Ellipse", Ns)).ToList();
            shapes.Should().ContainSingle();
            CheckWidthHeight(shapes[0], W(0.4), H(0.4));
            CheckCanvasLeftTop(shapes[0], X(0.5) - H(0.2), Y(0.5) - W(0.2));
        }

        [Test]
        [Category.Plots]
        public async Task Rectangle() {
            var code = @"grid.rect(0.5, 0.5, 0.3, 0.4)";
            var doc = await GridTest(code);
            var shapes = doc.Descendants(XName.Get("Rectangle", Ns)).ToList();
            shapes.Should().ContainSingle();
            CheckWidthHeight(shapes[0], W(0.3), H(0.4));
            CheckCanvasLeftTop(shapes[0], X(0.5) - H(0.15), Y(0.5) - W(0.2));
        }

        [Test]
        [Category.Plots]
        public async Task Path() {
            var code = @"grid.path(c(.1, .1, .9, .9, .2, .2, .8, .8), c(.1, .9, .9, .1, .2, .8, .8, .2), id=rep(1:2,each=4), rule='winding', gp=gpar(filled.contour='grey'))";
            var doc = await GridTest(code);
            var shapes = doc.Descendants(XName.Get("Path", Ns)).ToList();
            shapes.Should().ContainSingle();

            string expected = string.Format("F 1 M {0},{1} L {2},{3} L {4},{5} L {6},{7} Z M {8},{9} L {10},{11} L {12},{13} L {14},{15} Z ",
                X(.1), Y(.1),
                X(.1), Y(.9),
                X(.9), Y(.9),
                X(.9), Y(.1),
                X(.2), Y(.2),
                X(.2), Y(.8),
                X(.8), Y(.8),
                X(.8), Y(.2));
            CheckStringAttr(shapes[0], "Data", expected);
        }

        [Test(Skip = "https://github.com/Microsoft/RTVS/issues/3667")]
        [Category.Plots]
        public async Task TextXmlEscape() {
            var code = "grid.text('hello<>&\"', 0.1, 0.3)";
            var doc = await GridTest(code);
            var shapes = doc.Descendants(XName.Get("TextBlock", Ns)).ToList();
            shapes.Should().ContainSingle();

            CheckStringAttr(shapes[0], "Text", "hello<>&\"");
        }

        private async Task<XDocument> GridTest(string code) {
            string outputFilePath = System.IO.Path.GetTempFileName();
            return await RunGraphicsTest(string.Format(GridPrefixCode, outputFilePath.Replace("\\", "/"), DefaultWidth, DefaultHeight) + "\n" + code + "\n" + GridSuffixCode + "\n", outputFilePath);
        }

        private void CheckX1Y1X2Y2(XElement element, double x1, double y1, double x2, double y2) {
            CheckDoubleAttr(element, "X1", x1);
            CheckDoubleAttr(element, "Y1", y1);
            CheckDoubleAttr(element, "X2", x2);
            CheckDoubleAttr(element, "Y2", y2);
        }

        private void CheckWidthHeight(XElement element, double width, double height) {
            CheckDoubleAttr(element, "Width", width);
            CheckDoubleAttr(element, "Height", height);
        }

        private void CheckPoints(XElement element, params double[] xyPoints) {
            (xyPoints.Length % 2).Should().Be(0);
            var sb = new StringBuilder();
            int i = 0;
            while (i < xyPoints.Length) {
                sb.AppendFormat("{0},{1} ", xyPoints[i], xyPoints[i + 1]);
                i += 2;
            }
            CheckStringAttr(element, "Points", sb.ToString().Trim());
        }

        private void CheckCanvasLeftTop(XElement element, double left, double top) {
            CheckDoubleAttr(element, "Canvas.Left", left);
            CheckDoubleAttr(element, "Canvas.Top", top);
        }

        private void CheckStrokeThickness(XElement element, double expected)
            => CheckDoubleAttr(element, "StrokeThickness", expected);

        private void CheckStroke(XElement element, string expected)
            => CheckStringAttr(element, "Stroke", expected);

        private void CheckStrokeDashArray(XElement element, string expected)
            => CheckStringAttr(element, "StrokeDashArray", expected);

        private void CheckStringAttr(XElement element, string attributeName, string expected) {
            var attrs = element.Attributes(attributeName);

            if (expected != null) {
                attrs.Should().ContainSingle().Which.Should().HaveValue(expected);
            } else {
                attrs.Should().BeEmpty();
            }
        }

        private void CheckDoubleAttr(XElement element, string attributeName, double? expected) {
            var attrs = element.Attributes(attributeName);

            if (expected != null) {
                attrs.Should().ContainSingle().Which.Should().HaveValue(expected.ToString());
            } else {
                attrs.Should().BeEmpty();
            }
        }

        private async Task<XDocument> RunGraphicsTest(string code, string outputFilePath) {
            using (var sessionProvider = new RSessionProvider(_services)) {
                await sessionProvider.TrySwitchBrokerAsync(nameof(XamlGraphicsDeviceTest));
                var session = sessionProvider.GetOrCreate(_testMethod.Name);
                await session.StartHostAsync(new RHostStartupInfo(), new RHostClientTestApp(), 50000);

                using (var interaction = await session.BeginInteractionAsync()) {
                    await interaction.RespondAsync(code);
                }

                await session.StopHostAsync();
            }

            File.Exists(outputFilePath).Should().BeTrue();
            var doc = XDocument.Load(outputFilePath);
            var docXml = doc.ToString();
            Console.WriteLine(docXml);
            return doc;
        }
    }
}
