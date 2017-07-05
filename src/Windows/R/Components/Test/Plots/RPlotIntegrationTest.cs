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
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.Test.Fakes.Shell;
using Microsoft.Common.Wpf.Imaging;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Components.Plots;
using Microsoft.R.Components.Plots.Commands;
using Microsoft.R.Components.Test.Fakes.InteractiveWindow;
using Microsoft.R.Components.Test.Fakes.VisualComponentFactories;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Test.Fixtures;
using Microsoft.UnitTests.Core.FluentAssertions;
using Microsoft.UnitTests.Core.Threading;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.UnitTests.Core.XUnit.MethodFixtures;
using Xunit;

namespace Microsoft.R.Components.Test.Plots {
    [ExcludeFromCodeCoverage]
    [Category.Plots]
    public class RPlotIntegrationTest : IAsyncLifetime {
        private readonly IRInteractiveWorkflowVisual _workflow;
        private readonly TestRPlotDeviceVisualComponentContainerFactory _plotDeviceVisualComponentContainerFactory;
        private readonly IRPlotHistoryVisualComponentContainerFactory _plotHistoryVisualComponentContainerFactory;
        private readonly MethodInfo _testMethod;
        private readonly IRemoteBroker _remoteBroker;
        private readonly TestFilesFixture _testFiles;
        private readonly TestUIServices _ui;
        private IInteractiveWindowVisualComponent _replVisualComponent;
        private IRPlotManagerVisual _plotManager;

        public RPlotIntegrationTest(IServiceContainer services, IRemoteBroker remoteBroker, TestMethodFixture testMethod, TestFilesFixture testFiles) {
            _workflow = services.GetService<IRInteractiveWorkflowVisualProvider>().GetOrCreate();
            _plotDeviceVisualComponentContainerFactory = services.GetService<TestRPlotDeviceVisualComponentContainerFactory>();
            _plotHistoryVisualComponentContainerFactory = services.GetService<IRPlotHistoryVisualComponentContainerFactory>();
            _testMethod = testMethod.MethodInfo;
            _remoteBroker = remoteBroker;
            _testFiles = testFiles;
            _ui = _workflow.Shell.UI() as TestUIServices;
        }

        public async Task InitializeAsync() {
            await _remoteBroker.ConnectAsync(_workflow.RSessions);
            _replVisualComponent = await _workflow.GetOrCreateVisualComponentAsync();
            _plotManager = (IRPlotManagerVisual)_workflow.Plots;
            _plotDeviceVisualComponentContainerFactory.DeviceProperties = new PlotDeviceProperties(600, 500, 96);
        }

        public Task DisposeAsync() {
            _replVisualComponent.Dispose();
            return Task.CompletedTask;
        }

        private TestFileDialog FileDialog => _workflow.Shell.FileDialog() as TestFileDialog;

        [Test(ThreadType.UI)]
        public async Task AllCommandsDisabledWhenNoPlot() {
            await InitializeGraphicsDevice();

            var plotVC = _plotManager.GetPlotVisualComponent(_plotManager.ActiveDevice);
            CheckEnabledCommands(plotVC, isFirst: false, isLast: false, anyPlot: false);
        }

        [Test(ThreadType.UI)]
        public async Task SomeCommandsEnabledForSinglePlot() {
            var plot1to10 = await GetExpectedImageAsync("png", 600, 500, 96, "plot1-10", "plot(1:10)");

            await InitializeGraphicsDevice();
            await ExecuteAndWaitForPlotsAsync(new string[] {
                "plot(1:10)",
            });

            var plotVC = _plotManager.GetPlotVisualComponent(_plotManager.ActiveDevice);
            var bs = _plotManager.ActiveDevice.ActivePlot.Image as BitmapSource;
            bs.Should().HaveSamePixels(plot1to10);

            CheckEnabledCommands(plotVC, isFirst: true, isLast: true, anyPlot: true);
        }

        [Test(ThreadType.UI)]
        public async Task SomeCommandsEnabledForLastPlot() {
            var plot10to20 = await GetExpectedImageAsync("png", 600, 500, 96, "plot10-20", "plot(10:20)");

            await InitializeGraphicsDevice();
            await ExecuteAndWaitForPlotsAsync(new string[] {
                "plot(1:10)",
                "plot(10:20)",
            });

            var plotVC = _plotManager.GetPlotVisualComponent(_plotManager.ActiveDevice);
            var bs = _plotManager.ActiveDevice.ActivePlot.Image as BitmapSource;
            bs.Should().HaveSamePixels(plot10to20);

            CheckEnabledCommands(plotVC, isFirst: false, isLast: true, anyPlot: true);
        }

        [Test(ThreadType.UI)]
        public async Task SomeCommandsEnabledForMiddlePlot() {
            var plot10to20 = await GetExpectedImageAsync("png", 600, 500, 96, "plot10-20", "plot(10:20)");

            await InitializeGraphicsDevice();
            await ExecuteAndWaitForPlotsAsync(new string[] {
                "plot(1:10)",
                "plot(10:20)",
                "plot(20:30)",
            });

            var deviceVC = _plotManager.GetPlotVisualComponent(_plotManager.ActiveDevice);
            var deviceCommands = new RPlotDeviceCommands(_workflow, deviceVC);

            var plotReceivedTask = EventTaskSources.IRPlotDevice.PlotAddedOrUpdated.Create(_plotManager.ActiveDevice);
            await deviceCommands.PreviousPlot.InvokeAsync();
            await plotReceivedTask;

            var bs = _plotManager.ActiveDevice.ActivePlot.Image as BitmapSource;
            bs.Should().HaveSamePixels(plot10to20);

            CheckEnabledCommands(deviceVC, isFirst: false, isLast: false, anyPlot: true);
        }

        [Test(ThreadType.UI)]
        public async Task SomeCommandsEnabledForFirstPlot() {
            var plot1to10 = await GetExpectedImageAsync("png", 600, 500, 96, "plot1-10", "plot(1:10)");

            await InitializeGraphicsDevice();
            await ExecuteAndWaitForPlotsAsync(new string[] {
                "plot(1:10)",
                "plot(10:20)",
            });

            var deviceVC = _plotManager.GetPlotVisualComponent(_plotManager.ActiveDevice);
            var deviceCommands = new RPlotDeviceCommands(_workflow, deviceVC);

            var plotReceivedTask = EventTaskSources.IRPlotDevice.PlotAddedOrUpdated.Create(_plotManager.ActiveDevice);
            await deviceCommands.PreviousPlot.InvokeAsync();
            await plotReceivedTask;

            var bs = _plotManager.ActiveDevice.ActivePlot.Image as BitmapSource;
            bs.Should().HaveSamePixels(plot1to10);

            CheckEnabledCommands(deviceVC, isFirst: true, isLast: false, anyPlot: true);
        }

        [Test(ThreadType.UI)]
        public async Task DeviceCopyAsBitmap() {
            var plot1to10 = await GetExpectedImageAsync("bmp", 600, 500, 96, "plot1-10", "plot(1:10)");

            await InitializeGraphicsDevice();
            await ExecuteAndWaitForPlotsAsync(new string[] {
                "plot(1:10)",
            });

            var device1 = _plotManager.ActiveDevice;
            var device1VC = _plotManager.GetPlotVisualComponent(device1);
            var device1Commands = new RPlotDeviceCommands(_workflow, device1VC);
            var plot1 = device1.ActivePlot;

            await InitializeGraphicsDevice();
            await ExecuteAndWaitForPlotsAsync(new string[] {
                "plot(2:10)",
            });

            var device2 = _plotManager.ActiveDevice;

            Clipboard.Clear();

            var deviceCommands = new RPlotDeviceCommands(_workflow, device1VC);

            deviceCommands.CopyAsBitmap.Should().BeEnabled();
            await deviceCommands.CopyAsBitmap.InvokeAsync();

            // Exporting plot from device 1 should not have changed the active device
            _plotManager.ActiveDevice.Should().Be(device2);

            Clipboard.ContainsImage().Should().BeTrue();
            _ui.LastShownErrorMessage.Should().BeNullOrEmpty();

            var clipboardImage = Clipboard.GetImage();
            clipboardImage.Should().HaveSamePixels(plot1to10);
        }

        [Test(ThreadType.UI)]
        public async Task DeviceResize() {
            var plot1to10 = await GetExpectedImageAsync("bmp", 600, 500, 96, "plot1-10", "plot(1:10)");
            var plot1to10larger = await GetExpectedImageAsync("bmp", 650, 550, 96, "plot1-10larger", "plot(1:10)");

            await InitializeGraphicsDevice();
            await ExecuteAndWaitForPlotsAsync(new string[] {
                "plot(1:10)",
            });

            var device1 = _plotManager.ActiveDevice;
            var device1VC = _plotManager.GetPlotVisualComponent(device1);
            var device1Commands = new RPlotDeviceCommands(_workflow, device1VC);
            var plot1 = device1.ActivePlot;

            await InitializeGraphicsDevice();
            await ExecuteAndWaitForPlotsAsync(new string[] {
                "plot(2:10)",
            });

            var device2 = _plotManager.ActiveDevice;

            var plotReceivedTask = EventTaskSources.IRPlotDevice.PlotAddedOrUpdated.Create(device1);

            await device1VC.ResizePlotAsync(650, 550, 96);
            await plotReceivedTask;

            // Resizing device 1 should not have changed the active device
            _plotManager.ActiveDevice.Should().Be(device2);

            var bs = _plotManager.ActiveDevice.ActivePlot.Image as BitmapSource;
            bs.Should().HaveSamePixels(plot1to10larger);
        }

        [Test(ThreadType.UI)]
        public async Task DeviceCopyAsMetafile() {
            await InitializeGraphicsDevice();
            await ExecuteAndWaitForPlotsAsync(new string[] {
                "plot(1:10)",
            });

            var device1 = _plotManager.ActiveDevice;
            var device1VC = _plotManager.GetPlotVisualComponent(device1);
            var device1Commands = new RPlotDeviceCommands(_workflow, device1VC);
            var plot1 = device1.ActivePlot;

            await InitializeGraphicsDevice();
            await ExecuteAndWaitForPlotsAsync(new string[] {
                "plot(2:10)",
            });

            var device2 = _plotManager.ActiveDevice;

            Clipboard.Clear();

            var deviceCommands = new RPlotDeviceCommands(_workflow, device1VC);

            deviceCommands.CopyAsMetafile.Should().BeEnabled();
            await deviceCommands.CopyAsMetafile.InvokeAsync();

            // Exporting plot from device 1 should not have changed the active device
            _plotManager.ActiveDevice.Should().Be(device2);

            Clipboard.ContainsData(DataFormats.EnhancedMetafile).Should().BeTrue();
            _ui.LastShownErrorMessage.Should().BeNullOrEmpty();
        }

        [Test(ThreadType.UI)]
        public async Task DeviceCopy() {
            var plot1to10 = await GetExpectedImageAsync("bmp", 600, 500, 96, "plot1-10", "plot(1:10)");
            var plot2to10 = await GetExpectedImageAsync("bmp", 600, 500, 96, "plot2-10", "plot(2:10)");

            await InitializeGraphicsDevice();
            await ExecuteAndWaitForPlotsAsync(new string[] {
                "plot(1:10)",
            });

            var device1 = _plotManager.ActiveDevice;
            var device1VC = _plotManager.GetPlotVisualComponent(device1);
            var device1Commands = new RPlotDeviceCommands(_workflow, device1VC);
            var plot1 = device1.ActivePlot;

            await InitializeGraphicsDevice();
            await ExecuteAndWaitForPlotsAsync(new string[] {
                "plot(2:10)",
            });

            var device2 = _plotManager.ActiveDevice;
            var device2VC = _plotManager.GetPlotVisualComponent(device2);
            var device2Commands = new RPlotDeviceCommands(_workflow, device2VC);
            var plot2 = device2.ActivePlot;

            device1Commands.Copy.Should().BeEnabled();
            await device1Commands.Copy.InvokeAsync();

            _ui.LastShownErrorMessage.Should().BeNullOrEmpty();

            device2Commands.Paste.Should().BeEnabled();
            var plotReceivedTask = EventTaskSources.IRPlotDevice.PlotAddedOrUpdated.Create(device2);
            await device2Commands.Paste.InvokeAsync();
            await plotReceivedTask;

            _ui.LastShownErrorMessage.Should().BeNullOrEmpty();
            var bs = device2.ActivePlot.Image as BitmapSource;
            bs.Should().HaveSamePixels(plot1to10);
        }

        [Test(ThreadType.UI)]
        public async Task DeviceCut() {
            var plot1to10 = await GetExpectedImageAsync("bmp", 600, 500, 96, "plot1-10", "plot(1:10)");
            var plot2to10 = await GetExpectedImageAsync("bmp", 600, 500, 96, "plot2-10", "plot(2:10)");

            await InitializeGraphicsDevice();
            await ExecuteAndWaitForPlotsAsync(new string[] {
                "plot(1:10)",
            });

            var device1 = _plotManager.ActiveDevice;
            var device1VC = _plotManager.GetPlotVisualComponent(device1);
            var device1Commands = new RPlotDeviceCommands(_workflow, device1VC);
            var plot1 = device1.ActivePlot;

            await InitializeGraphicsDevice();
            await ExecuteAndWaitForPlotsAsync(new string[] {
                "plot(2:10)",
            });

            var device2 = _plotManager.ActiveDevice;
            var device2VC = _plotManager.GetPlotVisualComponent(device2);
            var device2Commands = new RPlotDeviceCommands(_workflow, device2VC);
            var plot2 = device2.ActivePlot;

            device1Commands.Cut.Should().BeEnabled();
            await device1Commands.Cut.InvokeAsync();

            _ui.LastShownErrorMessage.Should().BeNullOrEmpty();

            device2Commands.Paste.Should().BeEnabled();
            var plotReceivedTask = EventTaskSources.IRPlotDevice.PlotAddedOrUpdated.Create(device2);
            var plotRemovedTask = EventTaskSources.IRPlotDevice.PlotRemoved.Create(device1);
            await device2Commands.Paste.InvokeAsync();
            await plotReceivedTask;
            await plotRemovedTask;

            _ui.LastShownErrorMessage.Should().BeNullOrEmpty();

            device1.ActivePlot.Should().BeNull();
            device2.ActivePlot.Should().NotBeNull();

            var bs = device2.ActivePlot.Image as BitmapSource;
            bs.Should().HaveSamePixels(plot1to10);
        }

        [Test(ThreadType.UI)]
        public async Task DeviceRemoveAll() {
            await InitializeGraphicsDevice();
            await ExecuteAndWaitForPlotsAsync(new string[] {
                "plot(1:10)",
                "plot(1:20)",
            });

            var device1 = _plotManager.ActiveDevice;
            var device1VC = _plotManager.GetPlotVisualComponent(device1);
            var device1Commands = new RPlotDeviceCommands(_workflow, device1VC);
            var plot1 = device1.ActivePlot;

            await InitializeGraphicsDevice();
            await ExecuteAndWaitForPlotsAsync(new string[] {
                "plot(2:10)",
            });

            var device2 = _plotManager.ActiveDevice;
            var device2VC = _plotManager.GetPlotVisualComponent(device2);
            var plot2 = device2.ActivePlot;

            // Remove all plots from device 1
            device1Commands.RemoveAllPlots.Should().BeEnabled();
            var plotClearedTask = EventTaskSources.IRPlotDevice.Cleared.Create(device1);
            await device1Commands.RemoveAllPlots.InvokeAsync();
            await plotClearedTask;

            // Deleting the plots from device 1 should not have changed the active device
            _plotManager.ActiveDevice.Should().Be(device2);

            _ui.LastShownErrorMessage.Should().BeNullOrEmpty();

            device1.ActivePlot.Should().BeNull();
            device2.ActivePlot.Should().NotBeNull();
        }

        [Test(ThreadType.UI)]
        public async Task DeviceRemoveCurrent() {
            var plot1to10 = await GetExpectedImageAsync("bmp", 600, 500, 96, "plot1-10", "plot(1:10)");
            var plot2to10 = await GetExpectedImageAsync("bmp", 600, 500, 96, "plot2-10", "plot(2:10)");

            await InitializeGraphicsDevice();
            await ExecuteAndWaitForPlotsAsync(new string[] {
                "plot(1:10)",
                "plot(1:20)",
            });

            var device1 = _plotManager.ActiveDevice;
            var device1VC = _plotManager.GetPlotVisualComponent(device1);
            var device1Commands = new RPlotDeviceCommands(_workflow, device1VC);
            var plot1 = device1.ActivePlot;

            await InitializeGraphicsDevice();
            await ExecuteAndWaitForPlotsAsync(new string[] {
                "plot(2:10)",
            });

            var device2 = _plotManager.ActiveDevice;
            var device2VC = _plotManager.GetPlotVisualComponent(device2);
            var plot2 = device2.ActivePlot;

            // Remove plot 1:20 from device 1, leaving plot 1:10 as active
            device1Commands.RemoveCurrentPlot.Should().BeEnabled();
            var plotRemovedTask = EventTaskSources.IRPlotDevice.PlotRemoved.Create(device1);
            await device1Commands.RemoveCurrentPlot.InvokeAsync();
            await plotRemovedTask;

            // Deleting the plot from device 1 should not have changed the active device
            _plotManager.ActiveDevice.Should().Be(device2);

            _ui.LastShownErrorMessage.Should().BeNullOrEmpty();

            var bs = device1.ActivePlot.Image as BitmapSource;
            bs.Should().HaveSamePixels(plot1to10);

            bs = device2.ActivePlot.Image as BitmapSource;
            bs.Should().HaveSamePixels(plot2to10);
        }

        [Test(ThreadType.UI)]
        public async Task NewDevice() {
            var newCmd = new PlotDeviceNewCommand(_workflow);
            newCmd.Should().BeEnabled();

            _plotManager.ActiveDevice.Should().BeNull();

            var deviceChangedTask = EventTaskSources.IRPlotManager.ActiveDeviceChanged.Create(_plotManager);
            await newCmd.InvokeAsync();
            await deviceChangedTask;

            _plotManager.ActiveDevice.Should().NotBeNull();

            var eval = _workflow.ActiveWindow.InteractiveWindow.Evaluator;
            deviceChangedTask = EventTaskSources.IRPlotManager.ActiveDeviceChanged.Create(_plotManager);
            await eval.ExecuteCodeAsync("dev.off()".EnsureLineBreak());
            await deviceChangedTask;

            _plotManager.ActiveDevice.Should().BeNull();
        }

        [Test(ThreadType.UI)]
        public async Task ActivateDevice() {
            await InitializeGraphicsDevice();
            var device1 = _plotManager.ActiveDevice;
            var device1VC = _plotManager.GetPlotVisualComponent(device1);
            var device1Commands = new RPlotDeviceCommands(_workflow, device1VC);

            device1Commands.ActivatePlotDevice.Should().BeChecked();
            device1Commands.ActivatePlotDevice.Should().BeEnabled();

            await InitializeGraphicsDevice();
            var device2 = _plotManager.ActiveDevice;
            var device2VC = _plotManager.GetPlotVisualComponent(device2);
            var device2Commands = new RPlotDeviceCommands(_workflow, device2VC);

            device1Commands.ActivatePlotDevice.Should().BeUnchecked();
            device1Commands.ActivatePlotDevice.Should().BeEnabled();
            device2Commands.ActivatePlotDevice.Should().BeChecked();
            device2Commands.ActivatePlotDevice.Should().BeEnabled();

            var deviceChangedTask = EventTaskSources.IRPlotManager.ActiveDeviceChanged.Create(_plotManager);
            await device1Commands.ActivatePlotDevice.InvokeAsync();
            await deviceChangedTask;

            _plotManager.ActiveDevice.Should().Be(device1);
            device1Commands.ActivatePlotDevice.Should().BeChecked();
            device2Commands.ActivatePlotDevice.Should().BeUnchecked();

            deviceChangedTask = EventTaskSources.IRPlotManager.ActiveDeviceChanged.Create(_plotManager);
            await device2Commands.ActivatePlotDevice.InvokeAsync();
            await deviceChangedTask;

            _plotManager.ActiveDevice.Should().Be(device2);
            device1Commands.ActivatePlotDevice.Should().BeUnchecked();
            device2Commands.ActivatePlotDevice.Should().BeChecked();
        }

        [Test(ThreadType.UI)]
        public async Task ExportAsPdf() {
            await InitializeGraphicsDevice();
            await ExecuteAndWaitForPlotsAsync(new string[] {
                "plot(1:10)",
            });

            var outputFilePath = _testFiles.GetDestinationPath("ExportedPlot.pdf");
            FileDialog.SaveFilePath = outputFilePath;

            var deviceVC = _plotManager.GetPlotVisualComponent(_plotManager.ActiveDevice);
            var deviceCommands = new RPlotDeviceCommands(_workflow, deviceVC);

            deviceCommands.ExportAsPdf.Should().BeEnabled();
            await deviceCommands.ExportAsPdf.InvokeAsync();

            File.Exists(outputFilePath).Should().BeTrue();
            _ui.LastShownErrorMessage.Should().BeNullOrEmpty();
        }

        [Test(ThreadType.UI)]
        public async Task ExportAsImage() {
            await InitializeGraphicsDevice();
            await ExecuteAndWaitForPlotsAsync(new string[] {
                "plot(1:10)",
            });

            foreach (var ext in new string[] { "bmp", "jpg", "jpeg", "png", "tif", "tiff" }) {
                var outputFilePath = _testFiles.GetDestinationPath("ExportedPlot." + ext);
                FileDialog.SaveFilePath = outputFilePath;

                var deviceVC = _plotManager.GetPlotVisualComponent(_plotManager.ActiveDevice);
                var deviceCommands = new RPlotDeviceCommands(_workflow, deviceVC);

                deviceCommands.ExportAsImage.Should().BeEnabled();
                await deviceCommands.ExportAsImage.InvokeAsync();

                File.Exists(outputFilePath).Should().BeTrue();
                _ui.LastShownErrorMessage.Should().BeNullOrEmpty();

                var image = BitmapImageFactory.Load(outputFilePath);
                image.PixelWidth.Should().Be(600);
                image.PixelHeight.Should().Be(500);
                ((int)Math.Round(image.DpiX)).Should().Be(96);
                ((int)Math.Round(image.DpiY)).Should().Be(96);
            }
        }

        [Test(ThreadType.UI)]
        public async Task ExportAsImageUnsupportedExtension() {
            await InitializeGraphicsDevice();
            await ExecuteAndWaitForPlotsAsync(new string[] {
                "plot(1:10)",
            });

            // The file extension of the file the user selected in the save
            // dialog is what determines the image format. When it's an
            // unsupported format, we show an error msg.
            var outputFilePath = _testFiles.GetDestinationPath("ExportedPlot.unsupportedextension");
            FileDialog.SaveFilePath = outputFilePath;

            var deviceVC = _plotManager.GetPlotVisualComponent(_plotManager.ActiveDevice);
            var deviceCommands = new RPlotDeviceCommands(_workflow, deviceVC);

            deviceCommands.ExportAsImage.Should().BeEnabled();
            await deviceCommands.ExportAsImage.InvokeAsync();

            File.Exists(FileDialog.SaveFilePath).Should().BeFalse();
            _ui.LastShownErrorMessage.Should().Contain(".unsupportedextension");
        }

        [Test(ThreadType.UI)]
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

            await InitializeGraphicsDevice();
            await ExecuteAndWaitForPlotsAsync(scripts);

            var deviceVC = _plotManager.GetPlotVisualComponent(_plotManager.ActiveDevice);
            var deviceCommands = new RPlotDeviceCommands(_workflow, deviceVC);

            for (int i = 0; i < scripts.Length - 1; i++) {
                deviceCommands.PreviousPlot.Should().BeEnabled();
                var plotReceivedTask = EventTaskSources.IRPlotDevice.PlotAddedOrUpdated.Create(_plotManager.ActiveDevice);
                await deviceCommands.PreviousPlot.InvokeAsync();
                await plotReceivedTask;
            }

            deviceCommands.PreviousPlot.Should().BeDisabled();

            for (int i = 0; i < scripts.Length - 1; i++) {
                deviceCommands.NextPlot.Should().BeEnabled();
                var plotReceivedTask = EventTaskSources.IRPlotDevice.PlotAddedOrUpdated.Create(_plotManager.ActiveDevice);
                await deviceCommands.NextPlot.InvokeAsync();
                await plotReceivedTask;
            }

            deviceCommands.NextPlot.Should().BeDisabled();
        }

        [Test(ThreadType.UI)]
        public async Task PlotError() {
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

            var deviceVC = _plotManager.GetPlotVisualComponent(_plotManager.ActiveDevice);
            var deviceCommands = new RPlotDeviceCommands(_workflow, deviceVC);

            // Navigating to a plot in the history that cannot be rendered
            // will send a plot with zero-file size, which translate into an error message.
            deviceCommands.PreviousPlot.Should().BeEnabled();
            var plotReceivedTask = EventTaskSources.IRPlotDevice.PlotAddedOrUpdated.Create(_plotManager.ActiveDevice);
            await deviceCommands.PreviousPlot.InvokeAsync();
            await plotReceivedTask;

            _plotManager.ActiveDevice.ActivePlot.Image.Should().BeNull();

            deviceCommands.PreviousPlot.Should().BeEnabled();
            plotReceivedTask = EventTaskSources.IRPlotDevice.PlotAddedOrUpdated.Create(_plotManager.ActiveDevice);
            await deviceCommands.PreviousPlot.InvokeAsync();
            await plotReceivedTask;

            _plotManager.ActiveDevice.ActivePlot.Image.Should().NotBeNull();

            deviceCommands.PreviousPlot.Should().BeDisabled();
        }

        [Test(ThreadType.UI, Skip = "https://github.com/Microsoft/RTVS/issues/2939")]
        public async Task LocatorCommand() {
            _plotDeviceVisualComponentContainerFactory.DeviceProperties = new PlotDeviceProperties(360, 360, 96);

            await InitializeGraphicsDevice();

            await ExecuteAndWaitForPlotsAsync(new[] {
                "plot(0:10)",
            });

            var device = _plotManager.ActiveDevice;
            device.Should().NotBeNull();

            var deviceVC = _plotManager.GetPlotVisualComponent(device);
            var deviceCommands = new RPlotDeviceCommands(_workflow, deviceVC);
            device.LocatorMode.Should().BeFalse();

            deviceCommands.EndLocator.Should().BeInvisibleAndDisabled();

            var firstLocatorModeTask = EventTaskSources.IRPlotDevice.LocatorModeChanged.Create(device);
            var locatorTask = ExecuteAndDoNotWaitForPlotsAsync(new[] {
                "res <- locator()",
            });
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
                device.LocatorMode.Should().BeTrue();
                deviceCommands.EndLocator.Should().BeEnabled();

                // Send a result with a click point, which will causes
                // locator mode to end and immediately start again
                var locatorModeTask = EventTaskSources.IRPlotDevice.LocatorModeChanged.Create(device);
                _plotManager.EndLocatorMode(device, LocatorResult.CreateClicked((int)point.X, (int)point.Y));
                await locatorModeTask;

                device.LocatorMode.Should().BeFalse();
                deviceCommands.EndLocator.Should().BeInvisibleAndDisabled();

                locatorModeTask = EventTaskSources.IRPlotDevice.LocatorModeChanged.Create(device);
                await locatorModeTask;
            }

            // Send a result with a not clicked result, which causes
            // locator mode to end, and the high-level locator() function
            // call will return.
            var lastLocatorModeTask = EventTaskSources.IRPlotDevice.LocatorModeChanged.Create(device);
            await deviceCommands.EndLocator.InvokeAsync();
            await lastLocatorModeTask;

            device.LocatorMode.Should().BeFalse();
            deviceCommands.EndLocator.Should().BeInvisibleAndDisabled();

            string outputFilePath = _testFiles.GetDestinationPath("LocatorResult.csv");
            await ExecuteAndDoNotWaitForPlotsAsync(new[] {
                $"write.csv(res, {outputFilePath.ToRPath().ToRStringLiteral()})"
            });

            var x = new double[] { -2.48008095952895, 1.55378525638498, 10.0697250455366 };
            var y = new double[] { 14.4476461865435, 12.091623959219, 9.73560173189449 };
            CheckLocatorResult(outputFilePath, x, y);

            await locatorTask;
        }

        [Test(ThreadType.UI)]
        public async Task LocatorCommandNoClick() {
            _plotDeviceVisualComponentContainerFactory.DeviceProperties = new PlotDeviceProperties(360, 360, 96);

            await InitializeGraphicsDevice();

            await ExecuteAndWaitForPlotsAsync(new string[] {
                "plot(0:10)",
            });

            var device = _plotManager.ActiveDevice;
            device.Should().NotBeNull();

            var deviceVC = _plotManager.GetPlotVisualComponent(device);
            var deviceCommands = new RPlotDeviceCommands(_workflow, deviceVC);
            device.LocatorMode.Should().BeFalse();

            deviceCommands.EndLocator.Should().BeInvisibleAndDisabled();

            var locatorModeTask = EventTaskSources.IRPlotDevice.LocatorModeChanged.Create(device);
            var locatorTask = ExecuteAndDoNotWaitForPlotsAsync(new string[] {
                "res <- locator()",
            });
            await locatorModeTask;

            device.LocatorMode.Should().BeTrue();
            deviceCommands.EndLocator.Should().BeEnabled();

            locatorModeTask = EventTaskSources.IRPlotDevice.LocatorModeChanged.Create(device);
            await deviceCommands.EndLocator.InvokeAsync();
            await locatorModeTask;

            device.LocatorMode.Should().BeFalse();
            deviceCommands.EndLocator.Should().BeInvisibleAndDisabled();

            string outputFilePath = _testFiles.GetDestinationPath("LocatorResultNoClick.csv");
            await ExecuteAndDoNotWaitForPlotsAsync(new string[] {
                $"write.csv(res, {outputFilePath.ToRPath().ToRStringLiteral()})"
            });

            string output = File.ReadAllText(outputFilePath);
            output.Trim().Should().Be("\"\"");

            await locatorTask;
        }

        [Test(ThreadType.UI)]
        public async Task LocatorNotEnded() {
            _plotDeviceVisualComponentContainerFactory.DeviceProperties = new PlotDeviceProperties(360, 360, 96);

            await InitializeGraphicsDevice();

            await ExecuteAndWaitForPlotsAsync(new string[] {
                "plot(0:10)",
            });

            var device = _plotManager.ActiveDevice;
            device.Should().NotBeNull();

            device.LocatorMode.Should().BeFalse();

            var locatorModeTask = EventTaskSources.IRPlotDevice.LocatorModeChanged.Create(device);
            var locatorTask = ExecuteAndDoNotWaitForPlotsAsync(new string[] {
                "res <- locator()",
            });
            await locatorModeTask;

            device.LocatorMode.Should().BeTrue();
        }

        [Test(ThreadType.UI)]
        public async Task HistoryActivate() {
            using (_plotManager.GetOrCreateVisualComponent(_plotHistoryVisualComponentContainerFactory, 0)) {
                var plot1to10 = await GetExpectedImageAsync("bmp", 600, 500, 96, "plot1-10", "plot(1:10)");
                var plot2to10 = await GetExpectedImageAsync("bmp", 600, 500, 96, "plot2-10", "plot(2:10)");

                await InitializeGraphicsDevice();
                await ExecuteAndWaitForPlotsAsync(new string[] {
                    "plot(1:10)",
                    "plot(2:10)",
                });

                var deviceVC = _plotManager.GetPlotVisualComponent(_plotManager.ActiveDevice);
                var historyVC = _plotManager.HistoryVisualComponent;
                var historyCommands = new RPlotHistoryCommands(_workflow, historyVC);

                historyCommands.ActivatePlot.Should().BeDisabled();

                // Select and activate the first plot
                historyVC.SelectedPlots = new[] { _plotManager.ActiveDevice.GetPlotAt(0) };
                historyCommands.ActivatePlot.Should().BeEnabled();
                var plotReceivedTask = EventTaskSources.IRPlotDevice.PlotAddedOrUpdated.Create(_plotManager.ActiveDevice);
                await historyCommands.ActivatePlot.InvokeAsync();
                await plotReceivedTask;

                var bs = _plotManager.ActiveDevice.ActivePlot.Image as BitmapSource;
                bs.Should().HaveSamePixels(plot1to10);

                _ui.LastShownErrorMessage.Should().BeNullOrEmpty();
            }
        }

        [Test(ThreadType.UI)]
        public async Task HistoryCopy() {
            using (_plotManager.GetOrCreateVisualComponent(_plotHistoryVisualComponentContainerFactory, 0)) {
                await InitializeGraphicsDevice();
                await ExecuteAndWaitForPlotsAsync(new string[] {
                    "plot(1:10)",
                });

                var historyVC = _plotManager.HistoryVisualComponent;
                var historyCommands = new RPlotHistoryCommands(_workflow, historyVC);

                var device1 = _plotManager.ActiveDevice;
                var device1VC = _plotManager.GetPlotVisualComponent(device1);
                var plot1 = device1.ActivePlot;

                await InitializeGraphicsDevice();
                await ExecuteAndWaitForPlotsAsync(new string[] {
                    "plot(2:10)",
                });

                var device2 = _plotManager.ActiveDevice;
                var device2VC = _plotManager.GetPlotVisualComponent(device2);
                var device2Commands = new RPlotDeviceCommands(_workflow, device2VC);
                var plot2 = device2.ActivePlot;

                historyVC.SelectedPlots = new[] { plot1 };

                historyCommands.Copy.Should().BeEnabled();
                await historyCommands.Copy.InvokeAsync();

                _ui.LastShownErrorMessage.Should().BeNullOrEmpty();

                device2Commands.Paste.Should().BeEnabled();
                var plotReceivedTask = EventTaskSources.IRPlotDevice.PlotAddedOrUpdated.Create(device2);
                await device2Commands.Paste.InvokeAsync();
                await plotReceivedTask;

                _ui.LastShownErrorMessage.Should().BeNullOrEmpty();

                var bs = device2.ActivePlot.Image as BitmapSource;
                bs.Should().HaveSamePixels(plot1.Image as BitmapSource);
            }
        }

        [Test(ThreadType.UI)]
        public async Task HistoryRemove() {
            using (_plotManager.GetOrCreateVisualComponent(_plotHistoryVisualComponentContainerFactory, 0)) {
                await InitializeGraphicsDevice();
                await ExecuteAndWaitForPlotsAsync(new string[] {
                    "plot(1:10)",
                });

                var historyVC = _plotManager.HistoryVisualComponent;
                var historyCommands = new RPlotHistoryCommands(_workflow, historyVC);

                var device1 = _plotManager.ActiveDevice;
                var device1VC = _plotManager.GetPlotVisualComponent(device1);
                var plot1 = device1.ActivePlot;

                await InitializeGraphicsDevice();
                await ExecuteAndWaitForPlotsAsync(new string[] {
                    "plot(2:10)",
                });

                var device2 = _plotManager.ActiveDevice;
                var device2VC = _plotManager.GetPlotVisualComponent(device2);
                var plot2 = device2.ActivePlot;

                historyVC.SelectedPlots = Enumerable.Empty<IRPlot>();
                historyCommands.Remove.Should().BeDisabled();

                // Select the only plot in device 1 and remove it
                historyVC.SelectedPlots = new[] { plot1 };
                historyCommands.Remove.Should().BeEnabled();
                var plotRemovedTask = EventTaskSources.IRPlotDevice.PlotRemoved.Create(device1);
                await historyCommands.Remove.InvokeAsync();
                await plotRemovedTask;

                device1.ActivePlot.Should().BeNull();
                device1.PlotCount.Should().Be(0);
                device1.ActiveIndex.Should().Be(-1);

                // Deleting the plot from device 1 should not have changed the active device
                _plotManager.ActiveDevice.Should().Be(device2);

                // Select the only plot in device 2 and remove it
                historyVC.SelectedPlots = new[] { plot2 };
                historyCommands.Remove.Should().BeEnabled();
                plotRemovedTask = EventTaskSources.IRPlotDevice.PlotRemoved.Create(device2);
                await historyCommands.Remove.InvokeAsync();
                await plotRemovedTask;

                device2.ActivePlot.Should().BeNull();
                device2.PlotCount.Should().Be(0);
                device2.ActiveIndex.Should().Be(-1);
            }
        }

        [Test(ThreadType.UI)]
        public async Task HistoryZoom() {
            using (_plotManager.GetOrCreateVisualComponent(_plotHistoryVisualComponentContainerFactory, 0)) {
                var historyVC = _plotManager.HistoryVisualComponent;
                var historyCommands = new RPlotHistoryCommands(_workflow, historyVC);

                // Default is 96, and minimum is 48, so we are able to zoom out once
                historyCommands.ZoomOut.Should().BeEnabled();
                await historyCommands.ZoomOut.InvokeAsync();
                historyCommands.ZoomOut.Should().BeDisabled();

                historyCommands.ZoomIn.Should().BeEnabled();
                await historyCommands.ZoomIn.InvokeAsync();
                historyCommands.ZoomOut.Should().BeEnabled();
            }
        }

        [Test(ThreadType.UI)]
        public async Task HistoryAutoHide() {
            using (_plotManager.GetOrCreateVisualComponent(_plotHistoryVisualComponentContainerFactory, 0)) {
                var historyVC = _plotManager.HistoryVisualComponent;
                var historyCommands = new RPlotHistoryCommands(_workflow, historyVC);

                historyVC.AutoHide = false;
                historyCommands.AutoHide.Should().BeVisibleAndEnabled();
                historyCommands.AutoHide.Should().BeUnchecked();

                await historyCommands.AutoHide.InvokeAsync();
                historyVC.AutoHide.Should().BeTrue();
                historyCommands.AutoHide.Should().BeVisibleAndEnabled();
                historyCommands.AutoHide.Should().BeChecked();

                await historyCommands.AutoHide.InvokeAsync();
                historyVC.AutoHide.Should().BeFalse();
                historyCommands.AutoHide.Should().BeVisibleAndEnabled();
                historyCommands.AutoHide.Should().BeUnchecked();
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
            var deviceCreatedTask = EventTaskSources.IRPlotManager.DeviceAdded.Create(_plotManager);
            var deviceChangedTask = EventTaskSources.IRPlotManager.ActiveDeviceChanged.Create(_plotManager);

            var eval = _workflow.ActiveWindow.InteractiveWindow.Evaluator;
            var result = await eval.ExecuteCodeAsync("dev.new()\n");
            result.IsSuccessful.Should().BeTrue();

            await ParallelTools.WhenAll(20000, deviceCreatedTask, deviceChangedTask);
        }

        private Task ExecuteAndWaitForPlotsAsync(string[] scripts) => ExecuteAndWaitForPlotsAsync(_plotManager.ActiveDevice, scripts);

        private async Task ExecuteAndWaitForPlotsAsync(IRPlotDevice device, string[] scripts) {
            var eval = _workflow.ActiveWindow.InteractiveWindow.Evaluator;

            foreach (string script in scripts) {
                var plotReceivedTask = EventTaskSources.IRPlotDevice.PlotAddedOrUpdated.Create(device);

                var result = await eval.ExecuteCodeAsync(script.EnsureLineBreak());
                result.IsSuccessful.Should().BeTrue();

                await plotReceivedTask.Should().BeCompletedAsync(10000, $"it should execute script: {script}");
            }
        }

        private async Task ExecuteAndDoNotWaitForPlotsAsync(string[] scripts) {
            var eval = _workflow.ActiveWindow.InteractiveWindow.Evaluator;
            foreach (string script in scripts) {
                var result = await eval.ExecuteCodeAsync(script.EnsureLineBreak());
                result.IsSuccessful.Should().BeTrue();
            }
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

        private void CheckEnabledCommands(IRPlotDeviceVisualComponent visualComponent, bool isFirst, bool isLast, bool anyPlot) {
            var deviceCommands = new RPlotDeviceCommands(_workflow, visualComponent);

            deviceCommands.EndLocator.Should().BeDisabled();

            if (anyPlot) {
                deviceCommands.RemoveAllPlots.Should().BeEnabled();
                deviceCommands.RemoveCurrentPlot.Should().BeEnabled();
                deviceCommands.Cut.Should().BeEnabled();
                deviceCommands.Copy.Should().BeEnabled();
                deviceCommands.CopyAsBitmap.Should().BeEnabled();
                deviceCommands.CopyAsMetafile.Should().BeEnabled();
                deviceCommands.ExportAsImage.Should().BeEnabled();
                deviceCommands.ExportAsPdf.Should().BeEnabled();
            } else {
                deviceCommands.RemoveAllPlots.Should().BeDisabled();
                deviceCommands.RemoveCurrentPlot.Should().BeDisabled();
                deviceCommands.Cut.Should().BeDisabled();
                deviceCommands.Copy.Should().BeDisabled();
                deviceCommands.CopyAsBitmap.Should().BeDisabled();
                deviceCommands.CopyAsMetafile.Should().BeDisabled();
                deviceCommands.ExportAsImage.Should().BeDisabled();
                deviceCommands.ExportAsPdf.Should().BeDisabled();
            }

            if (isFirst || !anyPlot) {
                deviceCommands.PreviousPlot.Should().BeDisabled();
            } else {
                deviceCommands.PreviousPlot.Should().BeEnabled();
            }

            if (isLast || !anyPlot) {
                deviceCommands.NextPlot.Should().BeDisabled();
            } else {
                deviceCommands.NextPlot.Should().BeEnabled();
            }
        }
    }
}
