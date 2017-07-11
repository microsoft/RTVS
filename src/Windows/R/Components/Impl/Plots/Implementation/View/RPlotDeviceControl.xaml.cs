// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Threading;
using Microsoft.R.Components.Plots.ViewModel;
using Microsoft.R.Host.Client;

namespace Microsoft.R.Components.Plots.Implementation.View {
    public partial class RPlotDeviceControl {
        // Anything below 200 pixels at fixed 96dpi is impractical, and prone to rendering errors
        private const int MinPixelWidth = 200;
        private const int MinPixelHeight = 200;
        private const int DefaultResolution = 96;

        private readonly DelayedAsyncAction _resizeAction = new DelayedAsyncAction(250);
        private readonly DragSurface _dragSurface = new DragSurface();

        private int? _lastPixelWidth;
        private int? _lastPixelHeight;
        private int? _lastResolution;

        public IRPlotDeviceViewModel Model => DataContext as IRPlotDeviceViewModel;

        public event EventHandler<PointEventArgs> ContextMenuRequested;

        public RPlotDeviceControl() {
            InitializeComponent();
        }

        public PlotDeviceProperties GetPlotWindowProperties() {
            return GetPixelSizeAndResolution(new Size(ActualWidth, ActualHeight));
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e) {
            var props = GetPixelSizeAndResolution(e.NewSize);

            // Throttle rendering of plot while user is resizing the window.
            // Plot rendering isn't fast enough to keep up with live resizing,
            // which is what happens with undocked VS tool windows.
            var model = Model;
            if (model != null) {
                _resizeAction.Invoke(() => model.ResizePlotAsync(props.Width, props.Height, props.Resolution));
            }
        }

        private PlotDeviceProperties GetPixelSizeAndResolution(Size wpfSize) {
            var source = PresentationSource.FromVisual(Content as Visual);
            if (source != null) {
                var unadjustedPixelSize = WpfUnitsConversion.ToPixels(source, wpfSize);

                // If the window gets below a certain minimum size, plot to the minimum size
                _lastPixelWidth = Math.Max((int)unadjustedPixelSize.Width, MinPixelWidth);
                _lastPixelHeight = Math.Max((int)unadjustedPixelSize.Height, MinPixelHeight);
                _lastResolution = WpfUnitsConversion.GetResolution(source);
            }

            // The PresentationSource will be null in the specific scenario where:
            //   - Host is creating a graphics device, requesting properties of the plot window
            //   - This plot window is docked and has never been visible
            //
            // In that case, it is okay to re-use the last properties that we've
            // seen for this window. The window will have received the appropriate
            // SizeChanged events prior to this, and those were able to get a
            // non-null PresentationSource.
            //
            // I do not understand why, this might be some optimization that
            // VS/WPF is doing for hidden document windows.
            //
            // The fallback to the Minimum size when last properties are null 
            // is only there as a precaution, I have not seen it happen in my
            // testing. If they ever happen, you'll see a low resolution plot
            // when you activate the plot window.
            return new PlotDeviceProperties(_lastPixelWidth ?? MinPixelWidth, _lastPixelHeight ?? MinPixelHeight, _lastResolution ?? DefaultResolution);
        }

        private void Image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            var image = (FrameworkElement)sender;
            var pos = e.GetPosition(image);
            var pixelSize = WpfUnitsConversion.ToPixels(PresentationSource.FromVisual(image as Visual), pos);

            Model?.ClickPlot((int)pixelSize.X, (int)pixelSize.Y);
        }

        private void UserControl_Drop(object sender, DragEventArgs e) {
            var sources = PlotClipboardData.FromDataObject(e.Data).ToArray();
            if (sources.Length > 0) {
                var isMove = (e.KeyStates & DragDropKeyStates.ShiftKey) != 0;
                try {
                    foreach (var source in sources) {
                        Model?.CopyPlotFromAsync(source.DeviceId, source.PlotId, isMove).DoNotWait();
                    }
                } catch (RPlotManagerException ex) {
                    MessageBox.Show(ex.Message, string.Empty, MessageBoxButton.OK, MessageBoxImage.Error);
                } catch (OperationCanceledException) {
                }
                e.Handled = true;
            }
        }

        private void UserControl_DragEnter(object sender, DragEventArgs e) {
            var plots = PlotClipboardData.FromDataObject(e.Data);
            var targetDeviceId = Model?.Device?.DeviceId;
            if (targetDeviceId != null && plots.All(p => p.DeviceId == targetDeviceId)) {
                var isMove = (e.KeyStates & DragDropKeyStates.ShiftKey) != 0;
                e.Effects = isMove ? DragDropEffects.Move : DragDropEffects.Copy;
                e.Handled = true;
                return;
            }

            e.Effects = DragDropEffects.None;
            e.Handled = true;
        }

        private void UserControl_DragOver(object sender, DragEventArgs e) => UserControl_DragEnter(sender, e);

        private void Image_MouseMove(object sender, MouseEventArgs e) {
            if (_dragSurface.IsMouseMoveStartingDrag(e)) {
                var obj = PlotClipboardData.ToDataObject(Model.Device.DeviceId, Model.Device.ActivePlot.PlotId);
                DragDrop.DoDragDrop(this, obj, DragDropEffects.Copy | DragDropEffects.Move);
            } else {
                _dragSurface.MouseMove(e);
            }
        }

        private void Image_MouseLeave(object sender, MouseEventArgs e) => _dragSurface.MouseLeave(e);
        private void Image_PreviewMouseDown(object sender, MouseButtonEventArgs e) => _dragSurface.MouseDown(e);

        private void UserControl_MouseRightButtonUp(object sender, MouseButtonEventArgs e) {
            var point = e.GetPosition(this);
            ContextMenuRequested?.Invoke(this, new PointEventArgs(point));
        }
    }
}
