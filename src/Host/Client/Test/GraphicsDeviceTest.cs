using Microsoft.R.Support.Settings;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

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

        [TestMethod]
        public void Line1() {
            var code = @"grid.segments(.01, .5, .99, .5, gp = gpar(lty = '4812', lwd = 5))";
            var doc = GridTest(code);
            var lines = doc.Descendants(XName.Get("Line", ns)).ToList();
            Assert.AreEqual(1, lines.Count);
            var thickness = lines[0].Attributes("StrokeThickness").First().Value;
            Assert.AreEqual("5", thickness);
        }

        private XDocument GridTest(string code) {
            return RunGraphicsTest(gridPrefixCode + "\n" + code + "\n");
        }

        private XDocument RunGraphicsTest(string code) {
            var callbacks = new Callbacks(code);
            var host = new RHost(callbacks);
            var rhome = RToolsSettings.GetEnginePathFromRegistry();
            host.CreateAndRun(rhome).GetAwaiter().GetResult();
            // Right now we may receive more than one xaml file, so just use the last one
            // Later, we'll want to check that we got the expected count
            var filePath = callbacks.XamlFilePaths[callbacks.XamlFilePaths.Count - 1];
            Assert.IsNotNull(filePath);
            Assert.IsTrue(File.Exists(filePath));
            var doc = XDocument.Load(filePath);
            var docXml = doc.ToString();
            Console.WriteLine(docXml);
            return doc;
        }

        class Callbacks : IRCallbacks {
            private IRExpressionEvaluator _evaluator;
            private string _inputCode;
            public List<string> XamlFilePaths {
                get; private set; }

            public Callbacks(string code) {
                _inputCode = code;
                XamlFilePaths = new List<string>();
            }

            public void Dispose() {
            }

            public Task Busy(IReadOnlyCollection<IRContext> contexts, bool which, CancellationToken ct) {
                return Task.FromResult(true);
            }

            public Task Evaluate(IReadOnlyCollection<IRContext> contexts, IRExpressionEvaluator evaluator, CancellationToken ct) {
                _evaluator = evaluator;
                return Task.CompletedTask;
            }

            public Task Connected(string rVersion) {
                return Task.CompletedTask;
            }

            public Task Disconnected() {
                return Task.CompletedTask;
            }

            public Task<string> ReadConsole(IReadOnlyCollection<IRContext> contexts, string prompt, string buf, int len, bool addToHistory, CancellationToken ct) {
                // We're getting called a few times here
                // First time, send over the code to execute
                // After that, send nothing
                var code = _inputCode;
                _inputCode = "";
                return Task.FromResult(code);
            }

            public async Task ShowMessage(IReadOnlyCollection<IRContext> contexts, string s, CancellationToken ct) {
                await Console.Error.WriteLineAsync(s);
            }

            public async Task WriteConsoleEx(IReadOnlyCollection<IRContext> contexts, string buf, OutputType otype, CancellationToken ct) {
                var writer = otype == OutputType.Output ? Console.Out : Console.Error;
                await writer.WriteAsync(buf);
            }

            public Task<YesNoCancel> YesNoCancel(IReadOnlyCollection<IRContext> contexts, string s, CancellationToken ct) {
                return Task.FromResult<YesNoCancel>(Microsoft.R.Host.Client.YesNoCancel.Yes);
            }

            public Task PlotXaml(IReadOnlyCollection<IRContext> contexts, string xamlFilePath, CancellationToken ct) {
                XamlFilePaths.Add(xamlFilePath);
                return Task.CompletedTask;
            }
        }
    }
}
