using System;
using System.Windows;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    internal class GridPoints {
        private double[] _xPositions;
        private double[] _yPositions;
        private double[] _width;
        private double[] _height;

        private bool _xPositionValid;
        private bool _yPositionValid;

        public GridPoints(int rowCount, int columnCount) {
            RowCount = rowCount;
            ColumnCount = columnCount;

            _xPositions = new double[ColumnCount + 1];
            _xPositionValid = false;
            _yPositions = new double[RowCount + 1];
            _yPositionValid = false;
            _width = new double[ColumnCount];
            _height = new double[RowCount];

            MinItemWidth = 20.0;
            MinItemHeight = 10.0;

            InitializeWidthAndHeight();
        }

        private int RowCount { get; }

        private int ColumnCount { get; }

        public double MinItemWidth { get; set; }

        public double MinItemHeight { get; set; }

        private double _verticalOffset;
        public double VerticalOffset {
            get {
                return _verticalOffset;
            }
            set {
                double newOffset = value;
                if (newOffset < 0) newOffset = 0;
                if (newOffset > VerticalExtent) newOffset = VerticalExtent;

                if (_verticalOffset != newOffset) { // TODO: double util
                    _verticalOffset = newOffset;
                }
            }
        }

        private double _horizontalOffset;
        public double HorizontalOffset {
            get {
                return _horizontalOffset;
            }
            set {
                double newOffset = value;
                if (newOffset < 0) newOffset = 0;
                if (newOffset > HorizontalExtent) newOffset = HorizontalExtent;

                if (_horizontalOffset != newOffset) {   // TODO: double util
                    _horizontalOffset = newOffset;
                }
            }
        }

        public double VerticalExtent {
            get {
                EnsureYPositions();
                return _yPositions[RowCount];
            }
        }

        public double HorizontalExtent {
            get {
                EnsureXPositions();
                return _xPositions[ColumnCount];
            }
        }

        public double xPosition(int xIndex) {
            EnsureXPositions();
            return _xPositions[xIndex] - HorizontalOffset;
        }

        public double yPosition(int yIndex) {
            EnsureYPositions();
            return _yPositions[yIndex] - VerticalOffset;
        }

        public double GetWidth(int columnIndex) {
            return _width[columnIndex];
        }

        public void SetWidth(int xIndex, double value) {
            if (_width[xIndex] < value) {   // TODO: double util
                _width[xIndex] = value;
                _xPositionValid = false;
            }
        }

        public double GetWidth(Range range) {
            return Size(range, _xPositions);
        }

        public double GetHeight(int rowIndex) {
            return _height[rowIndex];
        }

        public void SetHeight(int yIndex, double value) {
            if (_height[yIndex] < value) {  // TODO: double util
                _height[yIndex] = value;
                _yPositionValid = false;
            }
        }

        public double GetHeight(Range range) {
            return Size(range, _yPositions);
        }

        private double Size(Range range, double[] positions) {
            return positions[range.Start + range.Count] - positions[range.Start];
        }

        public int xIndex(double position) {
            EnsureXPositions();
            return Index(position, _xPositions);
        }

        public int yIndex(double position) {
            EnsureYPositions();
            return Index(position, _yPositions);
        }

        private int Index(double position, double[] positions) {
            int index = Array.BinarySearch(positions, position);
            return (index < 0) ? (~index) - 1 : index;
        }

        private void InitializeWidthAndHeight() {
            for (int i = 0; i < ColumnCount; i++) {
                _width[i] = MinItemWidth;
            }

            for (int i = 0; i < RowCount; i++) {
                _height[i] = MinItemHeight;
            }

            ComputePositions();
        }

        public void ComputePositions() {
            ComputeYPositions();
            ComputeXPositions();
        }

        private void EnsureYPositions() {
            if (!_yPositionValid) {
                ComputeYPositions();
            }
        }

        private void ComputeYPositions() {
            double height = 0.0;
            for (int i = 0; i < RowCount; i++) {
                height += _height[i];
                _yPositions[i + 1] = height;
            }
            _yPositionValid = true;
        }

        private void EnsureXPositions() {
            if (!_xPositionValid) {
                ComputeXPositions();
            }
        }

        public void ComputeXPositions() {
            double width = 0.0;
            for (int i = 0; i < ColumnCount; i++) {
                width += _width[i];
                _xPositions[i + 1] = width;
            }
            _xPositionValid = true;
        }

        public GridRange ComputeDataViewport(Rect visualViewport) {
            int columnStart = xIndex(visualViewport.X);
            int rowStart = yIndex(visualViewport.Y);

            double width = 0.0;
            int columnCount = 0;
            for (int c = columnStart; c < ColumnCount; c++) {
                width += GetWidth(c);
                columnCount++;
                if (width >= visualViewport.Width) {    // TODO: DoubleUtil
                    break;
                }
            }

            double height = 0.0;
            int rowEnd = rowStart;
            int rowCount = 0;
            for (int r = rowStart; r < RowCount; r++) {
                height += GetHeight(r);
                rowCount++;
                if (height >= visualViewport.Height) {    // TODO: DoubleUtil
                    break;
                }
            }

            return new GridRange(
                new Range(rowStart, rowCount),
                new Range(columnStart, columnCount));
        }
    }
}
