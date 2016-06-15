// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.R.Components.Plots.ViewModel;

namespace Microsoft.R.Components.Plots.Implementation.View {
    public partial class RPlotManagerControl : UserControl {
        // Anything below 200 pixels at fixed 96dpi is impractical, and prone to rendering errors
        private const int MinPixelWidth = 200;
        private const int MinPixelHeight = 200;

        public IRPlotManagerViewModel Model => DataContext as IRPlotManagerViewModel;

        public RPlotManagerControl() {
            InitializeComponent();
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e) {
            var unadjustedPixelSize = WpfUnitsConversion.ToPixels(Content as Visual, e.NewSize);

            // If the window gets below a certain minimum size, plot to the minimum size
            int pixelWidth = Math.Max((int)unadjustedPixelSize.Width, MinPixelWidth);
            int pixelHeight = Math.Max((int)unadjustedPixelSize.Height, MinPixelHeight);
            int resolution = WpfUnitsConversion.GetResolution(Content as Visual);
            plotImage.ToolTip = string.Format($"{pixelWidth} x {pixelHeight}px");

            Model?.ResizePlotAfterDelay(pixelWidth, pixelHeight, resolution);
        }

        private void Image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            var image = (FrameworkElement)sender;
            var pos = e.GetPosition(image);
            var pixelSize = WpfUnitsConversion.ToPixels(image as Visual, pos);

            Model?.ClickPlot((int)pixelSize.X, (int)pixelSize.Y);
        }
    }
}
