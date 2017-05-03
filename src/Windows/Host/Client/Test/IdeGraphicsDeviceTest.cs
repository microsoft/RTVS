// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Common.Core;
using Microsoft.Common.Core.IO;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Test.Fixtures;
using Microsoft.R.Host.Client.Session;
using Microsoft.R.Host.Client.Test.Fixtures;
using Microsoft.R.Host.Client.Test.Script;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.UnitTests.Core.XUnit.MethodFixtures;

namespace Microsoft.R.Host.Client.Test {
    [ExcludeFromCodeCoverage]
    public class IdeGraphicsDeviceTest {
        private readonly IServiceContainer _services;
        private readonly GraphicsDeviceTestFilesFixture _files;
        private readonly TestMethodFixture _testMethod;

        private const int DefaultWidth = 360;
        private const int DefaultHeight = 360;

        private const int DefaultExportWidth = 480;
        private const int DefaultExportHeight = 480;
        private const int DefaultExportResolution = 96;

        public List<string> PlotFilePaths { get; } = new List<string>();
        public List<PlotMessage> OriginalPlotMessages { get; } = new List<PlotMessage>();

        private PlotDeviceProperties DefaultDeviceProperties = new PlotDeviceProperties(DefaultWidth, DefaultHeight, 96);

        public IdeGraphicsDeviceTest(IServiceContainer services, GraphicsDeviceTestFilesFixture files, TestMethodFixture testMethod) {
            _services = services;
            _files = files;
            _testMethod = testMethod;
        }

        private int X(double percentX) {
            return (int)(DefaultWidth * percentX);
        }

        private int Y(double percentY) {
            return (int)(DefaultHeight - DefaultHeight * percentY - 1);
        }

        [Test]
        [Category.Plots]
        public async Task GridLine() {
            var code = @"
library(grid)
grid.newpage()
grid.segments(.01, .1, .99, .1)
";
            var inputs = Batch(code);
            var actualPlotFilePaths = await GraphicsTestAsync(inputs);
            var plotFilePath = actualPlotFilePaths.Should().ContainSingle().Which;

            var bmp = (Bitmap)Image.FromFile(plotFilePath);
            bmp.Width.Should().Be(DefaultWidth);
            bmp.Height.Should().Be(DefaultHeight);
            var startX = X(0.01);
            var endX = X(0.99);
            var y = Y(0.1);

            var fg = Color.FromArgb(255, 0, 0, 0);
            var bg = Color.FromArgb(255, 255, 255, 255);

            // Check extremities on the line
            bmp.GetPixel(startX, y).Should().Be(fg);
            bmp.GetPixel(endX, y).Should().Be(fg);

            // Check extremities outside of line
            bmp.GetPixel(startX - 1, y).Should().Be(bg);
            bmp.GetPixel(endX + 1, y).Should().Be(bg);

            // Check above and below line
            bmp.GetPixel(startX, y - 1).Should().Be(bg);
            bmp.GetPixel(startX, y + 1).Should().Be(bg);
        }

        [Test]
        [Category.Plots]
        public async Task MultiplePagesTwoBatchesInteractive() {
            var inputs = new[] {
                @"
library(grid)
redGradient <- matrix(hcl(0, 80, seq(50, 80, 10)), nrow = 4, ncol = 5)

# interpolated
grid.newpage()
grid.raster(redGradient)
",
                @"
# blocky
grid.newpage()
grid.raster(redGradient, interpolate = FALSE)
"
            };

            var actualPlotFilePaths = await GraphicsTestAsync(inputs);
            actualPlotFilePaths.Should().HaveCount(2);
        }

        [Test]
        [Category.Plots]
        public async Task MultiplePlotsInteractive() {
            var code = @"
plot(0:10)
plot(5:15)
";
            var inputs = Interactive(code);
            var actualPlotFilePaths = await GraphicsTestAsync(inputs);
            actualPlotFilePaths.Should().HaveCount(2);
        }

        [Test]
        [Category.Plots]
        public async Task MultiplePlotsBatch() {
            var code = @"
plot(0:10)
plot(5:15)
";
            var inputs = Batch(code);
            var actualPlotFilePaths = await GraphicsTestAsync(inputs);
            actualPlotFilePaths.Should().HaveCount(2);
        }

        [Test]
        [Category.Plots]
        public async Task PlotCars() {
            var expectedPath = await WriteExpectedImageAsync("png", 360, 360, 96, "Expected", "plot(cars)");

            var code = @"
plot(cars)
";
            var inputs = Batch(code);
            var actualPlotPaths = (await GraphicsTestAsync(inputs)).ToArray();
            var expectedPlotPaths = new string[] { expectedPath };
            CompareImages(actualPlotPaths, expectedPlotPaths);
        }

        [Test]
        [Category.Plots]
        public async Task SetInitialSize() {
            DefaultDeviceProperties = new PlotDeviceProperties(600, 600, 96);

            var code = @"
plot(0:10)
";
            var inputs = Batch(code);
            var actualPlotFilePaths = await GraphicsTestAsync(inputs);
            var plotFilePath = actualPlotFilePaths.Should().ContainSingle().Which;

            var bmp = (Bitmap)Image.FromFile(plotFilePath);
            bmp.Width.Should().Be(600);
            bmp.Height.Should().Be(600);
        }

        [Test]
        [Category.Plots]
        public async Task ResizeNonInteractive() {
            var code = @"
plot(0:10)
rtvs:::graphics.ide.resize(rtvs:::graphics.ide.getactivedeviceid(), 600, 600, 96)
";
            var inputs = Batch(code);
            var actualPlotFilePaths = await GraphicsTestAsync(inputs);
            var plotFilePath = actualPlotFilePaths.Should().ContainSingle().Which;

            var bmp = (Bitmap)Image.FromFile(plotFilePath);
            bmp.Width.Should().Be(600);
            bmp.Height.Should().Be(600);
        }

        [Test]
        [Category.Plots]
        public async Task ResizeInteractive() {
            var code = @"
plot(0:10)
rtvs:::graphics.ide.resize(rtvs:::graphics.ide.getactivedeviceid(), 600, 600, 96)
";
            var inputs = Interactive(code);
            var actualPlotFilePaths = (await GraphicsTestAsync(inputs)).ToArray();
            actualPlotFilePaths.Should().HaveCount(2);

            var bmp1 = (Bitmap)Image.FromFile(actualPlotFilePaths[0]);
            var bmp2 = (Bitmap)Image.FromFile(actualPlotFilePaths[1]);
            bmp1.Width.Should().Be(DefaultWidth);
            bmp1.Height.Should().Be(DefaultHeight);
            bmp2.Width.Should().Be(600);
            bmp2.Height.Should().Be(600);
        }

        [Test]
        [Category.Plots]
        public async Task ResizeInteractiveNoTempFilesLeak() {
            //https://github.com/Microsoft/RTVS/issues/1568
            var code = @"
plot(0:10)
rtvs:::graphics.ide.resize(rtvs:::graphics.ide.getactivedeviceid(), 600, 600, 96)
";
            var tmpFilesBefore = Directory.GetFiles(Path.GetTempPath(), "rhost-ide-plot-*.png");
            var inputs = Interactive(code);
            var actualPlotFilePaths = (await GraphicsTestAsync(inputs)).ToArray();
            actualPlotFilePaths.Should().HaveCount(2);
            var tmpFilesAfter = Directory.GetFiles(Path.GetTempPath(), "rhost-ide-plot-*.png");
            tmpFilesAfter.ShouldAllBeEquivalentTo(tmpFilesBefore);
        }

        [Test]
        [Category.Plots]
        public async Task ExportToImage() {
            var exportedBmpFilePath = _files.ExportToBmpResultPath;
            var exportedPngFilePath = _files.ExportToPngResultPath;
            var exportedJpegFilePath = _files.ExportToJpegResultPath;
            var exportedTiffFilePath = _files.ExportToTiffResultPath;

            var code = string.Format(@"
plot(0:10)
"
            );

            string[] format = { "bmp", "png", "jpeg", "tiff" };
            string[] paths = { exportedBmpFilePath, exportedPngFilePath, exportedJpegFilePath, exportedTiffFilePath };
            var inputs = Interactive(code);
            var actualPlotFilePaths = await ExportToImageAsync(inputs, format, paths, DefaultExportWidth, DefaultExportHeight, DefaultExportResolution);
            var plotFilePath = actualPlotFilePaths.Should().ContainSingle().Which;

            var bmp = (Bitmap)Image.FromFile(plotFilePath);
            bmp.Width.Should().Be(DefaultWidth);
            bmp.Height.Should().Be(DefaultHeight);

            var exportedBmp = (Bitmap)Image.FromFile(exportedBmpFilePath);
            exportedBmp.Width.Should().Be(DefaultExportWidth);
            exportedBmp.Height.Should().Be(DefaultExportHeight);

            var exportedPng = (Bitmap)Image.FromFile(exportedPngFilePath);
            exportedPng.Width.Should().Be(DefaultExportWidth);
            exportedPng.Height.Should().Be(DefaultExportHeight);

            var exportedJpeg = (Bitmap)Image.FromFile(exportedJpegFilePath);
            exportedJpeg.Width.Should().Be(DefaultExportWidth);
            exportedJpeg.Height.Should().Be(DefaultExportHeight);

            var exportedTiff = (Bitmap)Image.FromFile(exportedTiffFilePath);
            exportedTiff.Width.Should().Be(DefaultExportWidth);
            exportedTiff.Height.Should().Be(DefaultExportHeight);
        }

        [Test]
        [Category.Plots]
        public async Task ExportPreviousPlotToImage() {
            var expectedExportedBmpFilePath = await WriteExpectedImageAsync("bmp", DefaultExportWidth, DefaultExportHeight, DefaultExportResolution, "Expected", "plot(0:10)");

            var actualExportedBmpFilePath = _files.GetDestinationPath("ExportPreviousPlotToImageExpected1.bmp");
            var code = string.Format(@"
plot(0:10)
plot(10:20)
rtvs:::graphics.ide.previousplot(rtvs:::graphics.ide.getactivedeviceid())
"
            );

            var inputs = Interactive(code);
            string[] format = { "bmp"};
            string[] paths = { actualExportedBmpFilePath };
            var actualPlotFilePaths = await ExportToImageAsync(inputs, format, paths, DefaultExportWidth, DefaultExportHeight, DefaultExportResolution);
            actualPlotFilePaths.Should().HaveCount(3);

            CompareImages(new string[] { actualExportedBmpFilePath }, new string[] { expectedExportedBmpFilePath });
        }

        [Test]
        [Category.Plots]
        public async Task ExportToPdf() {
            var exportedFilePath = _files.ExportToPdfResultPath;

            var code = string.Format(@"
plot(0:10)
"
            );

            var inputs = Interactive(code);
            var actualPlotFilePaths = await ExportToPdfAsync(inputs, exportedFilePath, 7, 7);
            var plotFilePath = actualPlotFilePaths.Should().ContainSingle().Which;

            var bmp = (Bitmap)Image.FromFile(plotFilePath);
            bmp.Width.Should().Be(DefaultWidth);
            bmp.Height.Should().Be(DefaultHeight);

            PdfComparer.ComparePdfFiles(exportedFilePath, _files.ExpectedExportToPdfPath);
        }

        [Test]
        [Category.Plots]
        public async Task ResizeInteractiveMultiPlots() {
            // Resize a graph with multiple plots, where the
            // code is executed one line at a time interactively
            // Make sure that all parts of the graph are present
            // We used to have a bug where the resized image only had
            // the top left plot, and the others were missing
            var expected1Path = await WriteExpectedImageAsync("png", 360, 360, 96, "Expected1", "par(mfrow=c(2,2));plot(0:1)");
            var expected2Path = await WriteExpectedImageAsync("png", 360, 360, 96, "Expected2", "par(mfrow=c(2,2));plot(0:1);plot(1:2)");
            var expected3Path = await WriteExpectedImageAsync("png", 360, 360, 96, "Expected3", "par(mfrow=c(2,2));plot(0:1);plot(1:2);plot(2:3)");
            var expected4Path = await WriteExpectedImageAsync("png", 360, 360, 96, "Expected4", "par(mfrow=c(2,2));plot(0:1);plot(1:2);plot(2:3);plot(3:4)");
            var expected5Path = await WriteExpectedImageAsync("png", 600, 600, 96, "Expected5", "par(mfrow=c(2,2));plot(0:1);plot(1:2);plot(2:3);plot(3:4)");

            var code = @"
par(mfrow = c(2, 2))
plot(0:1)
plot(1:2)
plot(2:3)
plot(3:4)
rtvs:::graphics.ide.resize(rtvs:::graphics.ide.getactivedeviceid(), 600, 600, 96)
";
            var inputs = Interactive(code);
            var actualPlotPaths = (await GraphicsTestAsync(inputs)).ToArray();
            var expectedPlotPaths = new string[] { expected1Path, expected2Path, expected3Path, expected4Path, expected5Path };
            CompareImages(actualPlotPaths, expectedPlotPaths);
        }

        [Test]
        [Category.Plots]
        public async Task ResizeNonInteractiveMultiPlots() {
            // Resize a graph with multiple plots, where the
            // code is executed all at once
            // Make sure that all parts of the graph are present
            // We used to have a bug where the resized image only had
            // the top left plot, and the others were missing
            var expected1Path = await WriteExpectedImageAsync("png", 360, 360, 96, "Expected1", "par(mfrow=c(2,2));plot(0:1);plot(1:2);plot(2:3);plot(3:4)");
            var expected2Path = await WriteExpectedImageAsync("png", 600, 600, 96, "Expected2", "par(mfrow=c(2,2));plot(0:1);plot(1:2);plot(2:3);plot(3:4)");

            var inputs = new [] {
                @"
par(mfrow = c(2, 2))
plot(0:1)
plot(1:2)
plot(2:3)
plot(3:4)
",
"rtvs:::graphics.ide.resize(rtvs:::graphics.ide.getactivedeviceid(), 600, 600, 96)",
            };
            var actualPlotPaths = (await GraphicsTestAsync(inputs)).ToArray();
            var expectedPlotPaths = new string[] { expected1Path, expected2Path };
            CompareImages(actualPlotPaths, expectedPlotPaths);
        }

        [Test]
        [Category.Plots]
        public async Task Previous() {
            var code = @"
plot(0:10)
plot(5:15)
rtvs:::graphics.ide.previousplot(rtvs:::graphics.ide.getactivedeviceid())
";

            var inputs = Interactive(code);
            var actualPlotFilePaths = (await GraphicsTestAsync(inputs)).ToArray();
            actualPlotFilePaths.Should().HaveCount(3);

            File.ReadAllBytes(actualPlotFilePaths[2]).Should().Equal(File.ReadAllBytes(actualPlotFilePaths[0]));
            File.ReadAllBytes(actualPlotFilePaths[1]).Should().NotEqual(File.ReadAllBytes(actualPlotFilePaths[0]));

            OriginalPlotMessages.Last().ActivePlotIndex.Should().Be(0);
            OriginalPlotMessages.Last().PlotCount.Should().Be(2);
        }

        [Test]
        [Category.Plots]
        public async Task ClearPlots() {
            var code = @"
plot(0:10)
plot(0:15)
rtvs:::graphics.ide.clearplots(rtvs:::graphics.ide.getactivedeviceid())
";

            var inputs = Interactive(code);
            var actualPlotFilePaths = await GraphicsTestAsync(inputs);
            actualPlotFilePaths.Should().HaveCount(2);

            OriginalPlotMessages.Last().ActivePlotIndex.Should().Be(-1);
            OriginalPlotMessages.Last().PlotCount.Should().Be(0);
        }

        [Test]
        [Category.Plots]
        public async Task RemovePlotFirst() {
            var code = @"
plot(0:10)
plot(0:20)
plot(0:30)
device_id <- rtvs:::graphics.ide.getactivedeviceid()
rtvs:::graphics.ide.previousplot(device_id)
rtvs:::graphics.ide.previousplot(device_id)
rtvs:::graphics.ide.removeplot(device_id, rtvs:::graphics.ide.getactiveplotid(device_id))
";

            var inputs = Interactive(code);
            var actualPlotFilePaths = await GraphicsTestAsync(inputs);
            actualPlotFilePaths.Should().HaveCount(6);

            OriginalPlotMessages.Last().ActivePlotIndex.Should().Be(0);
            OriginalPlotMessages.Last().PlotCount.Should().Be(2);
        }

        [Test]
        [Category.Plots]
        public async Task RemovePlotLast() {
            var code = @"
plot(0:10)
plot(0:20)
plot(0:30)
device_id <- rtvs:::graphics.ide.getactivedeviceid()
rtvs:::graphics.ide.removeplot(device_id, rtvs:::graphics.ide.getactiveplotid(device_id))
";

            var inputs = Interactive(code);
            var actualPlotFilePaths = await GraphicsTestAsync(inputs);
            actualPlotFilePaths.Should().HaveCount(4);

            OriginalPlotMessages.Last().ActivePlotIndex.Should().Be(1);
            OriginalPlotMessages.Last().PlotCount.Should().Be(2);
        }

        [Test]
        [Category.Plots]
        public async Task RemovePlotMiddle() {
            var code = @"
plot(0:10)
plot(0:20)
plot(0:30)
device_id <- rtvs:::graphics.ide.getactivedeviceid()
rtvs:::graphics.ide.previousplot(device_id)
rtvs:::graphics.ide.removeplot(device_id, rtvs:::graphics.ide.getactiveplotid(device_id))
";

            var inputs = Interactive(code);
            var actualPlotFilePaths = await GraphicsTestAsync(inputs);
            actualPlotFilePaths.Should().HaveCount(5);

            OriginalPlotMessages.Last().ActivePlotIndex.Should().Be(1);
            OriginalPlotMessages.Last().PlotCount.Should().Be(2);
        }

        [Test]
        [Category.Plots]
        public async Task RemovePlotSingle() {
            var code = @"
plot(0:10)
device_id <- rtvs:::graphics.ide.getactivedeviceid()
rtvs:::graphics.ide.removeplot(device_id, rtvs:::graphics.ide.getactiveplotid(device_id))
";

            var inputs = Interactive(code);
            var actualPlotFilePaths = await GraphicsTestAsync(inputs);
            actualPlotFilePaths.Should().HaveCount(1);

            OriginalPlotMessages.Last().ActivePlotIndex.Should().Be(-1);
            OriginalPlotMessages.Last().PlotCount.Should().Be(0);
        }

        [Test]
        [Category.Plots]
        public async Task HistoryResizeOldPlot() {
            var expected1Path = await WriteExpectedImageAsync("png", 360, 360, 96, "Expected1", "plot(0:10)");
            var expected2Path = await WriteExpectedImageAsync("png", 360, 360, 96, "Expected2", "plot(5:15)");
            var expected3Path = await WriteExpectedImageAsync("png", 600, 600, 96, "Expected3", "plot(5:15)");
            var expected4Path = await WriteExpectedImageAsync("png", 600, 600, 96, "Expected4", "plot(0:10)");

            var code = @"
plot(0:10)
plot(5:15)
device_id <- rtvs:::graphics.ide.getactivedeviceid()
rtvs:::graphics.ide.resize(device_id, 600, 600, 96)
rtvs:::graphics.ide.previousplot(device_id)
";

            var inputs = Interactive(code);
            var actualPlotPaths = (await GraphicsTestAsync(inputs)).ToArray();
            var expectedPlotPaths = new string[] { expected1Path, expected2Path, expected3Path, expected4Path };
            CompareImages(actualPlotPaths, expectedPlotPaths);
        }

        [Test]
        [Category.Plots]
        public async Task Locator() {
            var outputFilePath = _files.LocatorResultPath;
            var code = string.Format(@"
plot(0:10)
res <- locator()
write.csv(res, {0})
",
                outputFilePath.ToRPath().ToRStringLiteral());

            var locatorProvider = new TestLocatorResultProvider(new Point[] {
                new Point(10, 10),
                new Point(100, 50),
                new Point(290, 90),
            });

            var inputs = Interactive(code);
            var actualPlotFilePaths = (await GraphicsTestAsync(inputs, locatorProvider.Next)).ToArray();

            // Locator results for the above clicked positions
            var x = new double[] { -2.48008095952895, 1.55378525638498, 10.0697250455366 };
            var y = new double[] { 14.4476461865435, 12.091623959219, 9.73560173189449 };
            CheckLocatorResult(outputFilePath, x, y);
        }

        private void CompareImages(string[] actualPlotPaths, string[] expectedPlotPaths) {
            actualPlotPaths.Select(f => File.ReadAllBytes(f)).ShouldBeEquivalentTo(expectedPlotPaths.Select(f => File.ReadAllBytes(f)));
        }

        private void CheckLocatorResult(string locatorFilePath, double[] x, double[] y) {
            // Example result:
            //"","x","y"
            //"1",-2.48008095952895,14.4476461865435
            //"2",1.55378525638498,12.091623959219
            //"3",10.0697250455366,9.73560173189449
            string all = File.ReadAllText(locatorFilePath);
            string[] lines = File.ReadAllLines(locatorFilePath);
            lines[0].Should().Be("\"\",\"x\",\"y\"");
            x.Should().HaveSameCount(y);
            for (int i = 0; i < x.Length; i++) {
                var expected = $"\"{i+1}\",{x[i]},{y[i]}";
                lines[i + 1].Should().Be(expected);
            }
        }

        internal string SavePlotFile(string plotFilePath, int i) {
            var newFileName = $"{_testMethod.MethodInfo.DeclaringType?.FullName}-{_testMethod.MethodInfo.Name}-{i}{Path.GetExtension(plotFilePath)}";
            var testOutputFilePath = Path.Combine(_files.ActualFolderPath, newFileName);
            File.Copy(plotFilePath, testOutputFilePath);
            return testOutputFilePath;
        }

        private async Task<string> WriteExpectedImageAsync(string imageType, int width, int height, int res, string name, string code) {
            string filePath = _files.GetDestinationPath(_testMethod.MethodInfo.Name + name + "." + imageType);
            var inputs = Batch(string.Format(@"
{0}({1}, width={2}, height={3}, res={4})
{5}
dev.off()
", imageType, filePath.ToRPath().ToRStringLiteral(), width, height, res, code));

            // Don't set PlotHandler, so if any code accidentally triggers a plot msg, it will fail
            await ExecuteInSession(inputs, new RHostClientTestApp());

            return filePath;
        }

        private async Task<IEnumerable<string>> GraphicsTestAsync(string[] inputs, Func<LocatorResult> locatorHandler = null) {
            await ExecuteInSession(inputs, new RHostClientTestApp { PlotHandler = OnPlot, LocatorHandler = locatorHandler, PlotDeviceCreateHandler = OnDeviceCreate, PlotDeviceDestroyHandler = OnDeviceDestroy });

            // Ensure that all plot files created by the graphics device have been deleted
            foreach (var plot in OriginalPlotMessages) {
                File.Exists(plot.FilePath).Should().BeFalse();
            }

            return PlotFilePaths.AsReadOnly();
        }

        private async Task ExecuteInSession(string[] inputs, IRSessionCallback app) {
            using (var sessionProvider = new RSessionProvider(_services)) {
                await sessionProvider.TrySwitchBrokerAsync(nameof(IdeGraphicsDeviceTest));
                var session = sessionProvider.GetOrCreate(_testMethod.FileSystemSafeName);
                await session.StartHostAsync(new RHostStartupInfo(), app, 50000);

                foreach (var input in inputs) {
                    using (var interaction = await session.BeginInteractionAsync()) {
                        await interaction.RespondAsync(input.EnsureLineBreak());
                    }
                }

                await session.StopHostAsync();
            }
        }

        private async Task ExportToImageAsync(IRSession session, string format, string filePath, int widthInPixels,int heightInPixels, int resolution) {
            string script = String.Format(@"
device_id <- rtvs:::graphics.ide.getactivedeviceid()
rtvs:::export_to_image(device_id, rtvs:::graphics.ide.getactiveplotid(device_id), {0}, {1}, {2}, {3})
", format, widthInPixels, heightInPixels, resolution);
            var blobid = await session.EvaluateAsync<ulong>(script, REvaluationKind.Normal);

            using(DataTransferSession dts = new DataTransferSession(session, new WindowsFileSystem())) {
                await dts.FetchFileAsync(new RBlobInfo(blobid), filePath, true, null, CancellationToken.None);
            }
        }

        private async Task<IEnumerable<string>> ExportToImageAsync(string[] inputs, string[] format, string[] paths, int widthInPixels, int heightInPixels, int resolution) {
            var app = new RHostClientTestApp { PlotHandler = OnPlot, PlotDeviceCreateHandler = OnDeviceCreate, PlotDeviceDestroyHandler = OnDeviceDestroy };
            using (var sessionProvider = new RSessionProvider(_services)) {
                await sessionProvider.TrySwitchBrokerAsync(nameof(IdeGraphicsDeviceTest));
                var session = sessionProvider.GetOrCreate(_testMethod.FileSystemSafeName);
                await session.StartHostAsync(new RHostStartupInfo(), app, 50000);

                foreach (var input in inputs) {
                    using (var interaction = await session.BeginInteractionAsync()) {
                        await interaction.RespondAsync(input.EnsureLineBreak());
                    }
                }

                for (int i = 0; i < format.Length; ++i) {
                    await ExportToImageAsync(session, format[i], paths[i], widthInPixels, heightInPixels, resolution);
                }

                await session.StopHostAsync();
            }
            // Ensure that all plot files created by the graphics device have been deleted
            foreach (var plot in OriginalPlotMessages) {
                File.Exists(plot.FilePath).Should().BeFalse();
            }

            return PlotFilePaths.AsReadOnly();
        }

        private async Task<IEnumerable<string>> ExportToPdfAsync(string[] inputs, string filePath, int width, int height) {
            var app = new RHostClientTestApp { PlotHandler = OnPlot, PlotDeviceCreateHandler = OnDeviceCreate, PlotDeviceDestroyHandler = OnDeviceDestroy };
            using (var sessionProvider = new RSessionProvider(_services)) {
                await sessionProvider.TrySwitchBrokerAsync(nameof(IdeGraphicsDeviceTest));
                var session = sessionProvider.GetOrCreate(_testMethod.FileSystemSafeName);
                await session.StartHostAsync(new RHostStartupInfo(), app, 50000);

                foreach (var input in inputs) {
                    using (var interaction = await session.BeginInteractionAsync()) {
                        await interaction.RespondAsync(input.EnsureLineBreak());
                    }
                }

                string script = String.Format(@"
device_id <- rtvs:::graphics.ide.getactivedeviceid()
rtvs:::export_to_pdf(device_id, rtvs:::graphics.ide.getactiveplotid(device_id), {0}, {1})
", width, height);
                var blobid = await session.EvaluateAsync<ulong>(script, REvaluationKind.Normal);
                using (DataTransferSession dts = new DataTransferSession(session, new WindowsFileSystem())) {
                    await dts.FetchFileAsync(new RBlobInfo(blobid), filePath, true, null, CancellationToken.None);
                }

                await session.StopHostAsync();
            }
            // Ensure that all plot files created by the graphics device have been deleted
            foreach (var plot in OriginalPlotMessages) {
                File.Exists(plot.FilePath).Should().BeFalse();
            }

            return PlotFilePaths.AsReadOnly();
        }

        private static string[] Interactive(string code) {
            return code.Split(CharExtensions.LineBreakChars, StringSplitOptions.RemoveEmptyEntries);
        }

        private static string[] Batch(string code) {
            return new[] { code };
        }

        private PlotDeviceProperties OnDeviceCreate(Guid deviceId) {
            return DefaultDeviceProperties;
        }

        private void OnDeviceDestroy(Guid deviceId) {
        }

        private void OnPlot(PlotMessage plot) {
            // We also store the original plot messages, so we can 
            // validate that the files have been deleted when the host goes away
            OriginalPlotMessages.Add(plot);

            if (plot.FilePath.Length <= 0) {
                return;
            }

            // Make a copy of the plot file, and store the path to the copy
            // When the R code finishes executing, the graphics device is destructed,
            // which destructs all the plots which deletes the original plot files
            int index = PlotFilePaths.Count;
            PlotFilePaths.Add(SavePlotFile(plot.FilePath, index));
        }

        class TestLocatorResultProvider {
            private Point[] _points;
            private int _index;

            public TestLocatorResultProvider(Point[] points) {
                _points = points;
            }

            public LocatorResult Next() {
                if (_index < _points.Length) {
                    var res = LocatorResult.CreateClicked(_points[_index].X, _points[_index].Y);
                    _index++;
                    return res;
                }
                return LocatorResult.CreateNotClicked();
            }
        }
    }
}
