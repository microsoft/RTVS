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

        public DeferNotification DeferChangeNotification(bool suppressNotification) {
            return new DeferNotification(this, suppressNotification);
        }

        public static double MinItemWidth { get { return 40.0; } }

        public static double MinItemHeight { get { return 10.0; } }


        public double AverageItemHeight {
            get {
                Debug.Assert(_rowCount != 0);
                return VerticalExtent / _rowCount;
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

        public double VerticalComputedOffset {
            get {
                return VerticalOffset;
            }
            set {
                VerticalOffset = value;
            }
        }

        public double HorizontalComputedOffset {
            get {
                return HorizontalOffset;
            }
            set {
                HorizontalOffset = value;
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
                if (!LayoutDoubleUtil.AreClose(_viewportHeight, value)) {
                    _viewportHeight = value;
                    _scrolledDirection |= ScrollDirection.Vertical;
                }
            }
        }

        private double _viewportWidth;
        public double ViewportWidth {
            get {
                return _viewportWidth;
            }
            set {
                if (!LayoutDoubleUtil.AreClose(_viewportWidth, value)) {
                    _viewportWidth = value;
                    _scrolledDirection |= ScrollDirection.Horizontal;
                }
            }
        }

        public IPoints GetAccessToPoints(ScrollDirection scrollDirection) {
            return new PointAccessor(this, scrollDirection);
        }

        public double xPosition(int xIndex) {
            EnsureXPositions();
            return _xPositions[xIndex] - HorizontalComputedOffset;
        }

        public double yPosition(int yIndex) {
            EnsureYPositions();
            return _yPositions[yIndex] - VerticalComputedOffset;
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

        private double _columnHeaderHeight;
        public double ColumnHeaderHeight {
            get {
                return _columnHeaderHeight;
            }
            set {
                if (_columnHeaderHeight.LessThan(value)) {
                    _columnHeaderHeight = value;
                }
            }
        }

        private double _rowHeaderWidth;
        public double RowHeaderWidth {
            get {
                return _rowHeaderWidth;
            }
            set {
                if (_rowHeaderWidth.LessThan(value)) {
                    _rowHeaderWidth = value;
                }
            }
        }

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

            ColumnHeaderHeight = MinItemHeight;
            RowHeaderWidth = MinItemWidth;

            ComputePositions();
        }

        public GridRange ComputeDataViewport(Rect visualViewport, ref ScrollDirection overflow) {
            int columnStart = xIndex(visualViewport.X);
            int rowStart = yIndex(visualViewport.Y);

            Debug.Assert(HorizontalComputedOffset >= _xPositions[columnStart]);
            Debug.Assert(VerticalComputedOffset >= _yPositions[rowStart]);

            double width = _xPositions[columnStart] - HorizontalComputedOffset;
            int columnCount = 0;
            for (int c = columnStart; c < _columnCount; c++) {
                width += GetWidth(c);
                columnCount++;
                if (width.GreaterThanOrClose(visualViewport.Width)) {
                    break;
                }
            }

            if (width.LessThan(visualViewport.Width)) {
                for (int c = columnStart - 1; c >= 0; c--) {
                    overflow |= ScrollDirection.Horizontal;
                    width += GetWidth(c);
                    if (width.GreaterThanOrClose(visualViewport.Width)) {
                        break;
                    }
                }
            }

            double height = _yPositions[rowStart] - VerticalComputedOffset;
            int rowEnd = rowStart;
            int rowCount = 0;
            for (int r = rowStart; r < _rowCount; r++) {
                height += GetHeight(r);
                rowCount++;
                if (height.GreaterThanOrClose(visualViewport.Height)) {
                    break;
                }
            }

            if (height.LessThan(visualViewport.Height)) {
                for (int r = rowStart - 1; r >= 0; r--) {
                    overflow |= ScrollDirection.Vertical;
                    height += GetHeight(r);
                    if (height.GreaterThanOrClose(visualViewport.Height)) {
                        break;
                    }
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
            private bool _suppressNotification;

            public DeferNotification(GridPoints gridPoints, bool suppressNotification) {
                _gridPoints = gridPoints;
                _suppressNotification = suppressNotification;
            }

            public void Dispose() {
                if (!_suppressNotification && _gridPoints != null) {
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
                    yPosition = new Indexer<double>((i) => (i == 0 ? 0.0 : points.ColumnHeaderHeight), NotSupportedSetter);
                    Width = new Indexer<double>(points.GetWidth, points.SetWidth);
                    Height = new Indexer<double>((i) => points.ColumnHeaderHeight, (i, v) => points.ColumnHeaderHeight = v);
                } else if (scrollDirection == ScrollDirection.Vertical) {
                    // row header
                    xPosition = new Indexer<double>((i) => (i == 0 ? 0.0 : points.RowHeaderWidth), NotSupportedSetter);
                    yPosition = new Indexer<double>(points.yPosition, NotSupportedSetter);
                    Width = new Indexer<double>((i) => points.RowHeaderWidth, (i, v) => points.RowHeaderWidth = v);
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
