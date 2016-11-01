// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using FluentAssertions;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Test.Fakes.Shell;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Components.Plots;
using Microsoft.R.Components.Plots.Implementation;
using Microsoft.R.Components.Plots.Implementation.ViewModel;
using Microsoft.R.Components.Test.Fakes.InteractiveWindow;
using Microsoft.R.Components.Test.Fakes.VisualComponentFactories;
using Microsoft.R.Host.Client;
using Microsoft.UnitTests.Core.Mef;
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
        private readonly IExportProvider _exportProvider;
        private readonly TestRInteractiveWorkflowProvider _workflowProvider;
        private readonly IRInteractiveWorkflow _workflow;
        private readonly IInteractiveWindowComponentContainerFactory _componentContainerFactory;
        private readonly TestRPlotDeviceVisualComponentContainerFactory _plotDeviceVisualComponentContainerFactory;
        private readonly IRPlotHistoryVisualComponentContainerFactory _plotHistoryVisualComponentContainerFactory;
        private readonly MethodInfo _testMethod;
        private readonly TestFilesFixture _testFiles;
        private IInteractiveWindowVisualComponent _replVisualComponent;
        private IRPlotDeviceVisualComponent _plotVisualComponent;
        private IDisposable _containerDisposable;
        private const int PlotWindowInstanceId = 1;

        public RPlotIntegrationUITest(RComponentsMefCatalogFixture catalog, ContainerHostMethodFixture containerHost, TestMethodFixture testMethod, TestFilesFixture testFiles) {
            _containerHost = containerHost;
            _exportProvider = catalog.CreateExportProvider();
            _workflowProvider = _exportProvider.GetExportedValue<TestRInteractiveWorkflowProvider>();
            _workflowProvider.BrokerName = nameof(RPlotIntegrationTest);
            _plotDeviceVisualComponentContainerFactory = _exportProvider.GetExportedValue<TestRPlotDeviceVisualComponentContainerFactory>();
            // Don't override the standard behavior of using the control size
            _plotDeviceVisualComponentContainerFactory.DeviceProperties = null;
            _plotHistoryVisualComponentContainerFactory = _exportProvider.GetExportedValue<IRPlotHistoryVisualComponentContainerFactory>();
            _workflow = _exportProvider.GetExportedValue<IRInteractiveWorkflowProvider>().GetOrCreate();
            _componentContainerFactory = _exportProvider.GetExportedValue<IInteractiveWindowComponentContainerFactory>();
            _testMethod = testMethod.MethodInfo;
            _testFiles = testFiles;
            _plotVisualComponent = UIThreadHelper.Instance.Invoke(() => _workflow.Plots.GetOrCreateVisualComponent(_plotDeviceVisualComponentContainerFactory, PlotWindowInstanceId));
            UIThreadHelper.Instance.Invoke(() => _workflow.Plots.RegisterVisualComponent(_plotVisualComponent));
        }

        public async Task InitializeAsync() {
            _plotVisualComponent.Control.Width = 600;
            _plotVisualComponent.Control.Height = 500;
            _replVisualComponent = await _workflow.GetOrCreateVisualComponent(_componentContainerFactory);
            _containerDisposable = await _containerHost.AddToHost(_plotVisualComponent.Control);
        }

        public Task DisposeAsync() {
            UIThreadHelper.Instance.Invoke(() => {
                _plotVisualComponent.Dispose();
            });

            _containerDisposable?.Dispose();
            _exportProvider.Dispose();
            _replVisualComponent.Dispose();
            return Task.CompletedTask;
        }

        [Test(ThreadType.UI)]
        public async Task ResizePlot() {
            await InitializeGraphicsDevice();
            await ExecuteAndWaitForPlotsAsync(new [] {
                "plot(1:10)",
            });

            var plotVC = _workflow.Plots.GetPlotVisualComponent(_workflow.Plots.ActiveDevice);

            var expectedSize = GetPixelSize(600, 500);
            _workflow.Plots.ActiveDevice.ActivePlot.Image.PixelWidth.Should().Be((int)expectedSize.Width);
            _workflow.Plots.ActiveDevice.ActivePlot.Image.PixelHeight.Should().Be((int)expectedSize.Height);

            var plotReceivedTask = EventTaskSources.IRPlotDevice.PlotAddedOrUpdated.Create(_workflow.Plots.ActiveDevice);

            _plotVisualComponent.Control.Height = 450;

            await plotReceivedTask;

            var expectedModifiedSize = GetPixelSize(600, 450);
            _workflow.Plots.ActiveDevice.ActivePlot.Image.PixelWidth.Should().Be((int)expectedModifiedSize.Width);
            _workflow.Plots.ActiveDevice.ActivePlot.Image.PixelHeight.Should().Be((int)expectedModifiedSize.Height);
        }

        [Test(ThreadType.UI)]
        public async Task ResizePlotError() {
            using (_workflow.Plots.GetOrCreateVisualComponent(_plotHistoryVisualComponentContainerFactory, 0)) {
                await InitializeGraphicsDevice();
                await ExecuteAndWaitForPlotsAsync(new string[] {
                    "plot(mtcars)",
                });

                var plotVC = _workflow.Plots.GetPlotVisualComponent(_workflow.Plots.ActiveDevice);

                var expectedSize = GetPixelSize(600, 500);
                _workflow.Plots.ActiveDevice.ActivePlot.Image.PixelWidth.Should().Be((int)expectedSize.Width);
                _workflow.Plots.ActiveDevice.ActivePlot.Image.PixelHeight.Should().Be((int)expectedSize.Height);

                var plotReceivedTask = EventTaskSources.IRPlotDevice.PlotAddedOrUpdated.Create(_workflow.Plots.ActiveDevice);

                // mtcars cannot be rendered at 200
                _plotVisualComponent.Control.Width = 200;

                await plotReceivedTask;

                _workflow.Plots.ActiveDevice.ActivePlot.Image.Should().BeNull();

                // Plot history should have only one entry after a plot rendered ok followed by error
                var historyVC = (RPlotHistoryVisualComponent)_workflow.Plots.HistoryVisualComponent;
                var historyVM = (RPlotHistoryViewModel)historyVC.Control.DataContext;
                historyVM.Entries.Count.Should().Be(1);
                historyVM.Entries[0].PlotImage.Should().NotBeNull();
            }
        }

        private async Task InitializeGraphicsDevice() {
            var deviceCreatedTask = EventTaskSources.IRPlotManager.DeviceAdded.Create(_workflow.Plots);
            var deviceChangedTask = EventTaskSources.IRPlotManager.ActiveDeviceChanged.Create(_workflow.Plots);

            var eval = _workflow.ActiveWindow.InteractiveWindow.Evaluator;
            var result = await eval.ExecuteCodeAsync("dev.new()\n");
            result.IsSuccessful.Should().BeTrue();

            await ParallelTools.WhenAll(20000, deviceCreatedTask, deviceChangedTask);
        }

        private async Task ExecuteAndWaitForPlotsAsync(string[] scripts) {
            await ExecuteAndWaitForPlotsAsync(_workflow.Plots.ActiveDevice, scripts);
        }

        private async Task ExecuteAndWaitForPlotsAsync(IRPlotDevice device, string[] scripts) {
            var eval = _workflow.ActiveWindow.InteractiveWindow.Evaluator;

            foreach (string script in scripts) {
                var plotReceivedTask = EventTaskSources.IRPlotDevice.PlotAddedOrUpdated.Create(device);

                var result = await eval.ExecuteCodeAsync(script.EnsureLineBreak());
                result.IsSuccessful.Should().BeTrue();

                await ParallelTools.When(plotReceivedTask, $"{nameof(ExecuteAndWaitForPlotsAsync)} failed on timeout for script: {script}");
            }
        }

        private Size GetPixelSize(int width, int height) {
            var source = PresentationSource.FromVisual(_plotVisualComponent.Control as Visual);
            return WpfUnitsConversion.ToPixels(source, new Size(width, height));
        }
    }
}
