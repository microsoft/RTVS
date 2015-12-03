using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Actions.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Languages.Core.Test.Utility;
using System.Drawing;

namespace Microsoft.R.Host.Client.Test {
    [TestClass]
    public class VisualStudioGraphicsDeviceTest {
        private const string ns = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";

        // Copied from RSessionEvaluationCommands.cs
        // TODO: need to merge into a single location
        private const string setupCode = @"
.rtvs.vsgdresize <- function(width, height) {
   .External('rtvs::External.visualstudio_graphicsdevice_resize', width, height)
}
.rtvs.vsgd <- function() {
   .External('rtvs::External.visualstudio_graphicsdevice_new')
}
.rtvs.vsgdexportimage <- function(filename, device) {
    dev.copy(device=device,filename=filename)
    dev.off()
}
.rtvs.vsgdexportpdf <- function(filename) {
    dev.copy(device=pdf,file=filename)
    dev.off()
}
.rtvs.vsgdnextplot <- function() {
   .External('rtvs::External.visualstudio_graphicsdevice_next_plot')
}
.rtvs.vsgdpreviousplot <- function() {
   .External('rtvs::External.visualstudio_graphicsdevice_previous_plot')
}
options(device = '.rtvs.vsgd')
";

        private const int DefaultWidth = 360;
        private const int DefaultHeight = 360;

        private const int DefaultExportWidth = 480;
        private const int DefaultExportHeight = 480;

        public TestContext TestContext { get; set; }

        private int X(double percentX) {
            return (int)(DefaultWidth * percentX);
        }

        private int Y(double percentY) {
            return (int)(DefaultHeight - DefaultHeight * percentY - 1);
        }

        private int W(double percentX) {
            return (int)(DefaultWidth * percentX);
        }

        private int H(double percentY) {
            return (int)(DefaultHeight * percentY);
        }

        [TestMethod]
        public void GridLine() {
            var code = @"
library(grid)
grid.newpage()
grid.segments(.01, .1, .99, .1)
";
            var actualPlotFilePaths = GraphicsTest(code).ToArray();
            Assert.AreEqual(1, actualPlotFilePaths.Length);
            var bmp = (Bitmap)Bitmap.FromFile(actualPlotFilePaths[0]);
            Assert.AreEqual(DefaultWidth, bmp.Width);
            Assert.AreEqual(DefaultHeight, bmp.Height);
            var startX = X(0.01);
            var endX = X(0.99);
            var y = Y(0.1);

            var fg = Color.FromArgb(255, 0, 0, 0);
            var bg = Color.FromArgb(255, 255, 255, 255);

            // Check extremities on the line
            Assert.AreEqual(fg, bmp.GetPixel(startX, y));
            Assert.AreEqual(fg, bmp.GetPixel(endX, y));

            // Check extremities outside of line
            Assert.AreEqual(bg, bmp.GetPixel(startX - 1, y));
            Assert.AreEqual(bg, bmp.GetPixel(endX + 1, y));

            // Check above and below line
            Assert.AreEqual(bg, bmp.GetPixel(startX, y - 1));
            Assert.AreEqual(bg, bmp.GetPixel(startX, y + 1));
        }

        [TestMethod]
        public void MultiplePagesWithSleep() {
            var code = @"
library(grid)
redGradient <- matrix(hcl(0, 80, seq(50, 80, 10)), nrow = 4, ncol = 5)

# interpolated
grid.newpage()
grid.raster(redGradient)
Sys.sleep(1)

# blocky
grid.newpage()
grid.raster(redGradient, interpolate = FALSE)
";
            var actualPlotFilePaths = GraphicsTest(code).ToArray();
            Assert.AreEqual(2, actualPlotFilePaths.Length);
        }

        [TestMethod]
        public void MultiplePlotsWithSleep() {
            var code = @"
plot(0:10)
Sys.sleep(1)
plot(5:15)
";
            var actualPlotFilePaths = GraphicsTest(code).ToArray();
            Assert.AreEqual(2, actualPlotFilePaths.Length);
        }

        [TestMethod]
        public void MultiplePlotsNoSleep() {
            var code = @"
plot(0:10)
plot(5:15)
";
            var actualPlotFilePaths = GraphicsTest(code).ToArray();
            Assert.AreEqual(1, actualPlotFilePaths.Length);
        }

        [TestMethod]
        public void PlotCars() {
            GraphicsTestAgainstExpectedFiles(@"plot(cars)");
        }

        [TestMethod]
        public void SetInitialSize() {
            var code = @"
.rtvs.vsgdresize(600, 600)
plot(0:10)
";
            var actualPlotFilePaths = GraphicsTest(code).ToArray();
            Assert.AreEqual(1, actualPlotFilePaths.Length);
            var bmp = (Bitmap)Bitmap.FromFile(actualPlotFilePaths[0]);
            Assert.AreEqual(600, bmp.Width);
            Assert.AreEqual(600, bmp.Height);
        }

        [TestMethod]
        public void ResizeImmediately() {
            var code = @"
plot(0:10)
.rtvs.vsgdresize(600, 600)
";
            var actualPlotFilePaths = GraphicsTest(code).ToArray();
            Assert.AreEqual(1, actualPlotFilePaths.Length);
            var bmp = (Bitmap)Bitmap.FromFile(actualPlotFilePaths[0]);
            Assert.AreEqual(600, bmp.Width);
            Assert.AreEqual(600, bmp.Height);
        }

        [TestMethod]
        public void ResizeAfterDelay() {
            var code = @"
plot(0:10)
Sys.sleep(1)
.rtvs.vsgdresize(600, 600)
";
            var actualPlotFilePaths = GraphicsTest(code).ToArray();
            Assert.AreEqual(2, actualPlotFilePaths.Length);
            var bmp1 = (Bitmap)Bitmap.FromFile(actualPlotFilePaths[0]);
            var bmp2 = (Bitmap)Bitmap.FromFile(actualPlotFilePaths[1]);
            Assert.AreEqual(DefaultWidth, bmp1.Width);
            Assert.AreEqual(DefaultHeight, bmp1.Height);
            Assert.AreEqual(600, bmp2.Width);
            Assert.AreEqual(600, bmp2.Height);
        }

        [TestMethod]
        public void ExportToImage() {
            var exportedBmpFilePath = Path.Combine(TestContext.TestRunDirectory, "ExportToBmpResult.bmp");
            var exportedPngFilePath = Path.Combine(TestContext.TestRunDirectory, "ExportToPngResult.png");
            var exportedJpegFilePath = Path.Combine(TestContext.TestRunDirectory, "ExportToJpegResult.jpg");
            var exportedTiffFilePath = Path.Combine(TestContext.TestRunDirectory, "ExportToTiffResult.tif");

            var code = string.Format(@"
plot(0:10)
Sys.sleep(1)
.rtvs.vsgdexportimage('{0}', bmp)
.rtvs.vsgdexportimage('{1}', png)
.rtvs.vsgdexportimage('{2}', jpeg)
.rtvs.vsgdexportimage('{3}', tiff)
",
                exportedBmpFilePath.Replace("\\", "/"),
                exportedPngFilePath.Replace("\\", "/"),
                exportedJpegFilePath.Replace("\\", "/"),
                exportedTiffFilePath.Replace("\\", "/"));

            var actualPlotFilePaths = GraphicsTest(code).ToArray();
            Assert.AreEqual(1, actualPlotFilePaths.Length);

            var bmp = (Bitmap)Bitmap.FromFile(actualPlotFilePaths[0]);
            Assert.AreEqual(DefaultWidth, bmp.Width);
            Assert.AreEqual(DefaultHeight, bmp.Height);

            var exportedBmp = (Bitmap)Bitmap.FromFile(exportedBmpFilePath);
            Assert.AreEqual(DefaultExportWidth, exportedBmp.Width);
            Assert.AreEqual(DefaultExportHeight, exportedBmp.Height);

            var exportedPng = (Bitmap)Bitmap.FromFile(exportedPngFilePath);
            Assert.AreEqual(DefaultExportWidth, exportedPng.Width);
            Assert.AreEqual(DefaultExportHeight, exportedPng.Height);

            var exportedJpeg = (Bitmap)Bitmap.FromFile(exportedJpegFilePath);
            Assert.AreEqual(DefaultExportWidth, exportedJpeg.Width);
            Assert.AreEqual(DefaultExportHeight, exportedJpeg.Height);

            var exportedTiff = (Bitmap)Bitmap.FromFile(exportedTiffFilePath);
            Assert.AreEqual(DefaultExportWidth, exportedTiff.Width);
            Assert.AreEqual(DefaultExportHeight, exportedTiff.Height);
        }

        [TestMethod]
        public void ExportToPdf() {
            var exportedFilePath = Path.Combine(TestContext.TestRunDirectory, "ExportToPdfResult.pdf");

            var code = string.Format(@"
plot(0:10)
Sys.sleep(1)
.rtvs.vsgdexportpdf('{0}')
", exportedFilePath.Replace("\\", "/"));

            var actualPlotFilePaths = GraphicsTest(code).ToArray();
            Assert.AreEqual(1, actualPlotFilePaths.Length);

            var bmp = (Bitmap)Bitmap.FromFile(actualPlotFilePaths[0]);
            Assert.AreEqual(DefaultWidth, bmp.Width);
            Assert.AreEqual(DefaultHeight, bmp.Height);

            Assert.IsTrue(File.Exists(exportedFilePath));
        }

        [TestMethod]
        public void History() {
            var code = @"
plot(0:10)
Sys.sleep(1)
plot(5:15)
Sys.sleep(1)
.rtvs.vsgdpreviousplot()
";
            // TODO: when this passes, we can make it validate against expected files
            var actualPlotFilePaths = GraphicsTest(code).ToArray();
            Assert.AreEqual(3, actualPlotFilePaths.Length);
            CollectionAssert.AreEqual(File.ReadAllBytes(actualPlotFilePaths[0]), File.ReadAllBytes(actualPlotFilePaths[2]));
            CollectionAssert.AreNotEqual(File.ReadAllBytes(actualPlotFilePaths[0]), File.ReadAllBytes(actualPlotFilePaths[1]));

        }

        private void GraphicsTestAgainstExpectedFiles(string code) {
            var actualPlotPaths = GraphicsTest(code).ToArray();
            var expectedPlotPaths = GetTestExpectedFiles();
            Assert.AreEqual(expectedPlotPaths.Length, actualPlotPaths.Length);
            for (int i = 0; i < expectedPlotPaths.Length; i++) {
                var actualPlotPath = actualPlotPaths.SingleOrDefault(p => Path.GetFileName(p) == Path.GetFileName(expectedPlotPaths[i]));
                Assert.IsNotNull(actualPlotPath);
                CollectionAssert.AreEqual(File.ReadAllBytes(expectedPlotPaths[i]), File.ReadAllBytes(actualPlotPath));
            }
        }

        private string[] GetTestExpectedFiles() {
            var folderPath = TestFiles.GetTestFilesFolder(TestContext);
            var expectedFilesFilter = string.Format("{0}-{1}-*.png", TestContext.FullyQualifiedTestClassName, TestContext.TestName);
            var expectedFiles = Directory.GetFiles(folderPath, expectedFilesFilter);
            return expectedFiles;
        }

        private IEnumerable<string> GraphicsTest(string code) {
            var callbacks = new Callbacks(setupCode + "\n" + code + "\n");
            var host = new RHost(callbacks);
            var rhome = RInstallation.GetLatestEnginePathFromRegistry();
            var psi = new ProcessStartInfo();
            psi.CreateNoWindow = true;
            host.CreateAndRun(rhome, IntPtr.Zero, psi).GetAwaiter().GetResult();

            var images = new List<Image>();
            for (int i = 0; i < callbacks.PlotFilePaths.Count; i++) {
                // Copy and rename the plot file(s) to test run folder
                var tempFilePath = callbacks.PlotFilePaths[i];
                var newFileName = string.Format("{0}-{1}-{2}{3}", TestContext.FullyQualifiedTestClassName, TestContext.TestName, i, Path.GetExtension(tempFilePath));
                var testOutputFilePath = Path.Combine(TestContext.TestRunDirectory, newFileName);
                File.Copy(tempFilePath, testOutputFilePath);
                yield return testOutputFilePath;
            }

            // Delete the original files in the temp folder
            // If history feature was used, it's possible that the same
            // file was sent multiple times
            foreach (var tempFilePath in new HashSet<string>(callbacks.PlotFilePaths)) {
                File.Delete(tempFilePath);
            }
        }

        private void AssertImageEqual(Image actual, Image expected) {
            Assert.AreEqual(expected.Width, actual.Width);
            Assert.AreEqual(expected.Height, actual.Height);
            var actualBmp = (Bitmap)actual;
            var expectedBmp = (Bitmap)expected;
            for (int y = 0; y < actualBmp.Height; y++) {
                for (int x = 0; x < actualBmp.Width; x++) {
                    Assert.AreEqual(expectedBmp.GetPixel(x, y), actualBmp.GetPixel(x, y), string.Format("Pixel at coordinate (x={0},y={1})", x, y));
                }
            }
        }

        class Callbacks : IRCallbacks {
            private string _inputCode;
            public List<string> PlotFilePaths {
                get; private set;
            }

            public Callbacks(string code) {
                _inputCode = code;
                PlotFilePaths = new List<string>();
            }

            public void Dispose() {
            }

            public Task Busy(bool which, CancellationToken ct) {
                return Task.FromResult(true);
            }

            public Task Connected(string rVersion) {
                return Task.CompletedTask;
            }

            public Task Disconnected() {
                return Task.CompletedTask;
            }

            public Task<string> ReadConsole(IReadOnlyList<IRContext> contexts, string prompt, int len, bool addToHistory, bool isEvaluationAllowed, CancellationToken ct) {
                // We're getting called a few times here
                // First time, send over the code to execute
                // After that, send nothing
                var code = _inputCode;
                _inputCode = "";
                return Task.FromResult(code);
            }

            public async Task ShowMessage(string s, CancellationToken ct) {
                await Console.Error.WriteLineAsync(s);
            }

            public async Task WriteConsoleEx(string buf, OutputType otype, CancellationToken ct) {
                var writer = otype == OutputType.Output ? Console.Out : Console.Error;
                await writer.WriteAsync(buf);
            }

            public Task<YesNoCancel> YesNoCancel(IReadOnlyList<IRContext> contexts, string s, bool isEvaluationAllowed, CancellationToken ct) {
                return Task.FromResult<YesNoCancel>(Microsoft.R.Host.Client.YesNoCancel.Yes);
            }

            public Task Plot(string filePath, CancellationToken ct) {
                PlotFilePaths.Add(filePath);
                return Task.CompletedTask;
            }

            public Task Browser(string url) {
                throw new NotImplementedException();
            }

            public Task<MessageButtons> ShowDialog(IReadOnlyList<IRContext> contexts, string s, bool isEvaluationAllowed, MessageButtons buttons, CancellationToken ct) {
                throw new NotImplementedException();
            }
        }
    }
}
