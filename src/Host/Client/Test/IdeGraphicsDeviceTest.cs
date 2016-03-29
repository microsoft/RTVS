// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Common.Core.Test.Utility;
using Microsoft.R.Host.Client.Session;
using Microsoft.R.Host.Client.Test.Script;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;

namespace Microsoft.R.Host.Client.Test {
    [ExcludeFromCodeCoverage]
    public class IdeGraphicsDeviceTest {
        private readonly GraphicsDeviceTestFilesFixture _files;
        private readonly MethodInfo _testMethod;

        private const int DefaultWidth = 360;
        private const int DefaultHeight = 360;

        private const int DefaultExportWidth = 480;
        private const int DefaultExportHeight = 480;

        public List<string> PlotFilePaths { get; } = new List<string>();
        public List<string> OriginalPlotFilePaths { get; } = new List<string>();

        public IdeGraphicsDeviceTest(GraphicsDeviceTestFilesFixture files, TestMethodFixture testMethod) {
            _files = files;
            _testMethod = testMethod.MethodInfo;
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
            var code = @"
plot(cars)
";
            var inputs = Batch(code);
            await GraphicsTestAgainstExpectedFilesAsync(inputs);
        }

        [Test]
        [Category.Plots]
        public async Task SetInitialSize() {
            var code = @"
rtvs:::graphics.ide.resize(600, 600)
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
rtvs:::graphics.ide.resize(600, 600)
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
rtvs:::graphics.ide.resize(600, 600)
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
        public async Task ExportToImage() {
            var exportedBmpFilePath = _files.ExportToBmpResultPath;
            var exportedPngFilePath = _files.ExportToPngResultPath;
            var exportedJpegFilePath = _files.ExportToJpegResultPath;
            var exportedTiffFilePath = _files.ExportToTiffResultPath;

            var code = string.Format(@"
plot(0:10)
rtvs:::graphics.ide.exportimage({0}, bmp, {4}, {5})
rtvs:::graphics.ide.exportimage({1}, png, {4}, {5})
rtvs:::graphics.ide.exportimage({2}, jpeg, {4}, {5})
rtvs:::graphics.ide.exportimage({3}, tiff, {4}, {5})
",
                QuotedRPath(exportedBmpFilePath),
                QuotedRPath(exportedPngFilePath),
                QuotedRPath(exportedJpegFilePath),
                QuotedRPath(exportedTiffFilePath),
                DefaultExportWidth,
                DefaultExportHeight);

            var inputs = Interactive(code);
            var actualPlotFilePaths = await GraphicsTestAsync(inputs);
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
            var exportedBmpFilePath = _files.ExportPreviousPlotToImageResultPath;

            var code = string.Format(@"
plot(0:10)
plot(10:20)
rtvs:::graphics.ide.previousplot()
rtvs:::graphics.ide.exportimage({0}, bmp, {1}, {2})
",
                QuotedRPath(exportedBmpFilePath),
                DefaultWidth,
                DefaultHeight
            );

            var inputs = Interactive(code);
            var actualPlotFilePaths = await GraphicsTestAsync(inputs);
            actualPlotFilePaths.Should().HaveCount(3);

            File.ReadAllBytes(exportedBmpFilePath).Should().Equal(File.ReadAllBytes(_files.ExpectedExportPreviousPlotToImagePath));
        }

        [Test]
        [Category.Plots]
        public async Task ExportToPdf() {
            var exportedFilePath = _files.ExportToPdfResultPath;

            var code = string.Format(@"
plot(0:10)
rtvs:::graphics.ide.exportpdf({0}, {1}, {2})
",
                QuotedRPath(exportedFilePath),
                7,
                7
            );

            var inputs = Interactive(code);
            var actualPlotFilePaths = await GraphicsTestAsync(inputs);
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
            var code = @"
par(mfrow = c(2, 2))
plot(0:1)
plot(1:2)
plot(2:3)
plot(3:4)
rtvs:::graphics.ide.resize(600, 600)
";
            var inputs = Interactive(code);
            await GraphicsTestAgainstExpectedFilesAsync(inputs);
        }

        [Test]
        [Category.Plots]
        public async Task ResizeNonInteractiveMultiPlots() {
            // Resize a graph with multiple plots, where the
            // code is executed all at once
            // Make sure that all parts of the graph are present
            // We used to have a bug where the resized image only had
            // the top left plot, and the others were missing
            var inputs = new [] {
                @"
par(mfrow = c(2, 2))
plot(0:1)
plot(1:2)
plot(2:3)
plot(3:4)
",
"rtvs:::graphics.ide.resize(600, 600)",
            };
            await GraphicsTestAgainstExpectedFilesAsync(inputs);
        }

        [Test]
        [Category.Plots]
        public async Task HistoryInfo() {
            var outputFilePath = _files.HistoryInfoResultPath;
            var code = string.Format(@"
plot(0:10)
plot(5:15)
rtvs:::graphics.ide.previousplot()
info <- rtvs:::toJSON(rtvs:::graphics.ide.historyinfo())
write(info, {0})
",
                QuotedRPath(outputFilePath));

            var inputs = Interactive(code);
            var actualPlotFilePaths = await GraphicsTestAgainstExpectedFilesAsync(inputs);
            actualPlotFilePaths.Should().HaveCount(3);

            File.ReadAllBytes(actualPlotFilePaths[2]).Should().Equal(File.ReadAllBytes(actualPlotFilePaths[0]));
            File.ReadAllBytes(actualPlotFilePaths[1]).Should().NotEqual(File.ReadAllBytes(actualPlotFilePaths[0]));

            CheckHistoryResult(outputFilePath, expectedActive: 0, expectedCount: 2);
        }

        [Test]
        [Category.Plots]
        public async Task ClearPlots() {
            var outputFilePath = _files.ClearPlotsResultPath;
            var code = string.Format(@"
plot(0:10)
plot(0:15)
rtvs:::graphics.ide.clearplots()
info <- rtvs:::toJSON(rtvs:::graphics.ide.historyinfo())
write(info, {0})
",
                QuotedRPath(outputFilePath));

            var inputs = Interactive(code);
            var actualPlotFilePaths = await GraphicsTestAsync(inputs);
            actualPlotFilePaths.Should().HaveCount(2);

            CheckHistoryResult(outputFilePath, expectedActive: -1, expectedCount: 0);
        }

        [Test]
        [Category.Plots]
        public async Task RemovePlotFirst() {
            var outputFilePath = _files.ClearPlotsResultPath;
            var code = string.Format(@"
plot(0:10)
plot(0:20)
plot(0:30)
rtvs:::graphics.ide.previousplot()
rtvs:::graphics.ide.previousplot()
rtvs:::graphics.ide.removeplot()
info <- rtvs:::toJSON(rtvs:::graphics.ide.historyinfo())
write(info, {0})
",
                QuotedRPath(outputFilePath));

            var inputs = Interactive(code);
            var actualPlotFilePaths = await GraphicsTestAsync(inputs);
            actualPlotFilePaths.Should().HaveCount(6);

            CheckHistoryResult(outputFilePath, expectedActive: 0, expectedCount: 2);
        }

        [Test]
        [Category.Plots]
        public async Task RemovePlotLast() {
            var outputFilePath = _files.ClearPlotsResultPath;
            var code = string.Format(@"
plot(0:10)
plot(0:20)
plot(0:30)
rtvs:::graphics.ide.removeplot()
info <- rtvs:::toJSON(rtvs:::graphics.ide.historyinfo())
write(info, {0})
",
                QuotedRPath(outputFilePath));

            var inputs = Interactive(code);
            var actualPlotFilePaths = await GraphicsTestAsync(inputs);
            actualPlotFilePaths.Should().HaveCount(4);

            CheckHistoryResult(outputFilePath, expectedActive: 1, expectedCount: 2);
        }

        [Test]
        [Category.Plots]
        public async Task RemovePlotMiddle() {
            var outputFilePath = _files.ClearPlotsResultPath;
            var code = string.Format(@"
plot(0:10)
plot(0:20)
plot(0:30)
rtvs:::graphics.ide.previousplot()
rtvs:::graphics.ide.removeplot()
info <- rtvs:::toJSON(rtvs:::graphics.ide.historyinfo())
write(info, {0})
",
                QuotedRPath(outputFilePath));

            var inputs = Interactive(code);
            var actualPlotFilePaths = await GraphicsTestAsync(inputs);
            actualPlotFilePaths.Should().HaveCount(5);

            CheckHistoryResult(outputFilePath, expectedActive: 1, expectedCount: 2);
        }

        [Test]
        [Category.Plots]
        public async Task RemovePlotSingle() {
            var outputFilePath = _files.ClearPlotsResultPath;
            var code = string.Format(@"
plot(0:10)
rtvs:::graphics.ide.removeplot()
info <- rtvs:::toJSON(rtvs:::graphics.ide.historyinfo())
write(info, {0})
",
                QuotedRPath(outputFilePath));

            var inputs = Interactive(code);
            var actualPlotFilePaths = await GraphicsTestAsync(inputs);
            actualPlotFilePaths.Should().HaveCount(1);

            CheckHistoryResult(outputFilePath, expectedActive: -1, expectedCount: 0);
        }

        [Test]
        [Category.Plots]
        public async Task HistoryResizeOldPlot() {
            var code = @"
plot(0:10)
plot(5:15)
rtvs:::graphics.ide.resize(600, 600)
rtvs:::graphics.ide.previousplot()
";

            var inputs = Interactive(code);
            var actualPlotFilePaths = await GraphicsTestAgainstExpectedFilesAsync(inputs);
            actualPlotFilePaths.Should().HaveCount(4);

            var bmp1 = (Bitmap)Image.FromFile(actualPlotFilePaths[0]);
            var bmp2 = (Bitmap)Image.FromFile(actualPlotFilePaths[1]);
            var bmp3 = (Bitmap)Image.FromFile(actualPlotFilePaths[2]);
            var bmp4 = (Bitmap)Image.FromFile(actualPlotFilePaths[3]);
            bmp1.Width.Should().Be(DefaultWidth);
            bmp1.Height.Should().Be(DefaultHeight);
            bmp2.Width.Should().Be(DefaultWidth);
            bmp2.Height.Should().Be(DefaultHeight);
            bmp3.Width.Should().Be(600);
            bmp3.Height.Should().Be(600);
            bmp4.Width.Should().Be(600);
            bmp4.Height.Should().Be(600);
        }

        private void CheckHistoryResult(string historyInfoFilePath, int expectedActive, int expectedCount) {
            string json = File.ReadAllText(historyInfoFilePath).Trim();
            json.Should().Be($"[{expectedActive},{expectedCount}]");
        }

        private async Task<string[]> GraphicsTestAgainstExpectedFilesAsync(string[] inputs) {
            var actualPlotPaths = (await GraphicsTestAsync(inputs)).ToArray();
            var expectedPlotPaths = GetTestExpectedFiles();
            actualPlotPaths.Length.Should().Be(expectedPlotPaths.Length);

            foreach (string path in expectedPlotPaths) {
                var actualPlotPath = actualPlotPaths.First(p => Path.GetFileName(p) == Path.GetFileName(path));
                File.ReadAllBytes(actualPlotPath).Should().Equal(File.ReadAllBytes(path));
            }

            return actualPlotPaths;
        }

        private string[] GetTestExpectedFiles() {
            var folderPath = _files.DestinationPath;
            var expectedFilesFilter = $"{_testMethod.DeclaringType?.FullName}-{_testMethod.Name}-*.png";
            var expectedFiles = Directory.GetFiles(folderPath, expectedFilesFilter);
            return expectedFiles;
        }

        internal string SavePlotFile(string plotFilePath, int i) {
            var newFileName = $"{_testMethod.DeclaringType?.FullName}-{_testMethod.Name}-{i}{Path.GetExtension(plotFilePath)}";
            var testOutputFilePath = Path.Combine(_files.ActualFolderPath, newFileName);
            File.Copy(plotFilePath, testOutputFilePath);
            return testOutputFilePath;
        }

        private async Task<IEnumerable<string>> GraphicsTestAsync(string[] inputs) {
            using (var sessionProvider = new RSessionProvider()) {
                var session = sessionProvider.GetOrCreate(Guid.NewGuid(), new RHostClientTestApp {PlotHandler = OnPlot});
                await session.StartHostAsync(new RHostStartupInfo {
                    Name = _testMethod.Name,
                    RBasePath = RUtilities.FindExistingRBasePath()
                }, 50000);

                foreach (var input in inputs) {
                    using (var interaction = await session.BeginInteractionAsync()) {
                        await interaction.RespondAsync(input + "\n");
                    }
                }

                await session.StopHostAsync();
            }
            
            // Ensure that all plot files created by the graphics device have been deleted
            foreach (var deletedFilePath in OriginalPlotFilePaths) {
                File.Exists(deletedFilePath).Should().BeFalse();
            }

            return PlotFilePaths.AsReadOnly();
        }

        private static string QuotedRPath(string path) {
            return '"' + path.Replace("\\", "/") + '"';
        }

        private static string[] Interactive(string code) {
            return code.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
        }

        private static string[] Batch(string code) {
            return new[] { code };
        }

        private void OnPlot(string filePath) {
            if (filePath.Length <= 0) {
                return;
            }

            // Make a copy of the plot file, and store the path to the copy
            // When the R code finishes executing, the graphics device is destructed,
            // which destructs all the plots which deletes the original plot files
            int index = PlotFilePaths.Count;
            PlotFilePaths.Add(SavePlotFile(filePath, index));

            // We also store the original plot file paths, so we can 
            // validate that they have been deleted when the host goes away
            OriginalPlotFilePaths.Add(filePath);
        }
    }
}
