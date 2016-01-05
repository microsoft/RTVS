using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    internal enum GridType {
        Data,
        ColumnHeader,
        RowHeader,
    }

    internal class VisualGrid : FrameworkElement {
        private GridLineVisual _gridLine;
        private VisualCollection _visualChildren;
        private GridRange _dataViewport;
        private Grid<TextVisual> _visualGrid;

        public VisualGrid() {
            _visualChildren = new VisualCollection(this);
            _gridLine = new GridLineVisual();
            ClipToBounds = true;
        }

        public GridType GridType { get; set; }

        public IGridProvider<string> DataProvider { get; set; }

        public GridPoints Points { get; set; }

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
                    _typeFace = FontFamily.GetTypefaces().First(tf => tf.Style == FontStyles.Normal && tf.Weight == FontWeights.Normal);
                }
                return _typeFace;
            }
        }

        #endregion

        public int RowCount { get; set; }

        public int ColumnCount { get; set; }

        public double GridLineThickness { get { return _gridLine.GridLineThickness; } }

        private double HorizontalOffset {
            get {
                if (GridType == GridType.RowHeader) {
                    return 0.0;
                }
                return Points.HorizontalOffset;
            }
        }

        private double VerticalOffset {
            get {
                if (GridType == GridType.ColumnHeader) {
                    return 0.0;
                }
                return Points.VerticalOffset;
            }
        }

        internal void DrawVisuals(GridRange newViewport, IGrid<string> data) {
            DrawCells(newViewport, data);

            DrawGridLine();
        }

        private void DrawCells(GridRange newViewport, IGrid<string> data) {
            var orgGrid = _visualGrid;
            _visualGrid = new Grid<TextVisual>(
                newViewport,
                (r, c) => {
                    if (_dataViewport.Contains(r, c)) {
                        return orgGrid[r, c];
                    }
                    var visual = new TextVisual();
                    visual.Row = r;
                    visual.Column = c;
                    visual.Text = data[r, c];
                    visual.Typeface = Typeface;
                    visual.FontSize = FontSize * (96.0 / 72.0);  // TODO: test in High DPI
                    return visual;
                });

            _visualChildren.Clear();
            foreach (int c in newViewport.Columns.GetEnumerable()) {
                foreach (int r in newViewport.Rows.GetEnumerable()) {
                    var visual = _visualGrid[r, c];

                    double width = Points.GetWidth(c) - GridLineThickness;
                    double height = Points.GetHeight(r) - GridLineThickness;
                    if (visual.Draw(new Size(width, height))) {
                        Points.SetWidth(c, Math.Max(width, visual.Size.Width + GridLineThickness));
                        Points.SetHeight(r, Math.Max(height, visual.Size.Height + GridLineThickness));
                    }

                    _visualChildren.Add(_visualGrid[r, c]);
                }
            }

            foreach (int c in newViewport.Columns.GetEnumerable()) {
                foreach (int r in newViewport.Rows.GetEnumerable()) {
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
            _dataViewport = newViewport;

            // special handling for Row/Column header's size: this will layout system (measure/arrange) to know the size of component properly.
            if (GridType == GridType.ColumnHeader && RowCount > 0) {
                Height = Points.GetHeight(0);
            } else if (GridType == GridType.RowHeader && ColumnCount > 0) {
                Width = Points.GetWidth(0);
            }
        }

        private double xPosition(TextVisual visual) {
            if (GridType == GridType.RowHeader) {
                return 0.0;
            }

            return Points.xPosition(visual.Column);
        }

        private double yPosition(TextVisual visual) {
            if (GridType == GridType.ColumnHeader) {
                return 0.0;
            }

            return Points.yPosition(visual.Row);
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
}
