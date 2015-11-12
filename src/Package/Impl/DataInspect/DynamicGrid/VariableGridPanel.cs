//#define PANELTRACE
//#define PANELASSERT

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Threading;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    /// <summary>
    /// assumes, IsVirtualizing = true, VirtualizationMode = Standard, ScrollUnit = Pixel, CacheLength = 1, CacheLengthUnit = Item
    /// </summary>
    public class VariableGridPanel : VirtualizingPanel, IScrollInfo {
        #region IScrollInfo

        [Flags]
        enum ScrollHint {
            None = 0x00,
            Horizontal = 0x01,
            Vertial = 0x02,
        }

        [DefaultValue(false)]
        public bool CanHorizontallyScroll { get; set; }

        [DefaultValue(false)]
        public bool CanVerticallyScroll { get; set; }

        public double ExtentHeight { get; private set; }

        public double ExtentWidth { get; private set; }

        public double HorizontalOffset { get; private set; }

        public ScrollViewer ScrollOwner { get; set; }

        public double VerticalOffset { get; private set; }

        public double ViewportHeight { get; private set; }

        public double ViewportWidth { get; private set; }

        public void LineUp() {
            SetVerticalOffset(VerticalOffset - GetLineDelta(Orientation.Vertical));
        }

        public void LineDown() {
            SetVerticalOffset(VerticalOffset + GetLineDelta(Orientation.Vertical));
        }

        public void LineLeft() {
            SetHorizontalOffset(HorizontalOffset - GetLineDelta(Orientation.Horizontal));
        }

        public void LineRight() {
            SetHorizontalOffset(HorizontalOffset + GetLineDelta(Orientation.Horizontal));
        }

        public void MouseWheelUp() {
            SetVerticalOffset(VerticalOffset - GetMouseWheelDelta(Orientation.Vertical));
        }

        public void MouseWheelDown() {
            SetVerticalOffset(VerticalOffset + GetMouseWheelDelta(Orientation.Vertical));
        }

        public void MouseWheelLeft() {
            SetHorizontalOffset(HorizontalOffset - GetMouseWheelDelta(Orientation.Horizontal));
        }

        public void MouseWheelRight() {
            SetHorizontalOffset(HorizontalOffset + GetMouseWheelDelta(Orientation.Horizontal));
        }

        public void PageUp() {
            SetVerticalOffset(VerticalOffset - GetPageDelta(Orientation.Vertical));
        }

        public void PageDown() {
            SetVerticalOffset(VerticalOffset + GetPageDelta(Orientation.Vertical));
        }

        public void PageLeft() {
            SetHorizontalOffset(HorizontalOffset - GetPageDelta(Orientation.Horizontal));
        }

        public void PageRight() {
            SetHorizontalOffset(HorizontalOffset + GetPageDelta(Orientation.Horizontal));
        }

        private ScrollHint _scrollHint = ScrollHint.None;

        public void SetHorizontalOffset(double offset) {
            offset = CoerceOffset(offset, ViewportWidth, ExtentWidth);

            if (HorizontalOffset != offset) {   // TODO: use close instead of equality as it is double
                _scrollHint |= ScrollHint.Horizontal;
                HorizontalOffset = offset;
                InvalidateMeasure();
            }
        }

        public void SetVerticalOffset(double offset) {
            offset = CoerceOffset(offset, ViewportHeight, ExtentHeight);

            if (VerticalOffset != offset) {
                _scrollHint |= ScrollHint.Vertial;
                VerticalOffset = offset;
                InvalidateMeasure();
            }
        }

        /// <summary>
        /// Return adjusted offset to fit in the current scroll information
        /// </summary>
        private double CoerceOffset(double offset, double viewport, double extent) {
            offset = Math.Floor(offset);    // TODO: Pixel scroll

            if (offset > extent - viewport) {
                offset = extent - viewport;
            }
            if (offset < 0.0) {
                offset = 0.0;
            }

            return offset;
        }

        private double GetLineDelta(Orientation orientation) {
            return 1.0; // TODO: change to pixed based!
        }

        private double GetPageDelta(Orientation orientation) {
            return 10.0;
        }

        private double GetMouseWheelDelta(Orientation orientation) {
            return 1.0; // TODO: change to pixed based!
        }

        public Rect MakeVisible(Visual visual, Rect rectangle) {
            throw new NotImplementedException();
        }

        #endregion

        #region Overrides (Measure/Arrange)

        Point _lastMeasureOffset = new Point();
        Size _lastMeasureAvailableSize = new Size();
        Range _lastMeasureViewportRow = new Range();
        Range _lastMeasureViewportColumn = new Range();
        bool _isRow = true;
        Size _lastMeasureStepDesiredSize = new Size();
        TimeSpan OneStepTimeLimit;
        bool MeasureBackGround = true;

        protected override Size MeasureOverride(Size availableSize) {
            EnsurePrerequisite();

            if (double.IsInfinity(availableSize.Width) || double.IsInfinity(availableSize.Height)) {
                throw new NotSupportedException($"Must set CanContentScroll to true in ScrollViewer for {typeof(VariableGridPanel)}");
            }

            _lastMeasureAvailableSize = availableSize;
            if (MeasureBackGround) {
                OneStepTimeLimit = TimeSpan.FromMilliseconds(100);
                EnsureMeasureOperation();
            } else {
                OneStepTimeLimit = TimeSpan.MaxValue;
                OnMeasureStep(null);
            }

            return availableSize;
        }

        private DispatcherOperation _measureOperation;

        private void EnsureMeasureOperation() {
            if (_measureOperation == null) {
                _measureOperation = Dispatcher.BeginInvoke(DispatcherPriority.Background, new DispatcherOperationCallback(OnMeasureStep), null);
            }
        }

        private void ResetMeasureOperation() {
            _measureOperation = null;
        }

        private object OnMeasureStep(object arg) {
#if PANELTRACE
            DateTime startTime = DateTime.Now;
            _childMeasure = new TimeSpan();
#endif
            AdjustViewport();

            // create first cell
            if (_lastMeasureViewportRow.Count == 0 && _lastMeasureViewportColumn.Count == 0) {
                int horizontalGrowth = 1;
                int verticalGrowth = 1;

                var rowStack = Generator.GetRow(_lastMeasureViewportRow.Start);
                var columnStack = Generator.GetColumn(_lastMeasureViewportColumn.Start);
                Size cellSize = MeasureChild(rowStack, columnStack);
                _lastMeasureStepDesiredSize.Width = cellSize.Width;
                _lastMeasureStepDesiredSize.Height = cellSize.Height;
                _lastMeasureViewportRow.Count += verticalGrowth;
                _lastMeasureViewportColumn.Count += horizontalGrowth;
            }

            DateTime startTime = DateTime.Now;

            do {
                if (_isRow) {
                    GrowVertically(_lastMeasureAvailableSize.Height, ref _lastMeasureStepDesiredSize, ref _lastMeasureViewportRow, ref _lastMeasureViewportColumn);
                } else {
                    GrowHorizontally(_lastMeasureAvailableSize.Width, ref _lastMeasureStepDesiredSize, ref _lastMeasureViewportRow, ref _lastMeasureViewportColumn);
                }
                _isRow ^= true;
            } while (DateTime.Now - startTime < OneStepTimeLimit && ShoulContinueGrowing());

#if PANELASSERT
            AssertDesiredSize();
#endif

            UpdateScrollInfo();

#if PANELTRACE
            Trace(TraceLevel.Info, "VirtualizingGridPanel:Measure: row Count {0} column Count {1}", _lastMeasureViewportRow.Count, _lastMeasureViewportColumn.Count);
            Trace(TraceLevel.Info, "VirtualizingGridPanel:Measure: ChildMeasure {0} msec", _childMeasure.TotalMilliseconds);
            Trace(TraceLevel.Info, "VirtualizingGridPanel:Measure: {0} msec", (DateTime.Now - startTime).TotalMilliseconds);
#endif

            // measure operation
            ResetMeasureOperation();

            if (ShoulContinueGrowing()) {
                EnsureMeasureOperation();
            }

            return _lastMeasureStepDesiredSize;
        }

        private bool ShoulContinueGrowing() {
            return (_lastMeasureAvailableSize.Height > _lastMeasureStepDesiredSize.Height && _lastMeasureViewportRow.Count != Generator.RowCount)
                || (_lastMeasureAvailableSize.Width > _lastMeasureStepDesiredSize.Width && _lastMeasureViewportColumn.Count != Generator.ColumnCount);
        }

        private void AdjustViewport() {
#if PANELASSERT
            AssertDesiredSize();
#endif
            if (Math.Abs(VerticalOffset - _lastMeasureViewportRow.Start) > _lastMeasureViewportRow.Count) {
                _lastMeasureViewportRow.Start = (int)VerticalOffset;
                _lastMeasureViewportRow.Count = 0;
                _lastMeasureStepDesiredSize.Height = 0;
            } else {
                while (VerticalOffset > _lastMeasureViewportRow.Start && _lastMeasureViewportRow.Count > 0) {
                    double height = Generator.GetRow(_lastMeasureViewportRow.Start).LayoutSize.Max.Value;
                    _lastMeasureViewportRow.Start += 1;
                    _lastMeasureViewportRow.Count -= 1;
                    _lastMeasureStepDesiredSize.Height -= height;
                }

                int orgRowStart = _lastMeasureViewportRow.Start;
                while (VerticalOffset < orgRowStart && _lastMeasureViewportRow.Count > 0) {
                    double height = Generator.GetRow(_lastMeasureViewportRow.Start + _lastMeasureViewportRow.Count - 1).LayoutSize.Max.Value;
                    orgRowStart -= 1;
                    _lastMeasureViewportRow.Count -= 1;
                    _lastMeasureStepDesiredSize.Height -= height;
                }
            }

            if (Math.Abs(HorizontalOffset - _lastMeasureViewportColumn.Start) > _lastMeasureViewportColumn.Count) {
                _lastMeasureViewportColumn.Start = (int)HorizontalOffset;
                _lastMeasureViewportColumn.Count = 0;
                _lastMeasureStepDesiredSize.Width = 0;
            } else {
                while (HorizontalOffset > _lastMeasureViewportColumn.Start && _lastMeasureViewportColumn.Count > 0) {
                    double width = Generator.GetColumn(_lastMeasureViewportColumn.Start).LayoutSize.Max.Value;
                    _lastMeasureViewportColumn.Start += 1;
                    _lastMeasureViewportColumn.Count -= 1;
                    _lastMeasureStepDesiredSize.Width -= width;
                }

                int orgColumnStart = _lastMeasureViewportColumn.Start;
                while (HorizontalOffset < orgColumnStart && _lastMeasureViewportColumn.Count > 0) {
                    double width = Generator.GetColumn(_lastMeasureViewportColumn.Start + _lastMeasureViewportColumn.Count - 1).LayoutSize.Max.Value;
                    orgColumnStart -= 1;
                    _lastMeasureViewportColumn.Count -= 1;
                    _lastMeasureStepDesiredSize.Width -= width;
                }
            }
        }

#if PANELASSERT
        private bool DoubleClose(double value1, double value2) {
            return Math.Abs(value1 - value2) < 0.0001;
        }

        private void AssertDesiredSize() {
            double size = 0;
            for (int i = 0; i < _lastMeasureViewportRow.Count; i++) {
                size += Generator.GetRow(_lastMeasureViewportRow.Start + i).LayoutSize.Max.Value;
            }
            Debug.Assert(DoubleClose(_lastMeasureStepDesiredSize.Height, size));

            size = 0;
            for (int i = 0; i < _lastMeasureViewportColumn.Count; i++) {
                size += Generator.GetColumn(_lastMeasureViewportColumn.Start + i).LayoutSize.Max.Value;
            }
            Debug.Assert(DoubleClose(_lastMeasureStepDesiredSize.Width, size));
        }
#endif

        private void UpdateScrollInfo() {
            if (Generator != null) {
                ExtentWidth = Generator.ColumnCount;
                ExtentHeight = Generator.RowCount;

                VerticalOffset = _lastMeasureViewportRow.Start;
                HorizontalOffset = _lastMeasureViewportColumn.Start;

                ViewportWidth = _lastMeasureViewportColumn.Count;
                ViewportHeight = _lastMeasureViewportRow.Count;

                ScrollOwner?.InvalidateScrollInfo();
            }

            _scrollHint = ScrollHint.None;
        }

#if PANELTRACE
        private TimeSpan _childMeasure;
#endif

        private Size MeasureChild(VariableGridStack rowStack, VariableGridStack columnStack) {
#if PANELTRACE
            Trace(TraceLevel.Verbose, "VirtualizingGridPanel:MeasureChild: row {0} column {1}", rowStack.Index, columnStack.Index);
#endif
            double widthConstraint = columnStack.GetSizeConstraint();
            double heightConstraint = rowStack.GetSizeConstraint();

            bool newlyCreated;
            var child = Generator.GenerateAt(rowStack.Index, columnStack.Index, out newlyCreated);
            if (newlyCreated) {
                AddInternalChild(child);
            } else {
                Debug.Assert(InternalChildren.Contains(child));
            }

            if (newlyCreated || !rowStack.LayoutSize.Frozen || !columnStack.LayoutSize.Frozen) {
#if PANELTRACE
                DateTime startChildMeasure = DateTime.Now;
#endif
                child.Measure(new Size(widthConstraint, heightConstraint));

#if PANELTRACE
                _childMeasure += (DateTime.Now - startChildMeasure);
#endif

                columnStack.LayoutSize.Max = child.DesiredSize.Width;
                rowStack.LayoutSize.Max = child.DesiredSize.Height;
            }

            return new Size(columnStack.LayoutSize.Max.Value, rowStack.LayoutSize.Max.Value);
        }

        private int GrowVertically(double extent, ref Size desiredSize, ref Range viewportRow, ref Range viewportColumn) {
            int growth = 0;
            int growStartAt = viewportRow.Start + viewportRow.Count;

            // grow backward to fit scroll backward
            int growthBackward = 0;
            int growBackwardStartAt = viewportRow.Start - 1;
            if ((viewportRow.Start > VerticalOffset)
                && (desiredSize.Height < extent) && (growBackwardStartAt - growthBackward >= 0)) {
                var rowStack = Generator.GetRow(growBackwardStartAt - growthBackward);
                Size rowSize = MeasureRow(rowStack, ref viewportColumn);

                desiredSize.Height += rowSize.Height;
                desiredSize.Width = rowSize.Width;

                growthBackward += 1;
            }
            // grow forward
            else if ((desiredSize.Height < extent) && ((growStartAt + growth) < Generator.RowCount)) {
                var rowStack = Generator.GetRow(growStartAt + growth);
                Size rowSize = MeasureRow(rowStack, ref viewportColumn);

                desiredSize.Height += rowSize.Height;
                desiredSize.Width = rowSize.Width;

                growth += 1;
            }
            // grow backward again to fit size
            else if ((desiredSize.Height < extent) && (growBackwardStartAt - growthBackward >= 0)) {
                var rowStack = Generator.GetRow(growBackwardStartAt - growthBackward);
                Size rowSize = MeasureRow(rowStack, ref viewportColumn);

                desiredSize.Height += rowSize.Height;
                desiredSize.Width = rowSize.Width;

                growthBackward += 1;
            }

            viewportRow.Start -= growthBackward;
            viewportRow.Count += growth + growthBackward;

            return growth + growthBackward;
        }

        private int GrowHorizontally(double extent, ref Size desiredSize, ref Range viewportRow, ref Range viewportColumn) {
            int growth = 0;
            int growStartAt = viewportColumn.Start + viewportColumn.Count;

            // grow backward to fit scroll backward
            int growthBackward = 0;
            int growBackwardStartAt = viewportColumn.Start - 1;
            if ((viewportColumn.Start > HorizontalOffset)
                && (desiredSize.Width < extent) && ((growBackwardStartAt - growthBackward >= 0))) {
                var columnStack = Generator.GetColumn(growBackwardStartAt - growthBackward);
                Size columnSize = MeasureColumn(columnStack, ref viewportRow);

                desiredSize.Height = columnSize.Height;
                desiredSize.Width += columnSize.Width;

                growthBackward += 1;
            }
            // grow forward
            else if ((desiredSize.Width < extent) && ((growStartAt + growth) < Generator.ColumnCount)) {
                var columnStack = Generator.GetColumn(growStartAt + growth);
                Size columnSize = MeasureColumn(columnStack, ref viewportRow);

                desiredSize.Height = columnSize.Height;
                desiredSize.Width += columnSize.Width;

                growth += 1;
            }
            // grow backward again to fit size
            else if ((desiredSize.Width < extent) && ((growBackwardStartAt - growthBackward >= 0))) {
                var columnStack = Generator.GetColumn(growBackwardStartAt - growthBackward);
                Size columnSize = MeasureColumn(columnStack, ref viewportRow);

                desiredSize.Height = columnSize.Height;
                desiredSize.Width += columnSize.Width;

                growthBackward += 1;
            }

            viewportColumn.Start -= growthBackward;
            viewportColumn.Count += growth + growthBackward;

            return growth;
        }

        private Size MeasureRow(VariableGridStack rowStack, ref Range viewportColumn) {
            double width = 0.0;
            int column = viewportColumn.Start;
            while (viewportColumn.Contains(column) && column < Generator.ColumnCount) {
                var columnStack = Generator.GetColumn(column);
                Size childDesiredSize = MeasureChild(rowStack, columnStack);

                width += childDesiredSize.Width;

                column++;
            }

            return new Size(width, rowStack.LayoutSize.Max.Value);
        }

        private Size MeasureColumn(VariableGridStack columnStack, ref Range viewportRow) {
            double height = 0.0;
            int row = viewportRow.Start;
            while (viewportRow.Contains(row) && row < Generator.RowCount) {
                var rowStack = Generator.GetRow(row);
                Size childDesiredSize = MeasureChild(rowStack, columnStack);

                height += childDesiredSize.Height;

                row++;
            }

            return new Size(columnStack.LayoutSize.Max.Value, height);
        }

        protected override Size ArrangeOverride(Size finalSize) {
#if PANELTRACE
            DateTime startTime = DateTime.Now;
#endif
            double computedRowOffset, computedColumnOffset;
            Generator.ComputeStackPosition(_lastMeasureViewportRow, _lastMeasureViewportColumn, out computedRowOffset, out computedColumnOffset);

            foreach (VariableGridCell child in InternalChildren) {
                var columnStack = Generator.GetColumn(child.Column);
                var rowStack = Generator.GetRow(child.Row);

                Rect rect = new Rect(
                    columnStack.LayoutPosition.Value - computedColumnOffset + _lastMeasureOffset.X,
                    rowStack.LayoutPosition.Value - computedRowOffset + _lastMeasureOffset.Y,
                    columnStack.LayoutSize.Max.Value,
                    rowStack.LayoutSize.Max.Value);
#if PANELTRACE
                Trace(TraceLevel.Verbose, "VirtualizingGridPanel:Arrange: row {0} column {1} rect {2}", child.Row, child.Column, rect);
#endif
                child.Arrange(rect);
            }
#if PANELTRACE
            Trace(TraceLevel.Info, "VirtualizingGridPanel:Arrange(except PostArrange): {0} msec", (DateTime.Now - startTime).TotalMilliseconds);
#endif
            PostArrange();

            return finalSize;
        }

        #endregion

        #region Clean Up

        private void PostArrange() {
#if PANELTRACE
            DateTime startTime = DateTime.Now;
#endif
            EnsureCleanupOperation();

            Generator.FreezeLayoutSize();
#if PANELTRACE
            Trace(TraceLevel.Info, "VirtualizingGridPanel:PostArrange: {0} msec", (DateTime.Now - startTime).TotalMilliseconds);
#endif
        }

        private DispatcherOperation _cleanupOperation;

        private void EnsureCleanupOperation() {
            if (_cleanupOperation == null) {
                _cleanupOperation = Dispatcher.BeginInvoke(DispatcherPriority.Background, new DispatcherOperationCallback(OnCleanUp), null);
            }
        }

        private object OnCleanUp(object args) {
            try {
#if PANELTRACE
                DateTime startTime = DateTime.Now;
#endif
                // Remove from visual children
                int begin = -1;
                while (true) {
                    Range cleanBlock = FindFirstChildrenRangeOutsideViewport(ref begin);

                    if (cleanBlock.Count == 0) {
                        break;
                    }

                    for (int i = 0; i < cleanBlock.Count; i++) {
                        var child = (VariableGridCell)InternalChildren[cleanBlock.Start + i];
                        bool removed = Generator.RemoveAt(child.Row, child.Column);
#if PANELTRACE
                        Trace(TraceLevel.Verbose, "VirtualizingGridPanel:OnCleanUp: remove: row {0} column {1}", child.Row, child.Column);
#endif
                        Debug.Assert(removed);
                    }

                    RemoveInternalChildRange(cleanBlock.Start, cleanBlock.Count);
                }
#if PANELTRACE
                Trace(TraceLevel.Info, "VirtualizingGridPanel:OnCleanUp 1: {0} msec", (DateTime.Now - startTime).TotalMilliseconds);
#endif
                // clean up from container generator
                Generator.RemoveRowsExcept(_lastMeasureViewportRow);
                Generator.RemoveColumnsExcept(_lastMeasureViewportColumn);
#if PANELTRACE
                Trace(TraceLevel.Info, "VirtualizingGridPanel:OnCleanUp 2: {0} msec", (DateTime.Now - startTime).TotalMilliseconds);
#endif
            } finally {
                _cleanupOperation = null;
            }

            return null;
        }

        private Range FindFirstChildrenRangeOutsideViewport(ref int start) {
            int begin = start == -1 ? InternalChildren.Count - 1 : start;

            int? nextStart = null;
            int lastIndex = 0; int count = 0;
            for (int i = begin; i >= 0; i--) {
                VariableGridCell child = (VariableGridCell)InternalChildren[i];

                if (!_lastMeasureViewportRow.Contains(child.Row)
                    || !_lastMeasureViewportColumn.Contains(child.Column)) {
                    if (count == 0) {
                        lastIndex = i;
                    }
                    count++;
                } else {
                    if (count != 0) {
                        nextStart = i;
                        break;
                    }
                }
            }

            if (!nextStart.HasValue) {
                nextStart = 0;
            }

            Debug.Assert((start == -1) || (InternalChildren.Count >= start + count));
            return new Range() { Start = lastIndex - count + 1, Count = count };
        }

        #endregion

        private VariableGrid Generator { get; set; }

        internal ItemsControl OwningItemsControl { get; private set; }

        internal VirtualizationCacheLength CacheLength { get; private set; }

        internal VirtualizationCacheLengthUnit CacheLengthUnit { get; private set; }

        private void EnsurePrerequisite() {
            ItemsControl owningItemsControl = ItemsControl.GetItemsOwner(this);
            if (owningItemsControl == null) {
                throw new NotSupportedException($"{typeof(VariableGridPanel)} supports only ItemsPanel. Can't use stand alone");
            }
            OwningItemsControl = owningItemsControl;

            if (!(owningItemsControl is VariableGrid)) {
                throw new NotSupportedException($"{typeof(VariableGridPanel)} supports only {typeof(VariableGrid)}'s ItemsPanel");
            }
            this.Generator = ((VariableGrid)owningItemsControl);

            if (ScrollOwner == null) {
                throw new NotSupportedException($"{typeof(VariableGridPanel)} must be used for top level scrolling panel");
            }

            if (!GetIsVirtualizing(owningItemsControl)) {
                throw new NotSupportedException($"{typeof(VariableGridPanel)} supports onlly IsVirtualizing=\"true\"");
            }

            if (GetVirtualizationMode(owningItemsControl) != VirtualizationMode.Standard) {
                throw new NotSupportedException($"{typeof(VariableGridPanel)} supports onlly VirtualizationMode=\"Standard\"");
            }

            if (GetScrollUnit(owningItemsControl) != ScrollUnit.Item) {
                throw new NotSupportedException($"{typeof(VariableGridPanel)} supports onlly ScrollUnit=\"Item\"");
            }

            CacheLength = GetCacheLength(owningItemsControl);

            CacheLengthUnit = GetCacheLengthUnit(owningItemsControl);

            var children = this.Children;   // this ensures that ItemContainerGenerator is not null
        }

        #region TRACE
#if PANELTRACE
        private int _traceStart = 0;
        private List<string> _traces = new List<string>();
        private DispatcherOperation _traceOperation;
        private TraceLevel _traceLevel = TraceLevel.Info;

        object OnFlush(object args) {
            foreach (var message in _traces) {
                Debug.WriteLine(message);
            }
            _traces.Clear();
            _traceStart = 0;

            _traceOperation = null;
            return null;
        }

        void EnsureFlushOperation() {
            if (_traceOperation == null) {
                _traceOperation = Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new DispatcherOperationCallback(OnFlush), null);
            }
        }

        void Trace(TraceLevel level, string message) {
            if (level <= _traceLevel) {
                AddTrace(message);

                EnsureFlushOperation();
            }
        }

        void Trace(TraceLevel level, string format, params object[] args) {
            if (level <= _traceLevel) {
                AddTrace(string.Format(format, args));

                EnsureFlushOperation();
            }
        }

        void AddTrace(string message) {
            if (_traces.Count > 1000) {
                _traces[_traceStart++] = message;
            } else {
                _traces.Add(message);
            }
        }
#endif

        #endregion
    }
}
