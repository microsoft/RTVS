// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    /// <summary>
    /// A control that arranges Visual in grid
    /// </summary>
    internal sealed class VisualGrid : FrameworkElement {
        private readonly GridLineVisual _gridLine;
        private readonly VisualCollection _visualChildren;
        private SortOrder _sortOrder = new SortOrder();
        private GridRange _dataViewport;
        private Grid<TextVisual> _visualGrid;
        private GridIndex _selectedIndex;

        public VisualGrid() {
            _visualChildren = new VisualCollection(this);

            _gridLine = new GridLineVisual(this);
            AddLogicalChild(_gridLine);
            AddVisualChild(_gridLine);

            ClipToBounds = true;
            Focusable = true;
        }

        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(nameof(Header), typeof(bool), typeof(VisualGrid), new PropertyMetadata(false));

        /// <summary>
        /// If true, the object is assumed to be a grid header and clicking
        /// on fields changes sorting for the actual data grid.
        /// </summary>
        public bool Header {
            get => (bool)GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }

        /// <summary>
        /// Fires when sorting order changes
        /// </summary>
        public event EventHandler SortOrderChanged;

        public ScrollDirection ScrollDirection { get; set; }

        #region Font

        public static readonly DependencyProperty FontFamilyProperty = TextElement.FontFamilyProperty.AddOwner(typeof(VisualGrid),
                                                        new FrameworkPropertyMetadata(new FontFamily("Segoe UI"), FrameworkPropertyMetadataOptions.Inherits,
                                                        OnTypefaceParametersChanged));
        [Localizability(LocalizationCategory.Font)]
        public FontFamily FontFamily {
            get => (FontFamily)GetValue(FontFamilyProperty);
            set => SetValue(FontFamilyProperty, value);
        }

        public static readonly DependencyProperty FontSizeProperty = TextElement.FontSizeProperty.AddOwner(typeof(VisualGrid),
                                                       new FrameworkPropertyMetadata(12.0, FrameworkPropertyMetadataOptions.Inherits,
                                                       OnTypefaceParametersChanged));
        [TypeConverter(typeof(FontSizeConverter))]
        public double FontSize {
            get => (double)GetValue(FontSizeProperty);
            set => SetValue(FontSizeProperty, value);
        }

        public static readonly DependencyProperty FontWeightProperty = TextElement.FontWeightProperty.AddOwner(typeof(VisualGrid),
                                                       new FrameworkPropertyMetadata(FontWeights.Normal, FrameworkPropertyMetadataOptions.Inherits,
                                                       OnTypefaceParametersChanged));
        [TypeConverter(typeof(FontWeightConverter))]
        public FontWeight FontWeight {
            get => (FontWeight)GetValue(FontWeightProperty);
            set => SetValue(FontWeightProperty, value);
        }

        private static void OnTypefaceParametersChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((VisualGrid)d)._typeFace = null;

        private Typeface _typeFace;
        private bool _hasKeyboardFocus = false;

        private Typeface Typeface {
            get {
                if (_typeFace == null) {
                    if (FontFamily != null && FontSize > 0) {
                        try {
                            _typeFace = new Typeface(FontFamily, FontStyles.Normal, FontWeight, FontStretches.Normal);
                        } catch (ArgumentException) { }
                    }
                    _typeFace = _typeFace ?? new Typeface(new FontFamily("Segoe UI"), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);
                }
                return _typeFace;
            }
        }

        #endregion

        public double GridLineThickness => _gridLine.GridLineThickness;

        public void SetGridLineBrush(Brush brush) {
            _gridLine.GridLineBrush = brush;
        }

        public Brush Foreground { get; set; } = Brushes.Black;
        public Brush Background { get; set; } = Brushes.Transparent;
        public Brush SelectedForeground { get; set; } = Brushes.Black;
        public Brush SelectedBackground { get; set; } = Brushes.Transparent;

        public bool HasKeyboardFocus {
            get => _hasKeyboardFocus;
            set {
                if (value == _hasKeyboardFocus) {
                    return;
                }

                _hasKeyboardFocus = value;
                var visualGrid = _visualGrid;
                if (visualGrid != null && visualGrid.TryGet(_selectedIndex, out TextVisual visual)) {
                    visual.IsFocused = value;
                }
            }
        }

        public GridIndex SelectedIndex {
            get => _selectedIndex;
            set {
                if (value == _selectedIndex) {
                    return;
                }

                var visualGrid = _visualGrid;
                if (visualGrid != null && visualGrid.TryGet(_selectedIndex, out TextVisual visual)) {
                    visual.IsSelected = false;
                    visual.IsFocused = false;
                }

                _selectedIndex = value;

                if (visualGrid != null && visualGrid.TryGet(_selectedIndex, out visual)) {
                    visual.IsSelected = true;
                    visual.IsFocused = HasKeyboardFocus;
                }
            }
        }

        public void Clear() {
            _visualChildren.Clear();
            _sortOrder = new SortOrder();
        }

        public ISortOrder SortOrder => _sortOrder;

        internal void MeasurePoints(IPoints points, GridRange newViewport, IGrid<string> data, GridUpdateType updateType) {
            CreateGrid(newViewport, data, updateType);
            _visualChildren.Clear();

            foreach (int c in newViewport.Columns.GetEnumerable()) {
                foreach (int r in newViewport.Rows.GetEnumerable()) {
                    var visual = _visualGrid[r, c];

                    visual.Measure();
                    points.Width[c] = visual.Size.Width + visual.Margin * 2 + GridLineThickness;
                    points.Height[r] = visual.Size.Height + visual.Margin * 2 + GridLineThickness;

                    _visualChildren.Add(_visualGrid[r, c]);
                }
            }

            _dataViewport = newViewport;
        }

        private void CreateGrid(GridRange newViewport, IGrid<string> data, GridUpdateType updateType) {
            var orgGrid = _visualGrid;
            if (Header) {
                _visualGrid = new Grid<TextVisual>(
                        newViewport,
                        (r, c) => {
                            if (updateType == GridUpdateType.Sort) {
                                return orgGrid[r, c];
                            }
                            if (updateType != GridUpdateType.Refresh && _dataViewport.Contains(r, c)) {
                                return orgGrid[r, c];
                            }
                            var visual = new HeaderTextVisual(c);
                            InitVisual(r, c, data, visual);
                            return visual;
                        });
            } else {
                _visualGrid = new Grid<TextVisual>(
                    newViewport,
                    (r, c) => {
                        if (updateType != GridUpdateType.Refresh && _dataViewport.Contains(r, c)) {
                            return orgGrid[r, c];
                        }
                        var visual = new TextVisual();
                        InitVisual(r, c, data, visual);
                        return visual;
                    });
            }
        }

        private void InitVisual(long r, long c, IGrid<string> data, TextVisual visual) {
            visual.Row = r;
            visual.Column = c;
            visual.Text = data[r, c];
            visual.Typeface = Typeface;
            visual.FontSize = FontSize; // FontSize here is in device independent pixel, and Visual's FormattedText API uses the same unit
            visual.Background = Background;
            visual.Foreground = Foreground;
            visual.SelectedBackground = SelectedBackground;
            visual.SelectedForeground = SelectedForeground;
            visual.IsFocused = HasKeyboardFocus;
            visual.IsSelected = SelectedIndex.Row == r && SelectedIndex.Column == c;
        }

        internal void ArrangeVisuals(IPoints points) {
            foreach (int c in _dataViewport.Columns.GetEnumerable()) {
                foreach (int r in _dataViewport.Rows.GetEnumerable()) {
                    var visual = _visualGrid[r, c];

                    Debug.Assert(r == visual.Row && c == visual.Column);
                    Debug.Assert(points.Width[c] >= visual.Size.Width && points.Height[r] >= visual.Size.Height);

                    double cellX = points.xPosition[c];
                    double cellY = points.yPosition[r];
                    double cellWidth = points.Width[c];
                    double cellHeight = points.Height[r];

                    bool alignRight = visual.TextAlignment == TextAlignment.Right;
                    double x = cellX + (alignRight ? (cellWidth - visual.Size.Width - visual.Margin - GridLineThickness) : visual.Margin);
                    double y = cellY + visual.Margin;

                    var transform = visual.Transform as TranslateTransform;
                    if (transform == null) {
                        visual.Transform = new TranslateTransform(x, y);
                    } else {
                        transform.X = x;
                        transform.Y = y;
                    }

                    visual.X = x;
                    visual.Y = y;
                    visual.CellBounds = new Rect(cellX - x, cellY - y, cellWidth - 1, cellHeight - 1);
                    visual.Draw();
                }
            }

            // special handling for Row/Column header's size: this will layout system (measure/arrange) to know the size of component properly.
            if (ScrollDirection == ScrollDirection.Horizontal) {
                Height = points.Height[0];
            } else if (ScrollDirection == ScrollDirection.Vertical) {
                Width = points.Width[0];
            }

            _gridLine.Draw(_dataViewport, points);
        }

        protected override int VisualChildrenCount {
            get {
                if (_visualChildren.Count == 0) {
                    return 0;
                }

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

        public void ToggleSort(GridIndex index, bool add) {
            var visualGrid = _visualGrid;
            if (visualGrid == null || !visualGrid.TryGet(_selectedIndex, out TextVisual visual)) {
                return;
            }
            
            var headerVisual = (HeaderTextVisual)visual;
            // Order: None -> Ascending -> Descending -> Ascending -> Descending -> ...
            headerVisual.ToggleSortOrder();
            if (add) {
                // Shift+Click adds column to the sorting set.
                _sortOrder.Add(headerVisual);
            } else {
                // Clear all column sorts except the one that was clicked on.
                ResetSortToPrimary(headerVisual);
                _sortOrder.ResetTo(headerVisual);
            }
            SortOrderChanged?.Invoke(this, EventArgs.Empty);
        }

        private void ResetSortToPrimary(HeaderTextVisual primary) {
            foreach (var viz in _visualChildren) {
                var v = viz as HeaderTextVisual;
                if (v != null && v != primary) {
                    v.SortOrder = SortOrderType.None;
                }
            }
        }
    }
}
