// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using FluentAssertions;
using Microsoft.Common.Core;
using Microsoft.Common.Wpf.Imaging;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Components.Plots;
using Microsoft.R.Components.Plots.Implementation.ViewModel;
using Microsoft.R.Components.Plots.ViewModel;
using Microsoft.R.Components.Test.Fakes.InteractiveWindow;
using Microsoft.R.Components.Test.Fakes.Shell;
using Microsoft.R.Components.Test.Fakes.VisualComponentFactories;
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
        private readonly TestRPlotManagerVisualComponentContainerFactory _plotVisualComponentContainerFactory;
        private readonly MethodInfo _testMethod;
        private readonly TestFilesFixture _testFiles;

        public PlotIntegrationTest(RComponentsMefCatalogFixture catalog, TestMethodFixture testMethod, TestFilesFixture testFiles) {
            _exportProvider = catalog.CreateExportProvider();
            _workflowProvider = _exportProvider.GetExportedValue<TestRInteractiveWorkflowProvider>();
            _workflowProvider.BrokerName = nameof(PlotIntegrationTest);
            _plotVisualComponentContainerFactory = _exportProvider.GetExportedValue<TestRPlotManagerVisualComponentContainerFactory>();
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
                await InitializeGraphicsDevice();

                var viewModel = await GetActiveViewModelAsync();
                CheckEnabledCommands(viewModel, isFirst: false, isLast: false, anyPlot: false);
            }
        }

        [Test(ThreadType.UI)]
        [Category.Plots]
        public async Task SomeCommandsEnabledForSinglePlot() {
            _plotVisualComponentContainerFactory.DeviceProperties = new PlotDeviceProperties(600, 500, 96);
            using (await _workflow.GetOrCreateVisualComponent(_componentContainerFactory)) {
                var plot1to10 = await GetExpectedImageAsync("png", 600, 500, 96, "plot1-10", "plot(1:10)");

                await InitializeGraphicsDevice();
                await ExecuteAndWaitForPlotsAsync(new string[] {
                    "plot(1:10)",
                });

                var viewModel = await GetActiveViewModelAsync();
                viewModel.PlotImage.Should().HaveSamePixels(plot1to10);

                CheckEnabledCommands(viewModel, isFirst: true, isLast: true, anyPlot: true);
            }
        }

        [Test(ThreadType.UI)]
        [Category.Plots]
        public async Task SomeCommandsEnabledForLastPlot() {
            _plotVisualComponentContainerFactory.DeviceProperties = new PlotDeviceProperties(600, 500, 96);
            using (await _workflow.GetOrCreateVisualComponent(_componentContainerFactory)) {
                var plot10to20 = await GetExpectedImageAsync("png", 600, 500, 96, "plot10-20", "plot(10:20)");

                await InitializeGraphicsDevice();
                await ExecuteAndWaitForPlotsAsync(new string[] {
                    "plot(1:10)",
                    "plot(10:20)",
                });

                var viewModel = await GetActiveViewModelAsync();
                viewModel.PlotImage.Should().HaveSamePixels(plot10to20);

                CheckEnabledCommands(viewModel, isFirst: false, isLast: true, anyPlot: true);
            }
        }

        [Test(ThreadType.UI)]
        [Category.Plots]
        public async Task SomeCommandsEnabledForMiddlePlot() {
            _plotVisualComponentContainerFactory.DeviceProperties = new PlotDeviceProperties(600, 500, 96);
            using (await _workflow.GetOrCreateVisualComponent(_componentContainerFactory)) {
                var plot10to20 = await GetExpectedImageAsync("png", 600, 500, 96, "plot10-20", "plot(10:20)");

                await InitializeGraphicsDevice();
                await ExecuteAndWaitForPlotsAsync(new string[] {
                    "plot(1:10)",
                    "plot(10:20)",
                    "plot(20:30)",
                });

                var viewModel = await GetActiveViewModelAsync();

                var plotReceivedTask = EventTaskSources.IRPlotManager.PlotMessageReceived.Create(_workflow.Plots);
                await PlotDeviceCommandFactory.PreviousPlot(_workflow, viewModel).InvokeAsync();
                await plotReceivedTask;

                viewModel.PlotImage.Should().HaveSamePixels(plot10to20);

                CheckEnabledCommands(viewModel, isFirst: false, isLast: false, anyPlot: true);
            }
        }

        [Test(ThreadType.UI)]
        [Category.Plots]
        public async Task SomeCommandsEnabledForFirstPlot() {
            _plotVisualComponentContainerFactory.DeviceProperties = new PlotDeviceProperties(600, 500, 96);
            using (await _workflow.GetOrCreateVisualComponent(_componentContainerFactory)) {
                var plot1to10 = await GetExpectedImageAsync("png", 600, 500, 96, "plot1-10", "plot(1:10)");

                await InitializeGraphicsDevice();
                await ExecuteAndWaitForPlotsAsync(new string[] {
                    "plot(1:10)",
                    "plot(10:20)",
                });

                var viewModel = await GetActiveViewModelAsync();
                var plotReceivedTask = EventTaskSources.IRPlotManager.PlotMessageReceived.Create(_workflow.Plots);
                await PlotDeviceCommandFactory.PreviousPlot(_workflow, viewModel).InvokeAsync();
                await plotReceivedTask;

                viewModel.PlotImage.Should().HaveSamePixels(plot1to10);

                CheckEnabledCommands(viewModel, isFirst: true, isLast: false, anyPlot: true);
            }
        }

        [Test(ThreadType.UI)]
        [Category.Plots]
        public async Task DeviceCopyAsBitmap() {
            _plotVisualComponentContainerFactory.DeviceProperties = new PlotDeviceProperties(600, 500, 96);
            using (await _workflow.GetOrCreateVisualComponent(_componentContainerFactory)) {
                var plot1to10 = await GetExpectedImageAsync("bmp", 600, 500, 96, "plot1-10", "plot(1:10)");

                await InitializeGraphicsDevice();
                await ExecuteAndWaitForPlotsAsync(new string[] {
                    "plot(1:10)",
                });

                Clipboard.Clear();

                var viewModel = await GetActiveViewModelAsync();
                var cmd = PlotDeviceCommandFactory.CopyAsBitmap(_workflow, viewModel);
                cmd.Should().BeEnabled();
                await cmd.InvokeAsync();

                Clipboard.ContainsImage().Should().BeTrue();
                CoreShell.LastShownErrorMessage.Should().BeNullOrEmpty();

                var clipboardImage = Clipboard.GetImage();
                clipboardImage.Should().HaveSamePixels(plot1to10);
            }
        }

        [Test(ThreadType.UI)]
        [Category.Plots]
        public async Task DeviceCopyAsMetafile() {
            _plotVisualComponentContainerFactory.DeviceProperties = new PlotDeviceProperties(600, 500, 96);
            using (await _workflow.GetOrCreateVisualComponent(_componentContainerFactory)) {
                await InitializeGraphicsDevice();
                await ExecuteAndWaitForPlotsAsync(new string[] {
                    "plot(1:10)",
                });

                Clipboard.Clear();

                var viewModel = await GetActiveViewModelAsync();
                var cmd = PlotDeviceCommandFactory.CopyAsMetafile(_workflow, viewModel);
                cmd.Should().BeEnabled();
                await cmd.InvokeAsync();

                Clipboard.ContainsData(DataFormats.EnhancedMetafile).Should().BeTrue();
                CoreShell.LastShownErrorMessage.Should().BeNullOrEmpty();
            }
        }

        [Test(ThreadType.UI)]
        [Category.Plots]
        public async Task DeviceCopy() {
            _plotVisualComponentContainerFactory.DeviceProperties = new PlotDeviceProperties(600, 500, 96);
            using (await _workflow.GetOrCreateVisualComponent(_componentContainerFactory)) {
                var plot1to10 = await GetExpectedImageAsync("bmp", 600, 500, 96, "plot1-10", "plot(1:10)");
                var plot2to10 = await GetExpectedImageAsync("bmp", 600, 500, 96, "plot2-10", "plot(2:10)");

                await InitializeGraphicsDevice();
                await ExecuteAndWaitForPlotsAsync(new string[] {
                    "plot(1:10)",
                });

                var viewModel1 = await GetActiveViewModelAsync();
                var device1 = viewModel1.DeviceId;
                var plot1 = viewModel1.ActivePlotId;

                await InitializeGraphicsDevice();
                await ExecuteAndWaitForPlotsAsync(new string[] {
                    "plot(2:10)",
                });

                var viewModel2 = await GetActiveViewModelAsync();
                var device2 = viewModel2.DeviceId;
                var plot2 = viewModel2.ActivePlotId;

                var copyCmd = PlotDeviceCommandFactory.Copy(_workflow, viewModel1, cut: false);
                copyCmd.Should().BeEnabled();
                await copyCmd.InvokeAsync();

                CoreShell.LastShownErrorMessage.Should().BeNullOrEmpty();

                var pasteCmd = PlotDeviceCommandFactory.Paste(_workflow, viewModel2);
                pasteCmd.Should().BeEnabled();
                var plotReceivedTask = EventTaskSources.IRPlotManager.PlotMessageReceived.Create(_workflow.Plots);
                await pasteCmd.InvokeAsync();
                await plotReceivedTask;

                CoreShell.LastShownErrorMessage.Should().BeNullOrEmpty();

                viewModel2.PlotImage.Should().HaveSamePixels(plot1to10);
                _workflow.Plots.History.Entries.Count.Should().Be(3);
            }
        }

        [Test(ThreadType.UI)]
        [Category.Plots]
        public async Task DeviceCut() {
            _plotVisualComponentContainerFactory.DeviceProperties = new PlotDeviceProperties(600, 500, 96);
            using (await _workflow.GetOrCreateVisualComponent(_componentContainerFactory)) {
                var plot1to10 = await GetExpectedImageAsync("bmp", 600, 500, 96, "plot1-10", "plot(1:10)");
                var plot2to10 = await GetExpectedImageAsync("bmp", 600, 500, 96, "plot2-10", "plot(2:10)");

                await InitializeGraphicsDevice();
                await ExecuteAndWaitForPlotsAsync(new string[] {
                    "plot(1:10)",
                });

                var viewModel1 = await GetActiveViewModelAsync();
                var device1 = viewModel1.DeviceId;
                var plot1 = viewModel1.ActivePlotId;

                await InitializeGraphicsDevice();
                await ExecuteAndWaitForPlotsAsync(new string[] {
                    "plot(2:10)",
                });

                var viewModel2 = await GetActiveViewModelAsync();
                var device2 = viewModel2.DeviceId;
                var plot2 = viewModel2.ActivePlotId;

                var copyCmd = PlotDeviceCommandFactory.Copy(_workflow, viewModel1, cut: true);
                copyCmd.Should().BeEnabled();
                await copyCmd.InvokeAsync();

                CoreShell.LastShownErrorMessage.Should().BeNullOrEmpty();

                var pasteCmd = PlotDeviceCommandFactory.Paste(_workflow, viewModel2);
                pasteCmd.Should().BeEnabled();
                var plotReceivedTask = EventTaskSources.IRPlotManager.PlotMessageReceived.Create(_workflow.Plots);
                await pasteCmd.InvokeAsync();
                await plotReceivedTask;

                CoreShell.LastShownErrorMessage.Should().BeNullOrEmpty();

                viewModel1.PlotImage.Should().BeNull();
                viewModel2.PlotImage.Should().HaveSamePixels(plot1to10);

                _workflow.Plots.History.Entries.Count.Should().Be(2);
            }
        }

        [Test(ThreadType.UI)]
        [Category.Plots]
        public async Task DeviceRemoveAll() {
            _plotVisualComponentContainerFactory.DeviceProperties = new PlotDeviceProperties(600, 500, 96);
            using (await _workflow.GetOrCreateVisualComponent(_componentContainerFactory)) {
                await InitializeGraphicsDevice();
                await ExecuteAndWaitForPlotsAsync(new string[] {
                    "plot(1:10)",
                    "plot(1:20)",
                });

                var viewModel1 = await GetActiveViewModelAsync();
                var device1 = viewModel1.DeviceId;
                var plot1 = viewModel1.ActivePlotId;

                await InitializeGraphicsDevice();
                await ExecuteAndWaitForPlotsAsync(new string[] {
                    "plot(2:10)",
                });

                var viewModel2 = await GetActiveViewModelAsync();
                var device2 = viewModel2.DeviceId;
                var plot2 = viewModel2.ActivePlotId;

                var removeAllCmd = PlotDeviceCommandFactory.RemoveAllPlots(_workflow, viewModel1);
                removeAllCmd.Should().BeEnabled();
                var plotReceivedTask = EventTaskSources.IRPlotManager.PlotMessageReceived.Create(_workflow.Plots);
                await removeAllCmd.InvokeAsync();
                await plotReceivedTask;

                CoreShell.LastShownErrorMessage.Should().BeNullOrEmpty();

                viewModel1.PlotImage.Should().BeNull();
                viewModel2.PlotImage.Should().NotBeNull();

                _workflow.Plots.History.Entries.Should().HaveCount(1);
                _workflow.Plots.History.Entries[0].PlotId.Should().Be(plot2);
            }
        }

        [Test(ThreadType.UI)]
        [Category.Plots]
        public async Task DeviceRemoveCurrent() {
            _plotVisualComponentContainerFactory.DeviceProperties = new PlotDeviceProperties(600, 500, 96);
            using (await _workflow.GetOrCreateVisualComponent(_componentContainerFactory)) {
                var plot1to10 = await GetExpectedImageAsync("bmp", 600, 500, 96, "plot1-10", "plot(1:10)");

                await InitializeGraphicsDevice();
                await ExecuteAndWaitForPlotsAsync(new string[] {
                    "plot(1:10)",
                    "plot(1:20)",
                });

                var viewModel1 = await GetActiveViewModelAsync();
                var device1 = viewModel1.DeviceId;
                var plot1 = viewModel1.ActivePlotId;

                await InitializeGraphicsDevice();
                await ExecuteAndWaitForPlotsAsync(new string[] {
                    "plot(2:10)",
                });

                var viewModel2 = await GetActiveViewModelAsync();
                var device2 = viewModel2.DeviceId;
                var plot2 = viewModel2.ActivePlotId;

                var removeAllCmd = PlotDeviceCommandFactory.RemoveCurrentPlot(_workflow, viewModel1);
                removeAllCmd.Should().BeEnabled();
                var plotReceivedTask = EventTaskSources.IRPlotManager.PlotMessageReceived.Create(_workflow.Plots);
                await removeAllCmd.InvokeAsync();
                await plotReceivedTask;

                CoreShell.LastShownErrorMessage.Should().BeNullOrEmpty();

                viewModel1.PlotImage.Should().HaveSamePixels(plot1to10);
                viewModel2.PlotImage.Should().NotBeNull();

                _workflow.Plots.History.Entries.Should().HaveCount(2);
                _workflow.Plots.History.Entries.Should().NotContain(e => e.PlotId == plot1);
            }
        }

        [Test(ThreadType.UI)]
        [Category.Plots]
        public async Task NewDevice() {
            using (await _workflow.GetOrCreateVisualComponent(_componentContainerFactory)) {
                var newCmd = PlotDeviceCommandFactory.NewPlotDevice(_workflow);
                newCmd.Should().BeEnabled();

                _workflow.Plots.ActiveDeviceId.Should().BeEmpty();

                var deviceChangedTask = EventTaskSources.IRPlotManager.ActiveDeviceChanged.Create(_workflow.Plots);
                await newCmd.InvokeAsync();
                await deviceChangedTask;

                _workflow.Plots.ActiveDeviceId.Should().NotBeEmpty();

                var eval = _workflow.ActiveWindow.InteractiveWindow.Evaluator;
                deviceChangedTask = EventTaskSources.IRPlotManager.ActiveDeviceChanged.Create(_workflow.Plots);
                await eval.ExecuteCodeAsync("dev.off()".EnsureLineBreak());
                await deviceChangedTask;

                _workflow.Plots.ActiveDeviceId.Should().BeEmpty();
            }
        }

        [Test(ThreadType.UI)]
        [Category.Plots]
        public async Task ActivateDevice() {
            using (await _workflow.GetOrCreateVisualComponent(_componentContainerFactory)) {
                await InitializeGraphicsDevice();
                var viewModel1 = await GetActiveViewModelAsync();
                var activateDevice1Cmd = PlotDeviceCommandFactory.ActivatePlotDevice(_workflow, viewModel1);
                activateDevice1Cmd.Should().BeChecked();
                activateDevice1Cmd.Should().BeEnabled();
                _workflow.Plots.ActiveDeviceId.Should().Be(viewModel1.DeviceId);

                await InitializeGraphicsDevice();
                var viewModel2 = await GetActiveViewModelAsync();
                var activateDevice2Cmd = PlotDeviceCommandFactory.ActivatePlotDevice(_workflow, viewModel2);
                activateDevice1Cmd.Should().BeUnchecked();
                activateDevice1Cmd.Should().BeEnabled();
                activateDevice2Cmd.Should().BeChecked();
                activateDevice2Cmd.Should().BeEnabled();
                _workflow.Plots.ActiveDeviceId.Should().Be(viewModel2.DeviceId);

                var deviceChangedTask = EventTaskSources.IRPlotManager.ActiveDeviceChanged.Create(_workflow.Plots);
                await activateDevice1Cmd.InvokeAsync();
                await deviceChangedTask;

                _workflow.Plots.ActiveDeviceId.Should().Be(viewModel1.DeviceId);
                activateDevice1Cmd.Should().BeChecked();
                activateDevice2Cmd.Should().BeUnchecked();

                deviceChangedTask = EventTaskSources.IRPlotManager.ActiveDeviceChanged.Create(_workflow.Plots);
                await activateDevice2Cmd.InvokeAsync();
                await deviceChangedTask;

                _workflow.Plots.ActiveDeviceId.Should().Be(viewModel2.DeviceId);
                activateDevice1Cmd.Should().BeUnchecked();
                activateDevice2Cmd.Should().BeChecked();
            }
        }

        [Test(ThreadType.UI)]
        [Category.Plots]
        public async Task ExportAsPdf() {
            _plotVisualComponentContainerFactory.DeviceProperties = new PlotDeviceProperties(600, 500, 96);
            using (await _workflow.GetOrCreateVisualComponent(_componentContainerFactory)) {
                await InitializeGraphicsDevice();
                await ExecuteAndWaitForPlotsAsync(new string[] {
                    "plot(1:10)",
                });

                var outputFilePath = _testFiles.GetDestinationPath("ExportedPlot.pdf");
                CoreShell.SaveFilePath = outputFilePath;

                var viewModel = await GetActiveViewModelAsync();
                var cmd = PlotDeviceCommandFactory.ExportAsPdf(_workflow, viewModel);
                cmd.Should().BeEnabled();
                await cmd.InvokeAsync();

                File.Exists(outputFilePath).Should().BeTrue();
                CoreShell.LastShownErrorMessage.Should().BeNullOrEmpty();
            }
        }

        [Test(ThreadType.UI)]
        [Category.Plots]
        public async Task ExportAsImage() {
            _plotVisualComponentContainerFactory.DeviceProperties = new PlotDeviceProperties(600, 500, 96);
            using (await _workflow.GetOrCreateVisualComponent(_componentContainerFactory)) {
                await InitializeGraphicsDevice();
                await ExecuteAndWaitForPlotsAsync(new string[] {
                    "plot(1:10)",
                });

                foreach (var ext in new string[] { "bmp", "jpg", "jpeg", "png", "tif", "tiff" }) {
                    var outputFilePath = _testFiles.GetDestinationPath("ExportedPlot." + ext);
                    CoreShell.SaveFilePath = outputFilePath;

                    var viewModel = await GetActiveViewModelAsync();
                    var cmd = PlotDeviceCommandFactory.ExportAsImage(_workflow, viewModel);
                    cmd.Should().BeEnabled();
                    await cmd.InvokeAsync();

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
            _plotVisualComponentContainerFactory.DeviceProperties = new PlotDeviceProperties(600, 500, 96);
            using (await _workflow.GetOrCreateVisualComponent(_componentContainerFactory)) {
                await InitializeGraphicsDevice();
                await ExecuteAndWaitForPlotsAsync(new string[] {
                    "plot(1:10)",
                });

                // The file extension of the file the user selected in the save
                // dialog is what determines the image format. When it's an
                // unsupported format, we show an error msg.
                var outputFilePath = _testFiles.GetDestinationPath("ExportedPlot.unsupportedextension");
                CoreShell.SaveFilePath = outputFilePath;

                var viewModel = await GetActiveViewModelAsync();
                var cmd = PlotDeviceCommandFactory.ExportAsImage(_workflow, viewModel);
                cmd.Should().BeEnabled();
                await cmd.InvokeAsync();

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

            _plotVisualComponentContainerFactory.DeviceProperties = new PlotDeviceProperties(600, 500, 96);
            using (await _workflow.GetOrCreateVisualComponent(_componentContainerFactory)) {
                await InitializeGraphicsDevice();
                await ExecuteAndWaitForPlotsAsync(scripts);

                var viewModel = await GetActiveViewModelAsync();
                var prevCmd = PlotDeviceCommandFactory.PreviousPlot(_workflow, viewModel);
                var nextCmd = PlotDeviceCommandFactory.NextPlot(_workflow, viewModel);

                for (int i = 0; i < scripts.Length - 1; i++) {
                    prevCmd.Should().BeEnabled();
                    var plotReceivedTask = EventTaskSources.IRPlotManager.PlotMessageReceived.Create(_workflow.Plots);
                    await prevCmd.InvokeAsync();
                    await plotReceivedTask;
                }

                prevCmd.Should().BeDisabled();

                for (int i = 0; i < scripts.Length - 1; i++) {
                    nextCmd.Should().BeEnabled();
                    var plotReceivedTask = EventTaskSources.IRPlotManager.PlotMessageReceived.Create(_workflow.Plots);
                    await nextCmd.InvokeAsync();
                    await plotReceivedTask;
                }

                nextCmd.Should().BeDisabled();
            }
        }

        [Test(ThreadType.UI)]
        [Category.Plots]
        public async Task PlotError() {
            _plotVisualComponentContainerFactory.DeviceProperties = new PlotDeviceProperties(600, 500, 96);
            using (await _workflow.GetOrCreateVisualComponent(_componentContainerFactory)) {
                await InitializeGraphicsDevice();
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

                var viewModel = await GetActiveViewModelAsync();
                var prevCmd = PlotDeviceCommandFactory.PreviousPlot(_workflow, viewModel);

                // Navigating to a plot in the history that cannot be rendered
                // will send a plot with zero-file size, which translate into an error message.
                prevCmd.Should().BeEnabled();
                var plotReceivedTask = EventTaskSources.IRPlotManager.PlotMessageReceived.Create(_workflow.Plots);
                await prevCmd.InvokeAsync();
                await plotReceivedTask;

                viewModel.PlotImage.Should().BeNull();
                viewModel.ShowError.Should().BeTrue();
                viewModel.ShowWatermark.Should().BeFalse();

                prevCmd.Should().BeEnabled();
                plotReceivedTask = EventTaskSources.IRPlotManager.PlotMessageReceived.Create(_workflow.Plots);
                await prevCmd.InvokeAsync();
                await plotReceivedTask;

                viewModel.PlotImage.Should().NotBeNull();
                viewModel.ShowError.Should().BeFalse();
                viewModel.ShowWatermark.Should().BeFalse();

                prevCmd.Should().BeDisabled();
            }
        }

        [Test(ThreadType.UI)]
        [Category.Plots]
        public async Task LocatorCommand() {
            _plotVisualComponentContainerFactory.DeviceProperties = new PlotDeviceProperties(360, 360, 96);
            using (await _workflow.GetOrCreateVisualComponent(_componentContainerFactory)) {
                await InitializeGraphicsDevice();

                await ExecuteAndWaitForPlotsAsync(new string[] {
                    "plot(0:10)",
                });

                var viewModel = await GetActiveViewModelAsync();
                viewModel.LocatorMode.Should().BeFalse();

                var cmd = PlotDeviceCommandFactory.EndLocator(_workflow, viewModel);
                cmd.Should().BeInvisibleAndDisabled();

                var firstLocatorModeTask = EventTaskSources.IRPlotManager.LocatorModeChanged.Create(_workflow.Plots);
                ExecuteAndDoNotWaitForPlotsAsync(new string[] {
                    "res <- locator()",
                }).DoNotWait();
                await firstLocatorModeTask;

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
                    viewModel.LocatorMode.Should().BeTrue();
                    cmd.Should().BeEnabled();

                    // Send a result with a click point, which will causes
                    // locator mode to end and immediately start again
                    var locatorModeTask = EventTaskSources.IRPlotManager.LocatorModeChanged.Create(_workflow.Plots);
                    viewModel.ClickPlot((int)point.X, (int)point.Y);
                    await locatorModeTask;

                    viewModel.LocatorMode.Should().BeFalse();
                    cmd.Should().BeInvisibleAndDisabled();

                    locatorModeTask = EventTaskSources.IRPlotManager.LocatorModeChanged.Create(_workflow.Plots);
                    await locatorModeTask;
                }

                // Send a result with a not clicked result, which causes
                // locator mode to end, and the high-level locator() function
                // call will return.
                var lastLocatorModeTask = EventTaskSources.IRPlotManager.LocatorModeChanged.Create(_workflow.Plots);
                await cmd.InvokeAsync();
                await lastLocatorModeTask;

                viewModel.LocatorMode.Should().BeFalse();
                cmd.Should().BeInvisibleAndDisabled();

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
                await InitializeGraphicsDevice();

                await ExecuteAndWaitForPlotsAsync(new string[] {
                    "plot(0:10)",
                });

                var viewModel = await GetActiveViewModelAsync();
                viewModel.LocatorMode.Should().BeFalse();

                var cmd = PlotDeviceCommandFactory.EndLocator(_workflow, viewModel);
                cmd.Should().BeInvisibleAndDisabled();

                var locatorModeTask = EventTaskSources.IRPlotManager.LocatorModeChanged.Create(_workflow.Plots);
                ExecuteAndDoNotWaitForPlotsAsync(new string[] {
                    "res <- locator()",
                }).DoNotWait();
                await locatorModeTask;

                viewModel.LocatorMode.Should().BeTrue();
                cmd.Should().BeEnabled();

                locatorModeTask = EventTaskSources.IRPlotManager.LocatorModeChanged.Create(_workflow.Plots);
                await cmd.InvokeAsync();
                await locatorModeTask;

                viewModel.LocatorMode.Should().BeFalse();
                cmd.Should().BeInvisibleAndDisabled();

                string outputFilePath = _testFiles.GetDestinationPath("LocatorResultNoClick.csv");
                await ExecuteAndDoNotWaitForPlotsAsync(new string[] {
                    string.Format("write.csv(res, {0})", outputFilePath.ToRPath().ToRStringLiteral())
                });

                string output = File.ReadAllText(outputFilePath);
                output.Trim().Should().Be("\"\"");
            }
        }

        [Test(ThreadType.UI)]
        [Category.Plots]
        public async Task HistoryActivate() {
            _plotVisualComponentContainerFactory.DeviceProperties = new PlotDeviceProperties(600, 500, 96);
            using (await _workflow.GetOrCreateVisualComponent(_componentContainerFactory)) {
                await InitializeGraphicsDevice();
                await ExecuteAndWaitForPlotsAsync(new string[] {
                    "plot(1:10)",
                });

                await ExecuteAndWaitForPlotsAsync(new string[] {
                    "plot(2:10)",
                });

                var viewModel = await GetActiveViewModelAsync();

                var activateCmd = PlotHistoryCommandFactory.ActivatePlot(_workflow);
                _workflow.Plots.History.SelectedPlot = null;
                activateCmd.Should().BeDisabled();

                foreach (var plot in _workflow.Plots.History.Entries) {
                    _workflow.Plots.History.SelectedPlot = plot;
                    activateCmd.Should().BeEnabled();
                    var plotReceivedTask = EventTaskSources.IRPlotManager.PlotMessageReceived.Create(_workflow.Plots);
                    await activateCmd.InvokeAsync();
                    await plotReceivedTask;
                    viewModel.ActivePlotId.Should().Be(plot.PlotId);
                }

                CoreShell.LastShownErrorMessage.Should().BeNullOrEmpty();
            }
        }

        [Test(ThreadType.UI)]
        [Category.Plots]
        public async Task HistoryCopy() {
            _plotVisualComponentContainerFactory.DeviceProperties = new PlotDeviceProperties(600, 500, 96);
            using (await _workflow.GetOrCreateVisualComponent(_componentContainerFactory)) {
                await InitializeGraphicsDevice();
                await ExecuteAndWaitForPlotsAsync(new string[] {
                    "plot(1:10)",
                });

                var viewModel1 = await GetActiveViewModelAsync();
                var device1 = viewModel1.DeviceId;
                var plot1 = viewModel1.ActivePlotId;

                await InitializeGraphicsDevice();
                await ExecuteAndWaitForPlotsAsync(new string[] {
                    "plot(2:10)",
                });

                var viewModel2 = await GetActiveViewModelAsync();
                var device2 = viewModel2.DeviceId;
                var plot2 = viewModel2.ActivePlotId;

                _workflow.Plots.History.SelectedPlot = _workflow.Plots.History.Entries.Single(e => e.PlotId == plot1);
                var copyCmd = PlotHistoryCommandFactory.Copy(_workflow, cut: false);
                copyCmd.Should().BeEnabled();
                await copyCmd.InvokeAsync();

                CoreShell.LastShownErrorMessage.Should().BeNullOrEmpty();

                var pasteCmd = PlotDeviceCommandFactory.Paste(_workflow, viewModel2);
                pasteCmd.Should().BeEnabled();
                var plotReceivedTask = EventTaskSources.IRPlotManager.PlotMessageReceived.Create(_workflow.Plots);
                await pasteCmd.InvokeAsync();
                await plotReceivedTask;

                CoreShell.LastShownErrorMessage.Should().BeNullOrEmpty();

                viewModel2.PlotImage.Should().HaveSamePixels(viewModel1.PlotImage);
                _workflow.Plots.History.Entries.Count.Should().Be(3);
            }
        }

        [Test(ThreadType.UI)]
        [Category.Plots]
        public async Task HistoryRemove() {
            _plotVisualComponentContainerFactory.DeviceProperties = new PlotDeviceProperties(600, 500, 96);
            using (await _workflow.GetOrCreateVisualComponent(_componentContainerFactory)) {
                await InitializeGraphicsDevice();
                await ExecuteAndWaitForPlotsAsync(new string[] {
                    "plot(1:10)",
                });

                var viewModel1 = await GetActiveViewModelAsync();
                var device1 = viewModel1.DeviceId;
                var plot1 = viewModel1.ActivePlotId;

                await InitializeGraphicsDevice();
                await ExecuteAndWaitForPlotsAsync(new string[] {
                    "plot(2:10)",
                });

                var viewModel2 = await GetActiveViewModelAsync();
                var device2 = viewModel2.DeviceId;
                var plot2 = viewModel2.ActivePlotId;

                var removeCmd = PlotHistoryCommandFactory.Remove(_workflow);
                _workflow.Plots.History.SelectedPlot = null;
                removeCmd.Should().BeDisabled();

                _workflow.Plots.History.SelectedPlot = _workflow.Plots.History.Entries.Single(e => e.PlotId == plot1);
                removeCmd.Should().BeEnabled();
                var plotReceivedTask = EventTaskSources.IRPlotManager.PlotMessageReceived.Create(_workflow.Plots);
                await removeCmd.InvokeAsync();
                await plotReceivedTask;

                viewModel1.PlotImage.Should().BeNull();
                _workflow.Plots.History.Entries.Should().HaveCount(1);
                _workflow.Plots.History.Entries[0].PlotId.Should().Be(plot2);

                _workflow.Plots.History.SelectedPlot = _workflow.Plots.History.Entries.Single(e => e.PlotId == plot2);
                removeCmd.Should().BeEnabled();
                plotReceivedTask = EventTaskSources.IRPlotManager.PlotMessageReceived.Create(_workflow.Plots);
                await removeCmd.InvokeAsync();
                await plotReceivedTask;

                viewModel2.PlotImage.Should().BeNull();
                _workflow.Plots.History.Entries.Should().BeEmpty();
            }
        }

        [Test(ThreadType.UI)]
        [Category.Plots]
        public async Task HistoryZoom() {
            _plotVisualComponentContainerFactory.DeviceProperties = new PlotDeviceProperties(600, 500, 96);
            using (await _workflow.GetOrCreateVisualComponent(_componentContainerFactory)) {
                var zoomInCmd = PlotHistoryCommandFactory.ZoomIn(_workflow);
                var zoomOutCmd = PlotHistoryCommandFactory.ZoomOut(_workflow);

                _workflow.Plots.History.ThumbnailSize = 96;
                zoomOutCmd.Should().BeEnabled();
                await zoomOutCmd.InvokeAsync();
                _workflow.Plots.History.ThumbnailSize.Should().Be(48);

                _workflow.Plots.History.ThumbnailSize = 432;
                zoomInCmd.Should().BeEnabled();
                await zoomInCmd.InvokeAsync();
                _workflow.Plots.History.ThumbnailSize.Should().Be(480);

                _workflow.Plots.History.ThumbnailSize = RPlotHistoryViewModel.MinThumbnailSize;
                zoomInCmd.Should().BeEnabled();
                zoomOutCmd.Should().BeDisabled();

                _workflow.Plots.History.ThumbnailSize = RPlotHistoryViewModel.MaxThumbnailSize;
                zoomInCmd.Should().BeDisabled();
                zoomOutCmd.Should().BeEnabled();
            }
        }

        [Test(ThreadType.UI)]
        [Category.Plots]
        public async Task HistoryAutoHide() {
            _plotVisualComponentContainerFactory.DeviceProperties = new PlotDeviceProperties(600, 500, 96);
            using (await _workflow.GetOrCreateVisualComponent(_componentContainerFactory)) {
                var autoHideCmd = PlotHistoryCommandFactory.AutoHide(_workflow);

                _workflow.Plots.History.AutoHide = false;
                autoHideCmd.Should().BeVisibleAndEnabled();
                autoHideCmd.Should().BeUnchecked();

                await autoHideCmd.InvokeAsync();
                _workflow.Plots.History.AutoHide.Should().BeTrue();
                autoHideCmd.Should().BeVisibleAndEnabled();
                autoHideCmd.Should().BeChecked();

                await autoHideCmd.InvokeAsync();
                _workflow.Plots.History.AutoHide.Should().BeFalse();
                autoHideCmd.Should().BeVisibleAndEnabled();
                autoHideCmd.Should().BeUnchecked();
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

        private async Task InitializeGraphicsDevice() {
            var deviceCreatedTask = EventTaskSources.IRPlotManager.DeviceCreateMessageReceived.Create(_workflow.Plots);
            var deviceChangedTask = EventTaskSources.IRPlotManager.ActiveDeviceChanged.Create(_workflow.Plots);

            var eval = _workflow.ActiveWindow.InteractiveWindow.Evaluator;
            var result = await eval.ExecuteCodeAsync("dev.new()\n");
            result.IsSuccessful.Should().BeTrue();

            await deviceCreatedTask;
            await deviceChangedTask;
        }

        private async Task ExecuteAndWaitForPlotsAsync(string[] scripts) {
            var eval = _workflow.ActiveWindow.InteractiveWindow.Evaluator;

            foreach (string script in scripts) {
                var plotReceivedTask = EventTaskSources.IRPlotManager.PlotMessageReceived.Create(_workflow.Plots);

                var result = await eval.ExecuteCodeAsync(script.EnsureLineBreak());
                result.IsSuccessful.Should().BeTrue();

                await plotReceivedTask;
            }
        }

        private async Task ExecuteAndDoNotWaitForPlotsAsync(string[] scripts) {
            var eval = _workflow.ActiveWindow.InteractiveWindow.Evaluator;
            foreach (string script in scripts) {
                var result = await eval.ExecuteCodeAsync(script.EnsureLineBreak());
                result.IsSuccessful.Should().BeTrue();
            }
        }

        private Task<IRPlotDeviceViewModel> GetActiveViewModelAsync() {
            return Task.FromResult(_workflow.Plots.GetDeviceViewModel(_workflow.Plots.ActiveDeviceId));
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

        private void CheckEnabledCommands(IRPlotDeviceViewModel viewModel, bool isFirst, bool isLast, bool anyPlot) {
            PlotDeviceCommandFactory.EndLocator(_workflow, viewModel).Should().BeDisabled();

            if (anyPlot) {
                PlotDeviceCommandFactory.RemoveAllPlots(_workflow, viewModel).Should().BeEnabled();
                PlotDeviceCommandFactory.RemoveCurrentPlot(_workflow, viewModel).Should().BeEnabled();
                PlotDeviceCommandFactory.Copy(_workflow, viewModel, cut: false).Should().BeEnabled();
                PlotDeviceCommandFactory.Copy(_workflow, viewModel, cut: true).Should().BeEnabled();
                PlotDeviceCommandFactory.CopyAsBitmap(_workflow, viewModel).Should().BeEnabled();
                PlotDeviceCommandFactory.CopyAsMetafile(_workflow, viewModel).Should().BeEnabled();
                PlotDeviceCommandFactory.ExportAsImage(_workflow, viewModel).Should().BeEnabled();
                PlotDeviceCommandFactory.ExportAsPdf(_workflow, viewModel).Should().BeEnabled();
            } else {
                PlotDeviceCommandFactory.RemoveAllPlots(_workflow, viewModel).Should().BeDisabled();
                PlotDeviceCommandFactory.RemoveCurrentPlot(_workflow, viewModel).Should().BeDisabled();
                PlotDeviceCommandFactory.Copy(_workflow, viewModel, cut: false).Should().BeDisabled();
                PlotDeviceCommandFactory.Copy(_workflow, viewModel, cut: true).Should().BeDisabled();
                PlotDeviceCommandFactory.CopyAsBitmap(_workflow, viewModel).Should().BeDisabled();
                PlotDeviceCommandFactory.CopyAsMetafile(_workflow, viewModel).Should().BeDisabled();
                PlotDeviceCommandFactory.ExportAsImage(_workflow, viewModel).Should().BeDisabled();
                PlotDeviceCommandFactory.ExportAsPdf(_workflow, viewModel).Should().BeDisabled();
            }

            if (isFirst || !anyPlot) {
                PlotDeviceCommandFactory.PreviousPlot(_workflow, viewModel).Should().BeDisabled();
            } else {
                PlotDeviceCommandFactory.PreviousPlot(_workflow, viewModel).Should().BeEnabled();
            }

            if (isLast || !anyPlot) {
                PlotDeviceCommandFactory.NextPlot(_workflow, viewModel).Should().BeDisabled();
            } else {
                PlotDeviceCommandFactory.NextPlot(_workflow, viewModel).Should().BeEnabled();
            }
        }
    }
}
