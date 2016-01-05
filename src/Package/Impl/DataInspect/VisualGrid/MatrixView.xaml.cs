using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    public partial class MatrixView : UserControl {
        private GridPoints _gridPoints;
        private VisualGridScroller _scroller;

        public MatrixView() {
            InitializeComponent();
        }

        public void Initialize(IGridProvider<string> dataProvider) {
            _scroller = new VisualGridScroller();

            DataProvider = dataProvider;

            _gridPoints = new GridPoints(RowCount, ColumnCount);

            RowHeader.RowCount = RowCount;
            RowHeader.ColumnCount = 1;
            RowHeader.Points = _gridPoints;
            RowHeader.DataProvider = DataProvider;

            ColumnHeader.RowCount = 1;
            ColumnHeader.ColumnCount = ColumnCount;
            ColumnHeader.Points = _gridPoints;
            ColumnHeader.DataProvider = DataProvider;

            Data.RowCount = RowCount;
            Data.ColumnCount = ColumnCount;
            Data.Points = _gridPoints;
            Data.DataProvider = DataProvider;

            _scroller.Points = _gridPoints;
            _scroller.ColumnHeader = ColumnHeader;
            _scroller.RowHeader = RowHeader;
            _scroller.DataGrid = Data;
            _scroller.DataProvider = DataProvider;
        }

        private IGridProvider<string> _dataProvider;
        public IGridProvider<string> DataProvider {
            get {
                return _dataProvider;
            }
            set {
                _dataProvider = value;
            }
        }

        public int RowCount {
            get {
                return _dataProvider.RowCount;
            }
        }

        public int ColumnCount {
            get {
                return _dataProvider.ColumnCount;
            }
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo) {
            base.OnRenderSizeChanged(sizeInfo);

            // null if not Initialized yet
            if (_scroller != null) {
                _scroller.EnqueueCommand(ScrollType.SizeChange, Data.RenderSize);
            }
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e) {
            if (e.Delta > 0 || e.Delta < 0) {
                _scroller.EnqueueCommand(ScrollType.MouseWheel, e.Delta);
                e.Handled = true;
            }

            if (!e.Handled) {
                base.OnMouseWheel(e);
            }
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e) {
            Point pt = e.GetPosition(this);

            HitTestResult result = VisualTreeHelper.HitTest(this, pt);
            if (result.VisualHit is TextVisual) {
                var textVisual = (TextVisual)result.VisualHit;
                textVisual.ToggleHighlight();

                e.Handled = true;
            }

            if (!e.Handled) {
                base.OnMouseLeftButtonDown(e);
            }
        }

        private void VerticalScrollBar_Scroll(object sender, ScrollEventArgs e) {
            switch (e.ScrollEventType) {
                case ScrollEventType.EndScroll:
                    _scroller.EnqueueCommand(ScrollType.SetVerticalOffset, e.NewValue);
                    break;
                case ScrollEventType.First:
                    _scroller.EnqueueCommand(ScrollType.SetVerticalOffset, e.NewValue);
                    break;
                case ScrollEventType.LargeDecrement:
                    _scroller.EnqueueCommand(ScrollType.PageUp, e.NewValue);
                    break;
                case ScrollEventType.LargeIncrement:
                    _scroller.EnqueueCommand(ScrollType.PageDown, e.NewValue);
                    break;
                case ScrollEventType.Last:
                    _scroller.EnqueueCommand(ScrollType.SetVerticalOffset, e.NewValue);
                    break;
                case ScrollEventType.SmallDecrement:
                    _scroller.EnqueueCommand(ScrollType.LineUp, e.NewValue);
                    break;
                case ScrollEventType.SmallIncrement:
                    _scroller.EnqueueCommand(ScrollType.LineDown, e.NewValue);
                    break;
                case ScrollEventType.ThumbPosition:
                    _scroller.EnqueueCommand(ScrollType.SetVerticalOffset, e.NewValue);
                    break;
                case ScrollEventType.ThumbTrack:
                    _scroller.EnqueueCommand(ScrollType.SetVerticalOffset, e.NewValue);
                    break;
                default:
                    break;
            }
        }

        private void HorizontalScrollBar_Scroll(object sender, ScrollEventArgs e) {
            switch (e.ScrollEventType) {
                case ScrollEventType.EndScroll:
                    _scroller.EnqueueCommand(ScrollType.SetHorizontalOffset, e.NewValue);
                    break;
                case ScrollEventType.First:
                    _scroller.EnqueueCommand(ScrollType.SetHorizontalOffset, e.NewValue);
                    break;
                case ScrollEventType.LargeDecrement:
                    _scroller.EnqueueCommand(ScrollType.PageLeft, e.NewValue);
                    break;
                case ScrollEventType.LargeIncrement:
                    _scroller.EnqueueCommand(ScrollType.PageRight, e.NewValue);
                    break;
                case ScrollEventType.Last:
                    _scroller.EnqueueCommand(ScrollType.SetHorizontalOffset, e.NewValue);
                    break;
                case ScrollEventType.SmallDecrement:
                    _scroller.EnqueueCommand(ScrollType.LineLeft, e.NewValue);
                    break;
                case ScrollEventType.SmallIncrement:
                    _scroller.EnqueueCommand(ScrollType.LineRight, e.NewValue);
                    break;
                case ScrollEventType.ThumbPosition:
                    _scroller.EnqueueCommand(ScrollType.SetHorizontalOffset, e.NewValue);
                    break;
                case ScrollEventType.ThumbTrack:
                    _scroller.EnqueueCommand(ScrollType.SetHorizontalOffset, e.NewValue);
                    break;
                default:
                    break;
            }
        }
    }
}
