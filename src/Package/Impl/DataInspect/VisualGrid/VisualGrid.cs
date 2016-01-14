using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    /// <summary>
    /// A control that arranges Visual in grid
    /// </summary>
    internal class VisualGrid : FrameworkElement {
        private GridLineVisual _gridLine;
        private VisualCollection _visualChildren;
        private GridRange _dataViewport;
        private Grid<TextVisual> _visualGrid;

        public VisualGrid() {
            _visualChildren = new VisualCollection(this);
            _gridLine = new GridLineVisual(this);
            ClipToBounds = true;
        }

        internal MatrixView Owner { get; set; }

        public ScrollDirection ScrollDirection { get; set; }

        public GridPoints Points {
            get {
                return Owner.Points;
            }
        }

        #region Font

        public static readonly DependencyProperty FontFamilyProperty =
                TextElement.FontFamilyProperty.AddOwner(typeof(VisualGrid));

        [Localizability(LocalizationCategory.Font)]
        public FontFamily FontFamily {
            get { return (FontFamily)GetValue(FontFamilyProperty); }
            set {
                _typeFace = null;
                SetValue(FontFamilyProperty, value);
            }
        }

        public static readonly DependencyProperty FontSizeProperty =
                TextElement.FontSizeProperty.AddOwner(
                        typeof(VisualGrid));

        [TypeConverter(typeof(FontSizeConverter))]
        public double FontSize {
            get { return (double)GetValue(FontSizeProperty); }
            set { SetValue(FontSizeProperty, value); }
        }

        private Typeface _typeFace;
        private Typeface Typeface {
            get {
                if (_typeFace == null) {
                    // TODO: fall back when the specific typeface is not found
                    _typeFace = FontFamily.GetTypefaces().First(tf => tf.Style == FontStyles.Normal && tf.Weight == FontWeights.Normal && tf.Stretch == FontStretches.Normal);
                }
                return _typeFace;
            }
        }

        #endregion

        public double GridLineThickness {
            get {
                return _gridLine.GridLineThickness;
            }
        }

        public void SetGridLineBrush(Brush brush) {
            _gridLine.GridLineBrush = brush;
        }

        private Brush _foreground = Brushes.Black;
        public Brush Foreground {
            get {
                return _foreground;
            }
            set {
                _foreground = value;
            }
        }

        private Brush _background = Brushes.Transparent;
        public Brush Background {
            get {
                return _background;
            }
            set {
                _background = value;
            }
        }

        internal void MeasurePoints(GridRange newViewport, IGrid<string> data, bool refresh) {
            Debug.Assert(newViewport.Contains(data.Range));

            var orgGrid = _visualGrid;
            _visualGrid = new Grid<TextVisual>(
                newViewport,
                (r, c) => {
                    if (!refresh && _dataViewport.Contains(r, c)) {
                        return orgGrid[r, c];
                    }
                    var visual = new TextVisual();
                    visual.Row = r;
                    visual.Column = c;
                    visual.Text = data[r, c];
                    visual.Typeface = Typeface;
                    visual.FontSize = FontSize; // FontSize here is in device independent pixel, and Visual's FormattedText API uses the same unit
                    visual.Foreground = Foreground;
                    return visual;
                });

            _visualChildren.Clear();
            foreach (int c in newViewport.Columns.GetEnumerable()) {
                foreach (int r in newViewport.Rows.GetEnumerable()) {
                    var visual = _visualGrid[r, c];

                    double width = GetWidth(c) - GridLineThickness;
                    double height = GetHeight(r) - GridLineThickness;
                    visual.Draw(new Size(width, height));
                    SetWidth(c, Math.Max(width, visual.Size.Width + GridLineThickness));
                    SetHeight(r, Math.Max(height, visual.Size.Height + GridLineThickness));

                    _visualChildren.Add(_visualGrid[r, c]);
                }
            }

            _dataViewport = newViewport;
        }

        internal void ArrangeVisuals() {
            foreach (int c in _dataViewport.Columns.GetEnumerable()) {
                foreach (int r in _dataViewport.Rows.GetEnumerable()) {
                    var visual = _visualGrid[r, c];

                    var transform = visual.Transform as TranslateTransform;
                    if (transform == null) {
                        visual.Transform = new TranslateTransform(xPosition(visual), yPosition(visual));
                    } else {
                        transform.X = xPosition(visual);
                        transform.Y = yPosition(visual);
                    }
                }
            }

            // special handling for Row/Column header's size: this will layout system (measure/arrange) to know the size of component properly.
            if (ScrollDirection == ScrollDirection.Horizontal) {
                Height = GetHeight(0);
            } else if (ScrollDirection == ScrollDirection.Vertical) {
                Width = GetWidth(0);
            }

            DrawGridLine();
        }

        private double xPosition(TextVisual visual) {
            if (ScrollDirection == ScrollDirection.Vertical) {
                return 0.0;
            }

            return Points.xPosition(visual.Column);
        }

        private double yPosition(TextVisual visual) {
            if (ScrollDirection == ScrollDirection.Horizontal) {
                return 0.0;
            }

            return Points.yPosition(visual.Row);
        }

        private double _width = GridPoints.MinItemWidth;
        private double GetWidth(int column) {
            if (ScrollDirection == ScrollDirection.Vertical) {
                return _width;
            }

            return Points.GetWidth(column);
        }

        private void SetWidth(int column, double value) {
            if (ScrollDirection == ScrollDirection.Vertical) {
                _width = value;
            }

            Points.SetWidth(column, value);
        }

        private double _height = GridPoints.MinItemHeight;
        private double GetHeight(int row) {
            if (ScrollDirection == ScrollDirection.Horizontal) {
                return _height;
            }

            return Points.GetHeight(row);
        }

        private void SetHeight(int row, double value) {
            if (ScrollDirection == ScrollDirection.Horizontal) {
                _height = value;
            }

            Points.SetHeight(row, value);
        }

        private void DrawGridLine() {
            if (_gridLine == null) return;

            _gridLine.Draw(_dataViewport, Points);
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo) {
            base.OnRenderSizeChanged(sizeInfo);
        }

        protected override Size MeasureOverride(Size availableSize) {
            using (var elapsed = new Elapsed("Measure:")) {
                Size measured = base.MeasureOverride(availableSize);
                return measured;
            }
        }

        protected override Size ArrangeOverride(Size finalSize) {
            using (var elapsed = new Elapsed("Arrange:")) {
                return base.ArrangeOverride(finalSize);
            }
        }

        protected override void OnRender(DrawingContext drawingContext) {
            base.OnRender(drawingContext);

            drawingContext.DrawRectangle(Background, null, new Rect(RenderSize));
        }

        protected override int VisualChildrenCount {
            get {
                if (_visualChildren.Count == 0) return 0;
                return _visualChildren.Count + 1;
            }
        }

        protected override Visual GetVisualChild(int index) {
            if (index < 0 || index >= _visualChildren.Count + 1)
                throw new ArgumentOutOfRangeException("index");
            if (index == 0) return _gridLine;
            return _visualChildren[index - 1];
        }
    }

    /// <summary>
    /// Scroll orientation of Grid
    /// </summary>
    [Flags]
    internal enum ScrollDirection {
        /// <summary>
        /// grid doesn't scroll
        /// </summary>
        None = 0x00,

        /// <summary>
        /// grid scrolls horizontally
        /// </summary>
        Horizontal = 0x01,

        /// <summary>
        /// grid scrolls vertically
        /// </summary>
        Vertical = 0x02,

        /// <summary>
        /// grid scrolls in vertical and horizontal direction
        /// </summary>
        Both = 0x03,
    }
}
