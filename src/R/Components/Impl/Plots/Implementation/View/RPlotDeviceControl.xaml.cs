// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Threading;
using Microsoft.R.Components.Plots.ViewModel;
using Microsoft.R.Host.Client;

namespace Microsoft.R.Components.Plots.Implementation.View {
    public partial class RPlotDeviceControl : UserControl {
        // Anything below 200 pixels at fixed 96dpi is impractical, and prone to rendering errors
        private const int MinPixelWidth = 200;
        private const int MinPixelHeight = 200;

        private readonly DelayedAsyncAction _resizeAction = new DelayedAsyncAction(250);
        private readonly DragSurface _dragSurface = new DragSurface();

        public IRPlotDeviceViewModel Model => DataContext as IRPlotDeviceViewModel;

        public event EventHandler<MouseButtonEventArgs> ContextMenuRequested;

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
            var unadjustedPixelSize = WpfUnitsConversion.ToPixels(Content as Visual, wpfSize);

            // If the window gets below a certain minimum size, plot to the minimum size
            int pixelWidth = Math.Max((int)unadjustedPixelSize.Width, MinPixelWidth);
            int pixelHeight = Math.Max((int)unadjustedPixelSize.Height, MinPixelHeight);
            int resolution = WpfUnitsConversion.GetResolution(Content as Visual);

            return new PlotDeviceProperties(pixelWidth, pixelHeight, resolution);
        }

        private void Image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            var image = (FrameworkElement)sender;
            var pos = e.GetPosition(image);
            var pixelSize = WpfUnitsConversion.ToPixels(image as Visual, pos);

            Model?.ClickPlot((int)pixelSize.X, (int)pixelSize.Y);
        }

        private void UserControl_Drop(object sender, DragEventArgs e) {
            var source = PlotClipboardData.Parse((string)e.Data.GetData(PlotClipboardData.Format));
            if (source != null) {
                bool isMove = (e.KeyStates & DragDropKeyStates.ShiftKey) != 0;
                try {
                    Model?.CopyPlotFromAsync(source.DeviceId, source.PlotId, isMove).DoNotWait();
                } catch (RPlotManagerException ex) {
                    MessageBox.Show(ex.Message, string.Empty, MessageBoxButton.OK, MessageBoxImage.Error);
                } catch (OperationCanceledException) {
                }
                e.Handled = true;
            }
        }

        private void UserControl_DragEnter(object sender, DragEventArgs e) {
            if (e.Data.GetDataPresent(PlotClipboardData.Format)) {
                var source = PlotClipboardData.Parse((string)e.Data.GetData(PlotClipboardData.Format));
                if (source != null) {
                    var targetDeviceId = Model?.Device.DeviceId;
                    if (targetDeviceId != source.DeviceId) {
                        bool isMove = (e.KeyStates & DragDropKeyStates.ShiftKey) != 0;
                        e.Effects = isMove ? DragDropEffects.Move : DragDropEffects.Copy;
                        e.Handled = true;
                        return;
                    }
                }
            }

            e.Effects = DragDropEffects.None;
            e.Handled = true;
        }

        private void UserControl_DragOver(object sender, DragEventArgs e) {
            UserControl_DragEnter(sender, e);
        }

        private void Image_MouseMove(object sender, MouseEventArgs e) {
            if (_dragSurface.IsMouseMoveStartingDrag(e)) {
                var data = PlotClipboardData.Serialize(new PlotClipboardData(Model.Device.DeviceId, Model.Device.ActivePlot.PlotId, false));
                var obj = new DataObject(PlotClipboardData.Format, data);
                DragDrop.DoDragDrop(this, obj, DragDropEffects.Copy | DragDropEffects.Move);
            }
        }

        private void Image_PreviewMouseDown(object sender, MouseButtonEventArgs e) {
            _dragSurface.MouseDown(e);
        }

        private void UserControl_MouseRightButtonUp(object sender, MouseButtonEventArgs e) {
            // TODO: also support keyboard (Shift+F10 or context menu key)
            ContextMenuRequested?.Invoke(sender, e);
        }
    }
}
