// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using FluentAssertions;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Components.Plots;
using Microsoft.R.Components.Plots.Implementation;
using Microsoft.R.Components.Plots.Implementation.ViewModel;
using Microsoft.R.Components.Test.Fakes.VisualComponentFactories;
using Microsoft.R.Host.Client;
using Microsoft.UnitTests.Core.FluentAssertions;
using Microsoft.UnitTests.Core.Threading;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.UnitTests.Core.XUnit.MethodFixtures;
using Xunit;

namespace Microsoft.R.Components.Test.Plots {
    [ExcludeFromCodeCoverage]
    [Category.Plots]
    [Collection(CollectionNames.NonParallel)]
    public class RPlotIntegrationUITest : IAsyncLifetime {
        private readonly ContainerHostMethodFixture _containerHost;
        private readonly IRInteractiveWorkflowVisual _workflow;
        private readonly TestRPlotDeviceVisualComponentContainerFactory _plotDeviceVisualComponentContainerFactory;
        private readonly IRPlotHistoryVisualComponentContainerFactory _plotHistoryVisualComponentContainerFactory;
        private readonly IRPlotManagerVisual _plotVisual;
        private readonly IRPlotDeviceVisualComponent _plotVisualComponent;
        private IDisposable _containerDisposable;
        private IInteractiveWindowVisualComponent _interactiveWindow;
        private const int PlotWindowInstanceId = 1;

        public RPlotIntegrationUITest(IServiceContainer services, ContainerHostMethodFixture containerHost) {
            _containerHost = containerHost;
            _plotDeviceVisualComponentContainerFactory = services.GetService<TestRPlotDeviceVisualComponentContainerFactory>();

            // Don't override the standard behavior of using the control size
            _plotDeviceVisualComponentContainerFactory.DeviceProperties = null;
            _plotHistoryVisualComponentContainerFactory = services.GetService<IRPlotHistoryVisualComponentContainerFactory>();
            _workflow = services.GetService<IRInteractiveWorkflowVisualProvider>().GetOrCreate();
            _plotVisual = (IRPlotManagerVisual)_workflow.Plots;
            _plotVisualComponent = UIThreadHelper.Instance.Invoke(() => _workflow.Plots.GetOrCreateVisualComponent(_plotDeviceVisualComponentContainerFactory, PlotWindowInstanceId));
             UIThreadHelper.Instance.Invoke(() => ((IRPlotManagerVisual)_workflow.Plots).RegisterVisualComponent(_plotVisualComponent));
        }

        public async Task InitializeAsync() {
            await _workflow.RSessions.TrySwitchBrokerAsync(nameof(RPlotIntegrationUITest));
            _interactiveWindow = await _workflow.GetOrCreateVisualComponentAsync();

            _plotVisualComponent.Control.Width = 600;
            _plotVisualComponent.Control.Height = 500;
            _containerDisposable = await _containerHost.AddToHost(_plotVisualComponent.Control);
        }

        public Task DisposeAsync() {
            UIThreadHelper.Instance.Invoke(() => {
                _plotVisualComponent.Dispose();
                _interactiveWindow.Dispose();
            });

            _containerDisposable?.Dispose();
            return Task.CompletedTask;
        }

        [Test(ThreadType.UI)]
        public async Task ResizePlot() {
            await InitializeGraphicsDevice();
            await ExecuteAndWaitForPlotsAsync(new [] {
                "plot(1:10)",
            });

            var plotVC = _plotVisual.GetPlotVisualComponent(_workflow.Plots.ActiveDevice);

            var expectedSize = GetPixelSize(600, 500);
            var bs = _plotVisual.ActiveDevice.ActivePlot.Image as BitmapSource;
            bs.PixelWidth.Should().Be((int)expectedSize.Width);
            bs.PixelHeight.Should().Be((int)expectedSize.Height);

            var plotReceivedTask = EventTaskSources.IRPlotDevice.PlotAddedOrUpdated.Create(_workflow.Plots.ActiveDevice);

            _plotVisualComponent.Control.Height = 450;

            await plotReceivedTask;

            var expectedModifiedSize = GetPixelSize(600, 450);
            bs = _plotVisual.ActiveDevice.ActivePlot.Image as BitmapSource;
            bs.PixelWidth.Should().Be((int)expectedModifiedSize.Width);
            bs.PixelHeight.Should().Be((int)expectedModifiedSize.Height);
        }

        [Test(ThreadType.UI)]
        public async Task ResizePlotError() {
            using (_plotVisual.GetOrCreateVisualComponent(_plotHistoryVisualComponentContainerFactory, 0)) {
                await _workflow.RSession.HostStarted.Should().BeCompletedAsync(50000);
                await InitializeGraphicsDevice();
                await ExecuteAndWaitForPlotsAsync(new string[] {
                    "plot(mtcars)",
                });

                var plotVC = _plotVisual.GetPlotVisualComponent(_plotVisual.ActiveDevice);

                var expectedSize = GetPixelSize(600, 500);
                var bs = _plotVisual.ActiveDevice.ActivePlot.Image as BitmapSource;
                bs.PixelWidth.Should().Be((int)expectedSize.Width);
                bs.PixelHeight.Should().Be((int)expectedSize.Height);

                var plotReceivedTask = EventTaskSources.IRPlotDevice.PlotAddedOrUpdated.Create(_plotVisual.ActiveDevice);

                // mtcars cannot be rendered at 200
                _plotVisualComponent.Control.Width = 200;

                await plotReceivedTask;

                _plotVisual.ActiveDevice.ActivePlot.Image.Should().BeNull();

                // Plot history should have only one entry after a plot rendered ok followed by error
                var historyVC = (RPlotHistoryVisualComponent)_plotVisual.HistoryVisualComponent;
                var historyVM = (RPlotHistoryViewModel)historyVC.Control.DataContext;
                historyVM.Entries.Count.Should().Be(1);
                historyVM.Entries[0].PlotImage.Should().NotBeNull();
            }
        }

        private async Task InitializeGraphicsDevice() {
            var deviceCreatedTask = EventTaskSources.IRPlotManager.DeviceAdded.Create(_plotVisual);
            var deviceChangedTask = EventTaskSources.IRPlotManager.ActiveDeviceChanged.Create(_plotVisual);

            var eval = _workflow.ActiveWindow.InteractiveWindow.Evaluator;
            var result = await eval.ExecuteCodeAsync("dev.new()\n");
            result.IsSuccessful.Should().BeTrue();

            await ParallelTools.WhenAll(20000, deviceCreatedTask, deviceChangedTask);
        }

        private async Task ExecuteAndWaitForPlotsAsync(string[] scripts) {
            await ExecuteAndWaitForPlotsAsync(_plotVisual.ActiveDevice, scripts);
        }

        private async Task ExecuteAndWaitForPlotsAsync(IRPlotDevice device, string[] scripts) {
            var eval = _workflow.ActiveWindow.InteractiveWindow.Evaluator;

            foreach (string script in scripts) {
                var plotReceivedTask = EventTaskSources.IRPlotDevice.PlotAddedOrUpdated.Create(device);

                var result = await eval.ExecuteCodeAsync(script.EnsureLineBreak());
                result.IsSuccessful.Should().BeTrue();

                await plotReceivedTask.Should().BeCompletedAsync(10000, $"it should execute script: {script}");
            }
        }

        private Size GetPixelSize(int width, int height) {
            var source = PresentationSource.FromVisual(_plotVisualComponent.Control);
            return WpfUnitsConversion.ToPixels(source, new Size(width, height));
        }
    }
}
