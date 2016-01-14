using System;
using System.Windows;
using Microsoft.VisualStudio.PlatformUI;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    internal class PointChangedEvent : EventArgs {
        public PointChangedEvent(ScrollDirection direction) {
            Direction = direction;
        }

        public ScrollDirection Direction { get; }
    }

    /// <summary>
    /// A utility class that contains cell width and height in a grid
    /// </summary>
    internal class GridPoints {
        #region fields and ctor

        private int _rowCount;
        private int _columnCount;

        private double[] _xPositions;
        private double[] _yPositions;
        private double[] _width;
        private double[] _height;

        private bool _xPositionValid;
        private bool _yPositionValid;

        public GridPoints(int rowCount, int columnCount) {
            Reset(rowCount, columnCount);
        }

        #endregion

        public void Reset(int rowCount, int columnCount) {
            _rowCount = rowCount;
            _columnCount = columnCount;

            _xPositions = new double[_columnCount + 1];  // has one more item than the count
            _xPositionValid = false;
            _yPositions = new double[_rowCount + 1]; // has one more item than the count
            _yPositionValid = false;
            _width = new double[_columnCount];
            _height = new double[_rowCount];

            _horizontalOffset = 0.0;
            _verticalOffset = 0.0;

            InitializeWidthAndHeight();
        }

        public event EventHandler<PointChangedEvent> PointChanged;

        private ScrollDirection _scrolledDirection = ScrollDirection.None;
        private void OnPointChanged() {
            if (_scrolledDirection != ScrollDirection.None
                && PointChanged != null) {

                if (_scrolledDirection.HasFlag(ScrollDirection.Horizontal)) {
                    EnsureXPositions();
                }
                if (_scrolledDirection.HasFlag(ScrollDirection.Vertical)) {
                    EnsureYPositions();
                }

                PointChanged(this, new PointChangedEvent(_scrolledDirection));
            }

            _scrolledDirection = ScrollDirection.None;
        }

        public DeferNotification DeferChangeNotification() {
            return new DeferNotification(this);
        }

        public static double MinItemWidth { get { return 20.0; } }

        public static double MinItemHeight { get { return 10.0; } }

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
                    _scrolledDirection |= ScrollDirection.Vertical;
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
                    _scrolledDirection |= ScrollDirection.Horizontal;
                }
            }
        }

        public double VerticalExtent {
            get {
                EnsureYPositions();
                return _yPositions[_rowCount];
            }
        }

        public double HorizontalExtent {
            get {
                EnsureXPositions();
                return _xPositions[_columnCount];
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
                _scrolledDirection |= ScrollDirection.Horizontal;
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
                _scrolledDirection |= ScrollDirection.Vertical;
            }
        }

        public double ColumnHeight { get; set; }

        public double RowWidth { get; set; }

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
            for (int i = 0; i < _columnCount; i++) {
                _width[i] = MinItemWidth;
            }

            for (int i = 0; i < _rowCount; i++) {
                _height[i] = MinItemHeight;
            }

            ColumnHeight = MinItemHeight;
            RowWidth = MinItemWidth;

            ComputePositions();
        }

        public GridRange ComputeDataViewport(Rect visualViewport) {
            int columnStart = xIndex(visualViewport.X);
            int rowStart = yIndex(visualViewport.Y);

            double width = 0.0;
            int columnCount = 0;
            for (int c = columnStart; c < _columnCount; c++) {
                width += GetWidth(c);
                columnCount++;
                if (width.GreaterThanOrClose(visualViewport.Width)) {
                    break;
                }
            }

            double height = 0.0;
            int rowEnd = rowStart;
            int rowCount = 0;
            for (int r = rowStart; r < _rowCount; r++) {
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
            for (int i = 0; i < _rowCount; i++) {
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
            for (int i = 0; i < _columnCount; i++) {
                width += _width[i];
                _xPositions[i + 1] = width;
            }
            _xPositionValid = true;
        }

        #endregion

        public class DeferNotification : IDisposable {
            private GridPoints _gridPoints;
            public DeferNotification(GridPoints gridPoints) {
                _gridPoints = gridPoints;
            }

            public void Dispose() {
                if (_gridPoints != null) {
                    _gridPoints.OnPointChanged();

                    _gridPoints = null;
                }
            }
        }
    }
}
