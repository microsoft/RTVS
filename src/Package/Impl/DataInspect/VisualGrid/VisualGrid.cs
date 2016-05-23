// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    /// <summary>
    /// A control that arranges Visual in grid
    /// </summary>
    internal class VisualGrid : FrameworkElement {
        private readonly GridLineVisual _gridLine;
        private readonly VisualCollection _visualChildren;
        private GridRange _dataViewport;
        private Grid<TextVisual> _visualGrid;

        public VisualGrid() {
            _visualChildren = new VisualCollection(this);

            _gridLine = new GridLineVisual(this);
            AddLogicalChild(_gridLine);
            AddVisualChild(_gridLine);

            ClipToBounds = true;
            Focusable = true;
        }

        /// <summary>
        /// If true, the object is assumed to be a grid header and clicking
        /// on fields changes sorting for the actual data grid.
        /// </summary>
        public bool Header { get; set; }

        /// <summary>
        /// If sorting is enabled, defines sort direction
        /// </summary>
        public ListSortDirection SortDirection { get; set; }

        public ScrollDirection ScrollDirection { get; set; }

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
                    _typeFace = ChooseTypeface();
                }
                return _typeFace;
            }
        }

        private Typeface ChooseTypeface() {
            if (ScrollDirection == ScrollDirection.Vertical
                || ScrollDirection == ScrollDirection.Horizontal) {
                // TODO: fall back
                return FontFamily.GetTypefaces().First(tf => tf.Style == FontStyles.Normal && tf.Weight == FontWeights.DemiBold && tf.Stretch == FontStretches.Normal);
            } else {
                return FontFamily.GetTypefaces().First(tf => tf.Style == FontStyles.Normal && tf.Weight == FontWeights.Normal && tf.Stretch == FontStretches.Normal);
            }
        }

        #endregion

        public double GridLineThickness => _gridLine.GridLineThickness;

        public void SetGridLineBrush(Brush brush) {
            _gridLine.GridLineBrush = brush;
        }

        public Brush Foreground { get; set; } = Brushes.Black;
        public Brush Background { get; set; } = Brushes.Transparent;

        internal void MeasurePoints(IPoints points, GridRange newViewport, IGrid<string> data, bool refresh) {
            CreateGrid(newViewport, data, refresh);
            _visualChildren.Clear();

            foreach (int c in newViewport.Columns.GetEnumerable()) {
                foreach (int r in newViewport.Rows.GetEnumerable()) {
                    var visual = _visualGrid[r, c];

                    visual.Draw();
                    points.Width[c] = visual.Size.Width + (visual.Margin * 2) + GridLineThickness;
                    points.Height[r] = visual.Size.Height + (visual.Margin * 2) + GridLineThickness;

                    _visualChildren.Add(_visualGrid[r, c]);
                }
            }

            _dataViewport = newViewport;
        }

        private void CreateGrid(GridRange newViewport, IGrid<string> data, bool refresh) {
            var orgGrid = _visualGrid;
            if (Header) {
                _visualGrid = new Grid<TextVisual>(
                        newViewport,
                        (r, c) => {
                            if (!refresh && _dataViewport.Contains(r, c)) {
                                return orgGrid[r, c];
                            }
                            var visual = new HeaderTextVisual();
                            visual.Row = r;
                            visual.Column = c;
                            visual.Text = data[r, c];
                            visual.Typeface = Typeface;
                            visual.FontSize = FontSize; // FontSize here is in device independent pixel, and Visual's FormattedText API uses the same unit
                            visual.Foreground = Foreground;
                            return visual;
                        });
            } else {
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
            }
        }

        private bool alignRight = true;

        internal void ArrangeVisuals(IPoints points) {
            foreach (int c in _dataViewport.Columns.GetEnumerable()) {
                foreach (int r in _dataViewport.Rows.GetEnumerable()) {
                    var visual = _visualGrid[r, c];

                    Debug.Assert(r == visual.Row && c == visual.Column);
                    Debug.Assert(points.Width[c] >= visual.Size.Width && points.Height[r] >= visual.Size.Height);

                    double x = points.xPosition[c] + (alignRight ? (points.Width[c] - visual.Size.Width - visual.Margin - GridLineThickness) : 0.0);
                    double y = points.yPosition[r];

                    var transform = visual.Transform as TranslateTransform;
                    if (transform == null) {
                        visual.Transform = new TranslateTransform(x, y);
                    } else {
                        transform.X = x;
                        transform.Y = y;
                    }

                    visual.X = x;
                    visual.Y = y;
                }
            }

            // special handling for Row/Column header's size: this will layout system (measure/arrange) to know the size of component properly.
            if (ScrollDirection == ScrollDirection.Horizontal) {
                Height = points.Height[0];
            } else if (ScrollDirection == ScrollDirection.Vertical) {
                Width = points.Width[0];
            }

            _gridLine?.Draw(_dataViewport, points);
        }

        protected override void OnRender(DrawingContext drawingContext) {
            base.OnRender(drawingContext);
            drawingContext.DrawRectangle(Background, null, new Rect(RenderSize));
        }

        public void Clear() => _visualChildren.Clear();

        protected override int VisualChildrenCount {
            get {
                if (_visualChildren.Count == 0) return 0;
                return _visualChildren.Count + 1;
            }
        }

        protected override Visual GetVisualChild(int index) {
            if (index < 0 || index >= _visualChildren.Count + 1) {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            if (index == 0) {
                return _gridLine;
            }
            return _visualChildren[index - 1];
        }

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e) {
            if (Header) {
                var pt = e.GetPosition(this);
                for (int i = 0; i < VisualChildrenCount; i++) {
                    var v = GetVisualChild(i) as HeaderTextVisual;
                    if (v != null) {
                        Rect rc = new Rect(v.X, v.Y, v.Size.Width, v.Size.Height);
                        if (rc.Contains(pt)) {
                            v.ToggleSortOrder();
                            break;
                        }
                    }
                }
            }
            // Find out which visual is it
            base.OnPreviewMouseDown(e);
        }
    }
}
