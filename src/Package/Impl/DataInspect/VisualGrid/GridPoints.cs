using System;
using System.Diagnostics;
using System.Windows;
using Microsoft.VisualStudio.PlatformUI;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
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

        public GridPoints(int rowCount, int columnCount, Size initialViewportSize) {
            Reset(rowCount, columnCount);

            _viewportHeight = Math.Max(MinItemHeight, initialViewportSize.Height);
            _viewportWidth = Math.Max(MinItemWidth, initialViewportSize.Width);
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

        public event EventHandler<PointChangedEventArgs> PointChanged;

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

                PointChanged(this, new PointChangedEventArgs(_scrolledDirection));
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
                if (newOffset > (VerticalExtent - ViewportHeight)) {
                    newOffset = Math.Max(0.0, VerticalExtent - ViewportHeight);
                }

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
                if (newOffset > (HorizontalExtent - ViewportWidth)) {
                    newOffset = Math.Max(0.0, HorizontalExtent - ViewportWidth);
                }

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

        private double _viewportHeight;
        public double ViewportHeight {
            get {
                return _viewportHeight;
            }
            set {
                _viewportHeight = value;
                _scrolledDirection |= ScrollDirection.Vertical;
            }
        }

        private double _viewportWidth;
        public double ViewportWidth {
            get {
                return _viewportWidth;
            }
            set {
                _viewportWidth = value;
                _scrolledDirection |= ScrollDirection.Horizontal;
            }
        }

        public IPoints GetAccessToPoints(ScrollDirection scrollDirection) {
            return new PointAccessor(this, scrollDirection);
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

        public int xIndex(double position) {
            EnsureXPositions();
            int index = Index(position, _xPositions);

            // _xPositions has one more item than columns
            return Math.Min(index, _columnCount - 1);
        }

        public int yIndex(double position) {
            EnsureYPositions();
            int index = Index(position, _yPositions);

            // _yPositions has one more item than rows
            return Math.Min(index, _rowCount - 1);
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

            Debug.Assert(HorizontalOffset >= _xPositions[columnStart]);
            Debug.Assert(VerticalOffset >= _yPositions[rowStart]);

            double width = _xPositions[columnStart] - HorizontalOffset;
            int columnCount = 0;
            for (int c = columnStart; c < _columnCount; c++) {
                width += GetWidth(c);
                columnCount++;
                if (width.GreaterThanOrClose(visualViewport.Width)) {
                    break;
                }
            }

            double height = _yPositions[rowStart] - VerticalOffset;
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

        #region internal utility class

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

        class PointAccessor : IPoints {
            public PointAccessor(GridPoints points, ScrollDirection scrollDirection) {
                if (scrollDirection == ScrollDirection.Horizontal) {
                    // column header
                    xPosition = new Indexer<double>(points.xPosition, NotSupportedSetter);
                    yPosition = new Indexer<double>((i) => (i == 0 ? 0.0 : points.ColumnHeight), NotSupportedSetter);
                    Width = new Indexer<double>(points.GetWidth, points.SetWidth);
                    Height = new Indexer<double>((i) => points.ColumnHeight, (i, v) => points.ColumnHeight = v);
                } else if (scrollDirection == ScrollDirection.Vertical) {
                    // row header
                    xPosition = new Indexer<double>((i) => (i == 0 ? 0.0 : points.RowWidth), NotSupportedSetter);
                    yPosition = new Indexer<double>(points.yPosition, NotSupportedSetter);
                    Width = new Indexer<double>((i) => points.RowWidth, (i, v) => points.RowWidth = v);
                    Height = new Indexer<double>(points.GetHeight, points.SetHeight);
                } else if (scrollDirection == ScrollDirection.Both) {
                    // data
                    xPosition = new Indexer<double>(points.xPosition, NotSupportedSetter);
                    yPosition = new Indexer<double>(points.yPosition, NotSupportedSetter);
                    Width = new Indexer<double>(points.GetWidth, points.SetWidth);
                    Height = new Indexer<double>(points.GetHeight, points.SetHeight);
                } else {
                    throw new NotSupportedException();
                }
            }

            public Indexer<double> Width { get; }

            public Indexer<double> Height { get; }

            public Indexer<double> xPosition { get; }

            public Indexer<double> yPosition { get; }

            private static void NotSupportedSetter(int index, double value) {
                throw new NotSupportedException("Setter is not supported");
            }
        }

        #endregion
    }
}
