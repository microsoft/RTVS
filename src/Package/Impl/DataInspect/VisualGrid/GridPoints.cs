using System;
using System.Windows;
using Microsoft.VisualStudio.PlatformUI;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    /// <summary>
    /// A utility class that contains cell width and height in a grid
    /// </summary>
    internal class GridPoints {
        #region fields and ctor

        private double[] _xPositions;
        private double[] _yPositions;
        private double[] _width;
        private double[] _height;

        private bool _xPositionValid;
        private bool _yPositionValid;

        public GridPoints(int rowCount, int columnCount) {
            RowCount = rowCount;
            ColumnCount = columnCount;

            _xPositions = new double[ColumnCount + 1];  // has one more item than the count
            _xPositionValid = false;
            _yPositions = new double[RowCount + 1]; // has one more item than the count
            _yPositionValid = false;
            _width = new double[ColumnCount];
            _height = new double[RowCount];

            InitializeWidthAndHeight();
        }

        #endregion

        private int RowCount { get; }

        private int ColumnCount { get; }

        public double MinItemWidth { get { return 20.0; } }

        public double MinItemHeight { get { return 10.0; } }

        /// <summary>
        /// total width of the grid
        /// </summary>
        public double Width {
            get {
                return _xPositions[ColumnCount];
            }
        }

        /// <summary>
        /// total height of the grid
        /// </summary>
        public double Height {
            get {
                return _yPositions[RowCount];
            }
        }

        private double _verticalOffset;
        public double VerticalOffset {
            get {
                return _verticalOffset;
            }
            set {
                double newOffset = value;
                if (newOffset < 0) newOffset = 0;
                if (newOffset > VerticalExtent) newOffset = VerticalExtent;

                if (!_verticalOffset.AreClose(newOffset)) {
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

                if (!_horizontalOffset.AreClose(newOffset)) {
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
            if (_width[xIndex].LessThan(value)) {
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
            if (_height[yIndex].LessThan(value)) {
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

        public GridRange ComputeDataViewport(Rect visualViewport) {
            int columnStart = xIndex(visualViewport.X);
            int rowStart = yIndex(visualViewport.Y);

            double width = 0.0;
            int columnCount = 0;
            for (int c = columnStart; c < ColumnCount; c++) {
                width += GetWidth(c);
                columnCount++;
                if (width.GreaterThanOrClose(visualViewport.Width)) {
                    break;
                }
            }

            double height = 0.0;
            int rowEnd = rowStart;
            int rowCount = 0;
            for (int r = rowStart; r < RowCount; r++) {
                height += GetHeight(r);
                rowCount++;
                if (height.GreaterThanOrClose(visualViewport.Height)) {
                    break;
                }
            }

            return new GridRange(
                new Range(rowStart, rowCount),
                new Range(columnStart, columnCount));
        }

        #region private

        private void ComputePositions() {
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

        private void ComputeXPositions() {
            double width = 0.0;
            for (int i = 0; i < ColumnCount; i++) {
                width += _width[i];
                _xPositions[i + 1] = width;
            }
            _xPositionValid = true;
        }

        #endregion
    }
}
