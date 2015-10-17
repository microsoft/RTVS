using Microsoft.R.Support.Settings;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.R.Support.Utility;

namespace Microsoft.R.Host.Client.Test {
    [TestClass]
    public class GraphicsDeviceTest {
        private const string ns = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
        private const string gridPrefixCode = @"
vsgd <- function() { .External('C_vsgd', 5, 5)}
options(device = 'vsgd')
library(grid)
grid.newpage()
";

        private const int DPI = 72;
        private const int DefaultWidthInInches = 5;
        private const int DefaultHeightInInches = 5;
        private const int DefaultWidth = DefaultWidthInInches * DPI;
        private const int DefaultHeight = DefaultHeightInInches * DPI;

        private double X(double percentX) {
            return DefaultWidth * percentX;
        }

        private double Y(double percentY) {
            return DefaultHeight - DefaultHeight * percentY;
        }

        private double W(double percentX) {
            return DefaultWidth * percentX;
        }

        private double H(double percentY) {
            return DefaultHeight * percentY;
        }

        [TestMethod]
        public void Line() {
            var code = @"grid.segments(.01, .1, .99, .1)";
            var doc = GridTest(code);
            var shapes = doc.Descendants(XName.Get("Line", ns)).ToList();
            Assert.AreEqual(1, shapes.Count);
            CheckX1Y1X2Y2(shapes[0], X(0.01), Y(0.1), X(0.99), Y(0.1));
            CheckStrokeThickness(shapes[0], 1);
            CheckStroke(shapes[0], "#FF000000");
            CheckStrokeDashArray(shapes[0], null);
        }

        [TestMethod]
        public void LineCustomLineType() {
            var code = @"grid.segments(.01, .1, .99, .1, gp=gpar(lty='4812',lwd=5,col='Blue'))";
            var doc = GridTest(code);
            var shapes = doc.Descendants(XName.Get("Line", ns)).ToList();
            Assert.AreEqual(1, shapes.Count);
            CheckX1Y1X2Y2(shapes[0], X(0.01), Y(0.1), X(0.99), Y(0.1));
            CheckStrokeThickness(shapes[0], 5);
            CheckStroke(shapes[0], "#FF0000FF");
            CheckStrokeDashArray(shapes[0], "4 8 1 2");
        }

        [TestMethod]
        public void LineSolidLineType() {
            var code = @"grid.segments(.01, .1, .99, .1, gp=gpar(lty=1))";
            var doc = GridTest(code);
            var shapes = doc.Descendants(XName.Get("Line", ns)).ToList();
            Assert.AreEqual(1, shapes.Count);
            CheckX1Y1X2Y2(shapes[0], X(0.01), Y(0.1), X(0.99), Y(0.1));
            CheckStrokeThickness(shapes[0], 1);
            CheckStroke(shapes[0], "#FF000000");
            CheckStrokeDashArray(shapes[0], null);
        }

        [TestMethod]
        public void LineDashedLineType() {
            var code = @"grid.segments(.01, .1, .99, .1, gp=gpar(lty=2))";
            var doc = GridTest(code);
            var shapes = doc.Descendants(XName.Get("Line", ns)).ToList();
            Assert.AreEqual(1, shapes.Count);
            CheckX1Y1X2Y2(shapes[0], X(0.01), Y(0.1), X(0.99), Y(0.1));
            CheckStrokeThickness(shapes[0], 1);
            CheckStroke(shapes[0], "#FF000000");
            CheckStrokeDashArray(shapes[0], "4 4");
        }

        [TestMethod]
        public void Polygon() {
            var code = @"grid.polygon(x=c(0,0.5,1,0.5),y=c(0.5,1,0.5,0))";
            var doc = GridTest(code);
            var shapes = doc.Descendants(XName.Get("Polygon", ns)).ToList();
            Assert.AreEqual(1, shapes.Count);
            CheckPoints(shapes[0], X(0), Y(0.5), X(0.5), Y(1.0), X(1.0), Y(0.5), X(0.5), Y(0));
            CheckStrokeThickness(shapes[0], 1);
            CheckStroke(shapes[0], "#FF000000");
            CheckStrokeDashArray(shapes[0], null);
        }

        [TestMethod]
        public void Circle() {
            var code = @"grid.circle(0.5, 0.5, 0.2)";
            var doc = GridTest(code);
            var shapes = doc.Descendants(XName.Get("Ellipse", ns)).ToList();
            Assert.AreEqual(1, shapes.Count);
            CheckWidthHeight(shapes[0], W(0.4), H(0.4));
            CheckCanvasLeftTop(shapes[0], X(0.5) - H(0.2), Y(0.5) - W(0.2));
        }

        [TestMethod]
        public void Rectangle() {
            var code = @"grid.rect(0.5, 0.5, 0.3, 0.4)";
            var doc = GridTest(code);
            var shapes = doc.Descendants(XName.Get("Rectangle", ns)).ToList();
            Assert.AreEqual(1, shapes.Count);
            CheckWidthHeight(shapes[0], W(0.3), H(0.4));
            CheckCanvasLeftTop(shapes[0], X(0.5) - H(0.15), Y(0.5) - W(0.2));
        }

        [TestMethod]
        public void Path() {
            var code = @"grid.path(c(.1, .1, .9, .9, .2, .2, .8, .8), c(.1, .9, .9, .1, .2, .8, .8, .2), id=rep(1:2,each=4), rule='winding', gp=gpar(filled.contour='grey'))";
            var doc = GridTest(code);
            var shapes = doc.Descendants(XName.Get("Path", ns)).ToList();
            Assert.AreEqual(1, shapes.Count);
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

        [TestMethod]
        public void TextXmlEscape() {
            var code = "grid.text('hello<>&\"', 0.1, 0.3)";
            var doc = GridTest(code);
            var shapes = doc.Descendants(XName.Get("TextBlock", ns)).ToList();
            Assert.AreEqual(1, shapes.Count);
            CheckStringAttr(shapes[0], "Text", "hello<>&\"");
        }

        private XDocument GridTest(string code) {
            return RunGraphicsTest(gridPrefixCode + "\n" + code + "\n");
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
            Assert.AreEqual(0, xyPoints.Length % 2);
            var sb = new StringBuilder();
            int i = 0;
            while (i < xyPoints.Length) {
                sb.AppendFormat("{0},{1} ", xyPoints[i], xyPoints[i+1]);
                i += 2;
            }
            CheckStringAttr(element, "Points", sb.ToString().Trim());
        }
        private void CheckCanvasLeftTop(XElement element, double left, double top) {
            CheckDoubleAttr(element, "Canvas.Left", left);
            CheckDoubleAttr(element, "Canvas.Top", top);
        }

        private void CheckStrokeThickness(XElement element, double expected) {
            CheckDoubleAttr(element, "StrokeThickness", expected);
        }

        private void CheckStroke(XElement element, string expected) {
            CheckStringAttr(element, "Stroke", expected);
        }

        private void CheckStrokeDashArray(XElement element, string expected) {
            CheckStringAttr(element, "StrokeDashArray", expected);
        }

        private void CheckStringAttr(XElement element, string attributeName, string expected) {
            var attrs = element.Attributes(attributeName);
            Assert.AreEqual(expected != null ? 1 : 0, attrs.Count());
            if (expected != null) {
                Assert.AreEqual(expected, attrs.First().Value);
            }
        }

        private void CheckDoubleAttr(XElement element, string attributeName, double? expected) {
            var attrs = element.Attributes(attributeName);
            Assert.AreEqual(expected != null ? 1 : 0, attrs.Count());
            if (expected != null) {
                Assert.AreEqual(expected.Value, double.Parse(attrs.First().Value));
            }
        }

        private XDocument RunGraphicsTest(string code) {
            var callbacks = new Callbacks(code);
            var host = new RHost(callbacks);
            var rhome = RInstallation.GetLatestEnginePathFromRegistry();
            var psi = new ProcessStartInfo();
            psi.CreateNoWindow = true;
            host.CreateAndRun(rhome, psi).GetAwaiter().GetResult();
            // Right now we may receive more than one xaml file, so just use the last one
            // Later, we'll want to check that we got the expected count
            Assert.IsTrue(callbacks.XamlFilePaths.Count > 0);
            var filePath = callbacks.XamlFilePaths[callbacks.XamlFilePaths.Count - 1];
            Assert.IsNotNull(filePath);
            Assert.IsTrue(File.Exists(filePath));
            var doc = XDocument.Load(filePath);
            var docXml = doc.ToString();
            Console.WriteLine(docXml);
            return doc;
        }

        class Callbacks : IRCallbacks {
            private string _inputCode;
            public List<string> XamlFilePaths {
                get; private set; }

            public Callbacks(string code) {
                _inputCode = code;
                XamlFilePaths = new List<string>();
            }

            public void Dispose() {
            }

            public Task Busy(IReadOnlyList<IRContext> contexts, bool which, CancellationToken ct) {
                return Task.FromResult(true);
            }

            public Task Connected(string rVersion) {
                return Task.CompletedTask;
            }

            public Task Disconnected() {
                return Task.CompletedTask;
            }

            public Task<string> ReadConsole(IReadOnlyList<IRContext> contexts, string prompt, string buf, int len, bool addToHistory, bool isEvaluationAllowed, CancellationToken ct) {
                // We're getting called a few times here
                // First time, send over the code to execute
                // After that, send nothing
                var code = _inputCode;
                _inputCode = "";
                return Task.FromResult(code);
            }

            public async Task ShowMessage(IReadOnlyList<IRContext> contexts, string s, CancellationToken ct) {
                await Console.Error.WriteLineAsync(s);
            }

            public async Task WriteConsoleEx(IReadOnlyList<IRContext> contexts, string buf, OutputType otype, CancellationToken ct) {
                var writer = otype == OutputType.Output ? Console.Out : Console.Error;
                await writer.WriteAsync(buf);
            }

            public Task<YesNoCancel> YesNoCancel(IReadOnlyList<IRContext> contexts, string s, bool isEvaluationAllowed, CancellationToken ct) {
                return Task.FromResult<YesNoCancel>(Microsoft.R.Host.Client.YesNoCancel.Yes);
            }

            public Task PlotXaml(IReadOnlyList<IRContext> contexts, string xamlFilePath, CancellationToken ct) {
                XamlFilePaths.Add(xamlFilePath);
                return Task.CompletedTask;
            }
        }
    }
}
