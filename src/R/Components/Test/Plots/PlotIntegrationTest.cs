// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using FluentAssertions;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Tasks;
using Microsoft.Common.Wpf.Imaging;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Components.Plots;
using Microsoft.R.Components.Plots.Implementation.View;
using Microsoft.R.Components.Test.Fakes.InteractiveWindow;
using Microsoft.R.Components.Test.Fakes.Shell;
using Microsoft.R.Host.Client;
using Microsoft.UnitTests.Core.Mef;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.UnitTests.Core.XUnit.MethodFixtures;
using Xunit;

namespace Microsoft.R.Components.Test.Plots {
    [ExcludeFromCodeCoverage]
    public class PlotIntegrationTest : IAsyncLifetime {
        private readonly IExportProvider _exportProvider;
        private readonly TestRInteractiveWorkflowProvider _workflowProvider;
        private readonly IRInteractiveWorkflow _workflow;
        private readonly IInteractiveWindowComponentContainerFactory _componentContainerFactory;
        private readonly MethodInfo _testMethod;
        private readonly TestFilesFixture _testFiles;

        public PlotIntegrationTest(RComponentsMefCatalogFixture catalog, TestMethodFixture testMethod, TestFilesFixture testFiles) {
            _exportProvider = catalog.CreateExportProvider();
            _workflowProvider = _exportProvider.GetExportedValue<TestRInteractiveWorkflowProvider>();
            _workflowProvider.TestName = nameof(PlotIntegrationTest);
            _workflow = _exportProvider.GetExportedValue<IRInteractiveWorkflowProvider>().GetOrCreate();
            _componentContainerFactory = _exportProvider.GetExportedValue<IInteractiveWindowComponentContainerFactory>();
            _testMethod = testMethod.MethodInfo;
            _testFiles = testFiles;
        }

        public Task InitializeAsync() {
            return Task.CompletedTask;
        }

        public Task DisposeAsync() {
            _exportProvider.Dispose();
            return Task.CompletedTask;
        }

        private TestCoreShell CoreShell {
            get { return _workflow.Shell as TestCoreShell; }
        }

        [Test(ThreadType.UI)]
        [Category.Plots]
        public async Task AllCommandsDisabledWhenNoPlot() {
            using (await _workflow.GetOrCreateVisualComponent(_componentContainerFactory)) {
                CheckEnabledCommands(isFirst: false, isLast: false, anyPlot: false);
            }
        }

        [Test(ThreadType.UI)]
        [Category.Plots]
        public async Task SomeCommandsEnabledForSinglePlot() {
            using (await _workflow.GetOrCreateVisualComponent(_componentContainerFactory)) {
                var plot1to10 = await GetExpectedImageAsync("png", 600, 500, 96, "plot1-10", "plot(1:10)");

                await _workflow.Plots.ResizeAsync(600, 500, 96);

                await ExecuteAndWaitForPlotsAsync(new string[] {
                    "plot(1:10)",
                });

                var viewModel = ((RPlotManagerControl)_workflow.Plots.VisualComponent.Control).Model;
                viewModel.PlotImage.Should().HaveSamePixels(plot1to10);

                CheckEnabledCommands(isFirst: true, isLast: true, anyPlot: true);
            }
        }

        [Test(ThreadType.UI)]
        [Category.Plots]
        public async Task SomeCommandsEnabledForLastPlot() {
            using (await _workflow.GetOrCreateVisualComponent(_componentContainerFactory)) {
                var plot10to20 = await GetExpectedImageAsync("png", 600, 500, 96, "plot10-20", "plot(10:20)");

                await _workflow.Plots.ResizeAsync(600, 500, 96);

                await ExecuteAndWaitForPlotsAsync(new string[] {
                    "plot(1:10)",
                    "plot(10:20)",
                });

                var viewModel = ((RPlotManagerControl)_workflow.Plots.VisualComponent.Control).Model;
                viewModel.PlotImage.Should().HaveSamePixels(plot10to20);

                CheckEnabledCommands(isFirst: false, isLast: true, anyPlot: true);
            }
        }

        [Test(ThreadType.UI)]
        [Category.Plots]
        public async Task SomeCommandsEnabledForMiddlePlot() {
            using (await _workflow.GetOrCreateVisualComponent(_componentContainerFactory)) {
                var plot10to20 = await GetExpectedImageAsync("png", 600, 500, 96, "plot10-20", "plot(10:20)");

                await _workflow.Plots.ResizeAsync(600, 500, 96);

                await ExecuteAndWaitForPlotsAsync(new string[] {
                    "plot(1:10)",
                    "plot(10:20)",
                    "plot(20:30)",
                });

                await WaitForPlotAsync(async delegate {
                    await _workflow.Plots.Commands.Previous.InvokeAsync();
                });

                var viewModel = ((RPlotManagerControl)_workflow.Plots.VisualComponent.Control).Model;
                viewModel.PlotImage.Should().HaveSamePixels(plot10to20);

                CheckEnabledCommands(isFirst: false, isLast: false, anyPlot: true);
            }
        }

        [Test(ThreadType.UI)]
        [Category.Plots]
        public async Task SomeCommandsEnabledForFirstPlot() {
            using (await _workflow.GetOrCreateVisualComponent(_componentContainerFactory)) {
                var plot1to10 = await GetExpectedImageAsync("png", 600, 500, 96, "plot1-10", "plot(1:10)");

                await _workflow.Plots.ResizeAsync(600, 500, 96);

                await ExecuteAndWaitForPlotsAsync(new string[] {
                    "plot(1:10)",
                    "plot(10:20)",
                });

                await WaitForPlotAsync(async delegate {
                    await _workflow.Plots.Commands.Previous.InvokeAsync();
                });

                var viewModel = ((RPlotManagerControl)_workflow.Plots.VisualComponent.Control).Model;
                viewModel.PlotImage.Should().HaveSamePixels(plot1to10);

                CheckEnabledCommands(isFirst: true, isLast: false, anyPlot: true);
            }
        }

        [Test(ThreadType.UI)]
        [Category.Plots]
        public async Task CopyAsBitmap() {
            using (await _workflow.GetOrCreateVisualComponent(_componentContainerFactory)) {
                var plot1to10 = await GetExpectedImageAsync("bmp", 600, 500, 96, "plot1-10", "plot(1:10)");

                // We set an initial size for plots, because export as image command
                // will use the current size of plot control as export parameter.
                await _workflow.Plots.ResizeAsync(600, 500, 96);

                await ExecuteAndWaitForPlotsAsync(new string[] {
                    "plot(1:10)",
                });

                Clipboard.Clear();

                _workflow.Plots.Commands.CopyAsBitmap.Should().BeEnabled();
                await _workflow.Plots.Commands.CopyAsBitmap.InvokeAsync();

                Clipboard.ContainsImage().Should().BeTrue();
                CoreShell.LastShownErrorMessage.Should().BeNullOrEmpty();

                var clipboardImage = Clipboard.GetImage();
                clipboardImage.Should().HaveSamePixels(plot1to10);
            }
        }

        [Test(ThreadType.UI)]
        [Category.Plots]
        public async Task CopyAsMetafile() {
            using (await _workflow.GetOrCreateVisualComponent(_componentContainerFactory)) {
                // We set an initial size for plots, because export as image command
                // will use the current size of plot control as export parameter.
                await _workflow.Plots.ResizeAsync(600, 500, 96);

                await ExecuteAndWaitForPlotsAsync(new string[] {
                    "plot(1:10)",
                });

                Clipboard.Clear();

                _workflow.Plots.Commands.CopyAsMetafile.Should().BeEnabled();
                await _workflow.Plots.Commands.CopyAsMetafile.InvokeAsync();

                Clipboard.ContainsData(DataFormats.EnhancedMetafile).Should().BeTrue();
                CoreShell.LastShownErrorMessage.Should().BeNullOrEmpty();
            }
        }

        [Test(ThreadType.UI)]
        [Category.Plots]
        public async Task ExportAsPdf() {
            using (await _workflow.GetOrCreateVisualComponent(_componentContainerFactory)) {
                // We set an initial size for plots, because export as image command
                // will use the current size of plot control as export parameter.
                await _workflow.Plots.ResizeAsync(600, 500, 96);

                await ExecuteAndWaitForPlotsAsync(new string[] {
                    "plot(1:10)",
                });

                var outputFilePath = _testFiles.GetDestinationPath("ExportedPlot.pdf");
                CoreShell.SaveFilePath = outputFilePath;

                _workflow.Plots.Commands.ExportAsPdf.Should().BeEnabled();
                await _workflow.Plots.Commands.ExportAsPdf.InvokeAsync();

                File.Exists(outputFilePath).Should().BeTrue();
                CoreShell.LastShownErrorMessage.Should().BeNullOrEmpty();
            }
        }

        [Test(ThreadType.UI)]
        [Category.Plots]
        public async Task ExportAsImage() {
            using (await _workflow.GetOrCreateVisualComponent(_componentContainerFactory)) {
                // We set an initial size for plots, because export as image command
                // will use the current size of plot control as export parameter.
                await _workflow.Plots.ResizeAsync(600, 500, 96);

                await ExecuteAndWaitForPlotsAsync(new string[] {
                    "plot(1:10)",
                });

                foreach (var ext in new string[] { "bmp", "jpg", "jpeg", "png", "tif", "tiff" }) {
                    var outputFilePath = _testFiles.GetDestinationPath("ExportedPlot." + ext);
                    CoreShell.SaveFilePath = outputFilePath;

                    _workflow.Plots.Commands.ExportAsImage.Should().BeEnabled();
                    await _workflow.Plots.Commands.ExportAsImage.InvokeAsync();

                    File.Exists(outputFilePath).Should().BeTrue();
                    CoreShell.LastShownErrorMessage.Should().BeNullOrEmpty();

                    var image = BitmapImageFactory.Load(outputFilePath);
                    image.PixelWidth.Should().Be(600);
                    image.PixelHeight.Should().Be(500);
                    ((int)Math.Round(image.DpiX)).Should().Be(96);
                    ((int)Math.Round(image.DpiY)).Should().Be(96);
                }
            }
        }

        [Test(ThreadType.UI)]
        [Category.Plots]
        public async Task ExportAsImageUnsupportedExtension() {
            using (await _workflow.GetOrCreateVisualComponent(_componentContainerFactory)) {
                // We set an initial size for plots, because export as image command
                // will use the current size of plot control as export parameter.
                await _workflow.Plots.ResizeAsync(600, 500, 96);

                await ExecuteAndWaitForPlotsAsync(new string[] {
                    "plot(1:10)",
                });

                // The file extension of the file the user selected in the save
                // dialog is what determines the image format. When it's an
                // unsupported format, we show an error msg.
                var outputFilePath = _testFiles.GetDestinationPath("ExportedPlot.unsupportedextension");
                CoreShell.SaveFilePath = outputFilePath;

                _workflow.Plots.Commands.ExportAsImage.Should().BeEnabled();
                await _workflow.Plots.Commands.ExportAsImage.InvokeAsync();

                File.Exists(CoreShell.SaveFilePath).Should().BeFalse();
                CoreShell.LastShownErrorMessage.Should().Contain(".unsupportedextension");
            }
        }

        [Test(ThreadType.UI)]
        [Category.Plots]
        public async Task PlotBuiltinDatasets() {
            // Note that the following are not included, as they cause various errors:
            //   Harman23.cor, Harman74.cor, ability.cov, state.abb, state.name
            // Since the test waits for each plot to be received before moving on to
            // the next dataset, we don't execute statements that don't generate plots.
            string[] scripts = new string[] {
                "plot(AirPassengers)",
                "plot(BJsales)",
                "plot(BJsales.lead)",
                "plot(BOD)",
                "plot(CO2)",
                "plot(ChickWeight)",
                "plot(DNase)",
                "plot(EuStockMarkets)",
                "plot(Formaldehyde)",
                "plot(HairEyeColor)",
                "plot(Indometh)",
                "plot(InsectSprays)",
                "plot(JohnsonJohnson)",
                "plot(LakeHuron)",
                "plot(LifeCycleSavings)",
                "plot(Loblolly)",
                "plot(Nile)",
                "plot(Orange)",
                "plot(OrchardSprays)",
                "plot(PlantGrowth)",
                "plot(Puromycin)",
                "plot(Seatbelts)",
                "plot(Theoph)",
                "plot(Titanic)",
                "plot(ToothGrowth)",
                "plot(UCBAdmissions)",
                "plot(UKDriverDeaths)",
                "plot(UKgas)",
                "plot(USAccDeaths)",
                "plot(USArrests)",
                "plot(USJudgeRatings)",
                "plot(USPersonalExpenditure)",
                "plot(UScitiesD)",
                "plot(VADeaths)",
                "plot(WWWusage)",
                "plot(WorldPhones)",
                "plot(airmiles)",
                "plot(airquality)",
                "plot(anscombe)",
                "plot(attenu)",
                "plot(attitude)",
                "plot(austres)",
                "plot(beaver1)",
                "plot(beaver2)",
                "plot(cars)",
                "plot(chickwts)",
                "plot(co2)",
                "plot(crimtab)",
                "plot(discoveries)",
                "plot(esoph)",
                "plot(euro)",
                "plot(euro.cross)",
                "plot(eurodist)",
                "plot(faithful)",
                "plot(fdeaths)",
                "plot(freeny)",
                "plot(freeny.x)",
                "plot(freeny.y)",
                "plot(infert)",
                "plot(iris)",
                "plot(iris3)",
                "plot(islands)",
                "plot(ldeaths)",
                "plot(lh)",
                "plot(longley)",
                "plot(lynx)",
                "plot(mdeaths)",
                "plot(morley)",
                "plot(mtcars)",
                "plot(nhtemp)",
                "plot(nottem)",
                "plot(npk)",
                "plot(occupationalStatus)",
                "plot(precip)",
                "plot(presidents)",
                "plot(pressure)",
                "plot(quakes)",
                "plot(randu)",
                "plot(rivers)",
                "plot(rock)",
                "plot(sleep)",
                "plot(stack.loss)",
                "plot(stack.x)",
                "plot(stackloss)",
                "plot(state.area)",
                "plot(state.center)",
                "plot(state.division)",
                "plot(state.region)",
                "plot(state.x77)",
                "plot(sunspot.month)",
                "plot(sunspot.year)",
                "plot(sunspots)",
                "plot(swiss)",
                "plot(treering)",
                "plot(trees)",
                "plot(uspop)",
                "plot(volcano)",
                "plot(warpbreaks)",
                "plot(women)",
            };

            using (await _workflow.GetOrCreateVisualComponent(_componentContainerFactory)) {
                await _workflow.Plots.ResizeAsync(600, 500, 96);

                await ExecuteAndWaitForPlotsAsync(scripts);

                for (int i = 0; i < scripts.Length - 1; i++) {
                    _workflow.Plots.Commands.Previous.Should().BeEnabled();
                    await WaitForPlotAsync(async delegate {
                        await _workflow.Plots.Commands.Previous.InvokeAsync();
                    });
                }

                _workflow.Plots.Commands.Previous.Should().BeDisabled();

                for (int i = 0; i < scripts.Length - 1; i++) {
                    _workflow.Plots.Commands.Next.Should().BeEnabled();
                    await WaitForPlotAsync(async delegate {
                        await _workflow.Plots.Commands.Next.InvokeAsync();
                    });
                }

                _workflow.Plots.Commands.Next.Should().BeDisabled();
            }
        }

        [Test(ThreadType.UI)]
        [Category.Plots]
        public async Task PlotError() {
            using (await _workflow.GetOrCreateVisualComponent(_componentContainerFactory)) {
                // We set an initial size for plots, because export as image command
                // will use the current size of plot control as export parameter.
                await _workflow.Plots.ResizeAsync(600, 500, 96);

                await ExecuteAndWaitForPlotsAsync(new string[] {
                    "plot(state.area)",
                });

                // This is a plot statement that does not successfully generates a plot.
                // Right now the graphics device doesn't send a plot when a
                // new plot cannot be rendered, so don't wait for one.
                await ExecuteAndDoNotWaitForPlotsAsync(new string[] {
                    "plot(state.name)",
                });

                await ExecuteAndWaitForPlotsAsync(new string[] {
                    "plot(state.region)",
                });

                var viewModel = ((RPlotManagerControl)_workflow.Plots.VisualComponent.Control).Model;

                // Navigating to a plot in the history that cannot be rendered
                // will send a plot with zero-file size, which translate into an error message.
                _workflow.Plots.Commands.Previous.Should().BeEnabled();
                await WaitForPlotAsync(async delegate {
                    await _workflow.Plots.Commands.Previous.InvokeAsync();
                });

                viewModel.PlotImage.Should().BeNull();
                viewModel.ShowError.Should().BeTrue();
                viewModel.ShowWatermark.Should().BeFalse();

                _workflow.Plots.Commands.Previous.Should().BeEnabled();
                await WaitForPlotAsync(async delegate {
                    await _workflow.Plots.Commands.Previous.InvokeAsync();
                });

                viewModel.PlotImage.Should().NotBeNull();
                viewModel.ShowError.Should().BeFalse();
                viewModel.ShowWatermark.Should().BeFalse();

                _workflow.Plots.Commands.Previous.Should().BeDisabled();
            }
        }

        [Test(ThreadType.UI)]
        [Category.Plots]
        public async Task LocatorCommand() {
            using (await _workflow.GetOrCreateVisualComponent(_componentContainerFactory)) {
                await _workflow.Plots.ResizeAsync(360, 360, 96);

                await ExecuteAndWaitForPlotsAsync(new string[] {
                    "plot(0:10)",
                });

                _workflow.Plots.IsInLocatorMode.Should().BeFalse();
                _workflow.Plots.Commands.EndLocator.Should().BeInvisibleAndDisabled();

                await WaitForLocatorModeChangedAsync(delegate {
                    ExecuteAndDoNotWaitForPlotsAsync(new string[] {
                        "res <- locator()",
                    }).DoNotWait();
                });

                var points = new Point[] {
                    new Point(10, 10),
                    new Point(100, 50),
                    new Point(290, 90),
                };

                // R's high-level locator() function enters a loop that calls into
                // the graphics device low-level locator() API, which calls back into VS
                // to set locator mode and waits for either:
                // - a result with a click point
                // - a not clicked result
                // The high-level locator() function stops its loop when it gets
                // the not clicked result.
                foreach (var point in points) {
                    _workflow.Plots.IsInLocatorMode.Should().BeTrue();
                    _workflow.Plots.Commands.EndLocator.Should().BeEnabled();

                    // Send a result with a click point, which will causes
                    // locator mode to end and immediately start again
                    await WaitForLocatorModeChangedAsync(delegate {
                        _workflow.Plots.VisualComponent.Click((int)point.X, (int)point.Y);
                    });

                    _workflow.Plots.IsInLocatorMode.Should().BeFalse();
                    _workflow.Plots.Commands.EndLocator.Should().BeInvisibleAndDisabled();

                    await WaitForLocatorModeChangedAsync(() => { });
                }

                // Send a result with a not clicked result, which causes
                // locator mode to end, and the high-level locator() function
                // call will return.
                await WaitForLocatorModeChangedAsync(async delegate {
                    await _workflow.Plots.Commands.EndLocator.InvokeAsync();
                });

                _workflow.Plots.IsInLocatorMode.Should().BeFalse();
                _workflow.Plots.Commands.EndLocator.Should().BeInvisibleAndDisabled();

                string outputFilePath = _testFiles.GetDestinationPath("LocatorResult.csv");
                await ExecuteAndDoNotWaitForPlotsAsync(new string[] {
                    string.Format("write.csv(res, {0})", outputFilePath.ToRPath().ToRStringLiteral())
                });

                var x = new double[] { -2.48008095952895, 1.55378525638498, 10.0697250455366 };
                var y = new double[] { 14.4476461865435, 12.091623959219, 9.73560173189449 };
                CheckLocatorResult(outputFilePath, x, y);
            }
        }

        [Test(ThreadType.UI)]
        [Category.Plots]
        public async Task LocatorCommandNoClick() {
            using (await _workflow.GetOrCreateVisualComponent(_componentContainerFactory)) {
                await _workflow.Plots.ResizeAsync(360, 360, 96);

                await ExecuteAndWaitForPlotsAsync(new string[] {
                    "plot(0:10)",
                });

                await WaitForLocatorModeChangedAsync(delegate {
                    ExecuteAndDoNotWaitForPlotsAsync(new string[] {
                        "res <- locator()",
                    }).DoNotWait();
                });

                _workflow.Plots.IsInLocatorMode.Should().BeTrue();
                _workflow.Plots.Commands.EndLocator.Should().BeEnabled();

                await WaitForLocatorModeChangedAsync(async delegate {
                    await _workflow.Plots.Commands.EndLocator.InvokeAsync();
                });

                _workflow.Plots.IsInLocatorMode.Should().BeFalse();
                _workflow.Plots.Commands.EndLocator.Should().BeInvisibleAndDisabled();

                string outputFilePath = _testFiles.GetDestinationPath("LocatorResultNoClick.csv");
                await ExecuteAndDoNotWaitForPlotsAsync(new string[] {
                    string.Format("write.csv(res, {0})", outputFilePath.ToRPath().ToRStringLiteral())
                });

                string output = File.ReadAllText(outputFilePath);
                output.Trim().Should().Be("\"\"");
            }
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
                var expected = $"\"{i + 1}\",{x[i]},{y[i]}";
                lines[i + 1].Should().Be(expected);
            }
        }

        private async Task ExecuteAndWaitForPlotsAsync(string[] scripts) {
            var eval = _workflow.ActiveWindow.InteractiveWindow.Evaluator;
            foreach (string script in scripts) {
                await WaitForPlotAsync(async delegate {
                    var result = await eval.ExecuteCodeAsync(script.EnsureLineBreak());
                    result.IsSuccessful.Should().BeTrue();
                });
            }
        }

        private async Task ExecuteAndDoNotWaitForPlotsAsync(string[] scripts) {
            var eval = _workflow.ActiveWindow.InteractiveWindow.Evaluator;
            foreach (string script in scripts) {
                var result = await eval.ExecuteCodeAsync(script.EnsureLineBreak());
                result.IsSuccessful.Should().BeTrue();
            }
        }

        private async Task WaitForPlotAsync(Func<Task> action) {
            var eas = new EventTaskSource<IRPlotManager>((o, h) => o.PlotChanged += h, (o, h) => o.PlotChanged -= h);
            var plotChangedTask = eas.Create(_workflow.Plots);
            await action();
            await plotChangedTask;
        }

        private async Task WaitForLocatorModeChangedAsync(Action action) {
            var eas = new EventTaskSource<IRPlotManager>((o, h) => o.LocatorModeChanged += h, (o, h) => o.LocatorModeChanged -= h);
            var locatorModeChangedTask = eas.Create(_workflow.Plots);
            action();
            await locatorModeChangedTask;
        }

        private async Task WaitForLocatorModeChangedAsync(Func<Task> action) {
            var eas = new EventTaskSource<IRPlotManager>((o, h) => o.LocatorModeChanged += h, (o, h) => o.LocatorModeChanged -= h);
            var locatorModeChangedTask = eas.Create(_workflow.Plots);
            await action();
            await locatorModeChangedTask;
        }

        private async Task<BitmapImage> GetExpectedImageAsync(string imageType, int width, int height, int res, string name, string code) {
            var filePath = _testFiles.GetDestinationPath(_testMethod.Name + name + "." + imageType);
            var script = string.Format(@"
{0}({1}, width={2}, height={3}, res={4})
{5}
dev.off()
", imageType, filePath.ToRPath().ToRStringLiteral(), width, height, res, code);

            var eval = _workflow.ActiveWindow.InteractiveWindow.Evaluator;
            var result = await eval.ExecuteCodeAsync(script);
            return BitmapImageFactory.Load(filePath);
        }

        private void CheckEnabledCommands(bool isFirst, bool isLast, bool anyPlot) {
            _workflow.Plots.Commands.EndLocator.Should().BeDisabled();

            if (anyPlot) {
                _workflow.Plots.Commands.RemoveAll.Should().BeEnabled();
                _workflow.Plots.Commands.RemoveCurrent.Should().BeEnabled();
                _workflow.Plots.Commands.CopyAsBitmap.Should().BeEnabled();
                _workflow.Plots.Commands.CopyAsMetafile.Should().BeEnabled();
                _workflow.Plots.Commands.ExportAsImage.Should().BeEnabled();
                _workflow.Plots.Commands.ExportAsPdf.Should().BeEnabled();
            } else {
                _workflow.Plots.Commands.RemoveAll.Should().BeDisabled();
                _workflow.Plots.Commands.RemoveCurrent.Should().BeDisabled();
                _workflow.Plots.Commands.CopyAsBitmap.Should().BeDisabled();
                _workflow.Plots.Commands.CopyAsMetafile.Should().BeDisabled();
                _workflow.Plots.Commands.ExportAsImage.Should().BeDisabled();
                _workflow.Plots.Commands.ExportAsPdf.Should().BeDisabled();
            }

            if (isFirst || !anyPlot) {
                _workflow.Plots.Commands.Previous.Should().BeDisabled();
            } else {
                _workflow.Plots.Commands.Previous.Should().BeEnabled();
            }

            if (isLast || !anyPlot) {
                _workflow.Plots.Commands.Next.Should().BeDisabled();
            } else {
                _workflow.Plots.Commands.Next.Should().BeEnabled();
            }
        }
    }
}
