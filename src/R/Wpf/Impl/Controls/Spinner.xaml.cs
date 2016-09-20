// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Media;

namespace Microsoft.R.Wpf.Controls {
    /// <summary>
    /// Interaction logic for Spinner.xaml
    /// </summary>
    public partial class Spinner : UserControl {
        public Spinner() {
            InitializeComponent();
        }

        public double ScaleX => ActualWidth / 120;
        public double ScaleY => ActualHeight / 120;
    }

    internal class SpinnerEllipseDetails {
        public double Width { get; set; }
        public double Height { get; set; }
        public double Left { get; set; }
        public double Top { get; set; }
        public Brush Fill { get; set; }
    }

    internal class SpinnerEllipseData : ObservableCollection<SpinnerEllipseDetails> {
        private static readonly double[] _leftCoordinates = { 20.1696, 2.86816, 5.03758e-006, 12.1203, 36.5459, 64.6723, 87.6176, 98.165, 92.9838, 47.2783 };
        private static readonly double[] _topCoordinates = { 9.76358, 29.9581, 57.9341, 83.3163, 98.138, 96.8411, 81.2783, 54.414, 26.9938, 0.5 };
        private static readonly int[] _opacities = { 0xE6, 0xCD, 0xB3, 0x9A, 0x80, 0x67, 0x4D, 0x34, 0x1A, 0xFF };
        private static readonly SolidColorBrush _indicatorFill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#007ACC"));

        public SpinnerEllipseData() {
            var baseColor = _indicatorFill.Color;
            for (var i = 0; i < _leftCoordinates.Length; i++) {
                Add(new SpinnerEllipseDetails {
                    Width = 21.835,
                    Height = 21.862,
                    Left = _leftCoordinates[i],
                    Top = _topCoordinates[i],
                    Fill = new SolidColorBrush(Color.FromArgb((byte)_opacities[i], baseColor.R, baseColor.G, baseColor.B))
                });
            }
        }
    }
}
