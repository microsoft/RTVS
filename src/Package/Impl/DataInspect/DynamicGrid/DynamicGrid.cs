using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Microsoft.VisualStudio.R.Package.Wpf;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    public class DynamicGrid : MultiSelector {
        private LinkedList<DynamicGridRow> _realizedRows = new LinkedList<DynamicGridRow>();

        static DynamicGrid() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DynamicGrid), new FrameworkPropertyMetadata(typeof(DynamicGrid)));
        }

        #region override

        protected override DependencyObject GetContainerForItemOverride() {
            return new DynamicGridRow();
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item) {
            base.PrepareContainerForItemOverride(element, item);

            DynamicGridRow row = (DynamicGridRow)element;
            if (!_realizedRows.Equals(row.Track.List)) {
                _realizedRows.AddFirst(row.Track);
            } else {
                Debug.Fail("hey");
            }
            row.Header = RowHeaderSource[Items.IndexOf(item)];
            row.Prepare(this, item);
        }

        protected override void ClearContainerForItemOverride(DependencyObject element, object item) {
            base.ClearContainerForItemOverride(element, item);

            DynamicGridRow row = (DynamicGridRow)element;

            row.Header = null;

            _realizedRows.Remove(row.Track);
            row.CleanUp(this, item);
        }

        protected override Size MeasureOverride(Size constraint) {
            EnsureHorizontalScrollbar();

            return base.MeasureOverride(constraint);
        }

        #endregion override

        #region RowHeader

        public double RowHeaderActualWidth {
            get { return (double)GetValue(RowHeaderActualWidthProperty); }
            internal set { SetValue(RowHeaderActualWidthPropertyKey, value); }
        }

        private static readonly DependencyPropertyKey RowHeaderActualWidthPropertyKey =
            DependencyProperty.RegisterReadOnly("RowHeaderActualWidth", typeof(double), typeof(DynamicGrid), new FrameworkPropertyMetadata(0.0, new PropertyChangedCallback(OnNotifyRowHeaderPropertyChanged)));

        public static readonly DependencyProperty RowHeaderActualWidthProperty = RowHeaderActualWidthPropertyKey.DependencyProperty;

        private static void OnNotifyRowHeaderPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            ((DynamicGrid)d).OnNotifyRowHeaderPropertyChanged();
        }

        private void OnNotifyRowHeaderPropertyChanged() {
            foreach (var row in _realizedRows) {
                row.NotifyRowHeader();
            }
        }

        #endregion

        #region ColumnHeader

        private DynamicGridColumnHeadersPresenter _columnHeadersPresenter;

        internal DynamicGridColumnHeadersPresenter ColumnHeadersPresenter {
            get {
                return _columnHeadersPresenter;
            }
            set {
                _columnHeadersPresenter = value;
                _columnHeadersPresenter.ItemsSource = ColumnHeaderSource;
            }
        }

        #endregion

        #region Column

        private SortedList<int, DynamicGridStripe> _columns = new SortedList<int, DynamicGridStripe>();
        internal DynamicGridStripe GetColumn(int index) {
            DynamicGridStripe stack;
            if (_columns.TryGetValue(index, out stack)) {
                return stack;
            }

            stack = new DynamicGridStripe(Orientation.Vertical, index);
            _columns.Add(index, stack);

            return stack;
        }

        private const double EstimatedWidth = 20.0; // TODO: configurable

        Size _panelSize;
        LayoutInfo _layoutInfo;
        internal void OnReportPanelSize(Size size) {
            _panelSize = size;

            int horizontalOffset = (int)HorizontalOffset;

            double viewportWidth = Math.Ceiling(size.Width / EstimatedWidth);

            ViewportWidth = viewportWidth;
            ScrollableWidth = ExtentWidth - viewportWidth;

            var newLayoutInfo = new LayoutInfo() {
                FirstItemIndex = horizontalOffset,
                FirstItemOffset = 0.0,
                ItemCountInViewport = (int)viewportWidth,
            };

            if (!_layoutInfo.Equals(newLayoutInfo)) {
                _layoutInfo = newLayoutInfo;

                if (_columnHeadersPresenter != null) {
                    _columnHeadersPresenter.ScrollChanged();
                }

                // TODO: move to background(?)
                var toRemove = _columns.Where(c => c.Key < _layoutInfo.FirstItemIndex || c.Key >= (_layoutInfo.FirstItemIndex + _layoutInfo.ItemCountInViewport)).ToList();
                foreach (var item in toRemove) {
                    _columns.Remove(item.Key);
                }
            }
        }

        internal LayoutInfo GetLayoutInfo(Size size) {
            return _layoutInfo;
        }

        public static readonly DependencyProperty ScrollableWidthProperty =
                DependencyProperty.Register(
                        "ScrollableWidth",
                        typeof(double),
                        typeof(DynamicGrid),
                        new FrameworkPropertyMetadata(0d));

        public double ScrollableWidth {
            get {
                return (double) GetValue(ScrollableWidthProperty);
            }
            set {
                SetValue(ScrollableWidthProperty, value);
            }
        }

        public static readonly DependencyProperty ExtentWidthProperty =
                DependencyProperty.Register(
                        "ExtentWidth",
                        typeof(double),
                        typeof(DynamicGrid),
                        new FrameworkPropertyMetadata(0d));

        public double ExtentWidth {
            get {
                return (double)GetValue(ExtentWidthProperty);
            }
            set {
                SetValue(ExtentWidthProperty, value);
            }
        }

        public static readonly DependencyProperty HorizontalOffsetProperty =
                DependencyProperty.Register(
                        "HorizontalOffset",
                        typeof(double),
                        typeof(DynamicGrid),
                        new FrameworkPropertyMetadata(0d));

        public double HorizontalOffset {
            get {
                return (double)GetValue(HorizontalOffsetProperty);
            }
            set {
                SetValue(HorizontalOffsetProperty, value);
            }
        }

        public static readonly DependencyProperty ViewportWidthProperty =
                DependencyProperty.Register(
                        "ViewportWidth",
                        typeof(double),
                        typeof(DynamicGrid),
                        new FrameworkPropertyMetadata(0d));

        public double ViewportWidth {
            get {
                return (double)GetValue(ViewportWidthProperty);
            }
            set {
                SetValue(ViewportWidthProperty, value);
            }
        }

        private bool _scrollbar = false;
        private void EnsureHorizontalScrollbar() {
            if (!_scrollbar) {

                var scrollbar = WpfHelper.FindChild<ScrollBar>(this, (bar) => bar.Name == "HorizontalScrollBar");
                if (scrollbar != null) {
                    scrollbar.Scroll += Scrollbar_Scroll; ;
                }

                _scrollbar = true;
            }
        }

        private void Scrollbar_Scroll(object sender, ScrollEventArgs e) {
            if (e.ScrollEventType == ScrollEventType.EndScroll) {
                OnReportPanelSize(_panelSize);  // refresh viewport

                HorizontalOffset = e.NewValue;

                // same calls in OnReportPanelSize
                //if (_columnHeadersPresenter != null) {
                //    _columnHeadersPresenter.ScrollChanged();
                //}

                foreach (var row in _realizedRows) {
                    row.ScrollChanged();
                }
            }
        }

        #endregion

        public IList RowHeaderSource { get; set; }

        public IList ColumnHeaderSource { get; set; }
    }
}
