using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Common.Core;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    /// <summary>
    /// Handles scroll command
    /// </summary>
    internal class VisualGridScroller {
        private TaskScheduler ui;
        private BlockingCollection<ScrollCommand> _scrollCommands;

        public VisualGridScroller() {
            ui = TaskScheduler.FromCurrentSynchronizationContext();

            _scrollCommands = new BlockingCollection<ScrollCommand>();

            // silence every exception and don't wait
            Task.Run(() => ScrollCommandsHandler().SilenceException<Exception>().DoNotWait());
        }

        public GridPoints Points { get; set; }
        public VisualGrid ColumnHeader { get; set; }
        public VisualGrid RowHeader { get; set; }
        public VisualGrid DataGrid { get; set; }

        private IGridProvider<string> _dataProvider;
        public IGridProvider<string> DataProvider {
            get {
                return _dataProvider;
            }
            set {
                FlushCommands();
                _dataProvider = value;
            }
        }

        internal void FlushCommands() {
            ScrollCommand command;
            while (_scrollCommands.TryTake(out command)) {
            }
        }

        internal void EnqueueCommand(ScrollType code, double param) {
            _scrollCommands.Add(new ScrollCommand(code, param));
        }

        internal void EnqueueCommand(ScrollType code, Size size) {
            _scrollCommands.Add(new ScrollCommand(code, size));
        }

        private async Task ScrollCommandsHandler() {
            const int ScrollCommandUpperBound = 50;
            List<ScrollCommand> batch = new List<ScrollCommand>();

            foreach (var command in _scrollCommands.GetConsumingEnumerable()) {
                try {
                    batch.Add(command);
                    if (_scrollCommands.Count > 0
                        && _scrollCommands.Count < ScrollCommandUpperBound) {
                        // another command has been queued already. continue to next
                        // upperbound 50 prevents infinite loop in case scroll commands is queued fast and endlessly, which happens only in theory
                        continue;
                    } else {
                        for (int i = 0; i < batch.Count; i++) {
                            // if next command is same the current one, skip to next (new one) for optimization
                            if (i < (batch.Count - 1)
                                && ((batch[i].Code == ScrollType.SizeChange && batch[i + 1].Code == ScrollType.SizeChange)
                                    || (batch[i].Code == ScrollType.SetHorizontalOffset && batch[i + 1].Code == ScrollType.SetHorizontalOffset)
                                    || (batch[i].Code == ScrollType.SetVerticalOffset && batch[i + 1].Code == ScrollType.SetVerticalOffset)
                                    || (batch[i].Code == ScrollType.Refresh && batch[i + 1].Code == ScrollType.Refresh))) {
                                continue;
                            } else {
                                await ExecuteCommand(batch[i]);
                            }
                        }
                        batch.Clear();
                    }
                } catch (Exception ex) {
                    Trace.WriteLine(ex.Message);    // TODO: handle exception
                    batch.Clear();
                }
            }
        }

        private async Task ExecuteCommand(ScrollCommand cmd) {
            switch (cmd.Code) {
                case ScrollType.LineUp:
                    await LineUpAsync();
                    break;
                case ScrollType.LineDown:
                    await LineDownAsync();
                    break;
                case ScrollType.LineLeft: LineLeft(); break;
                case ScrollType.LineRight: LineRight(); break;

                case ScrollType.PageUp:
                    await PageUpAsync();
                    break;
                case ScrollType.PageDown:
                    await PageDownAsync();
                    break;
                case ScrollType.PageLeft: PageLeft(); break;
                case ScrollType.PageRight: PageRight(); break;

                case ScrollType.SetHorizontalOffset:
                    await SetHorizontalOffsetAsync(cmd.Param);
                    break;
                case ScrollType.SetVerticalOffset:
                    await SetVerticalOffsetAsync(cmd.Param);
                    break;
                case ScrollType.MouseWheel:
                    await SetMouseWheelAsync(cmd.Param); break;

                case ScrollType.SizeChange:
                    await DrawVisualsAsync(
                        new Rect(
                            Points.HorizontalOffset,
                            Points.VerticalOffset,
                            DataGrid.RenderSize.Width,
                            DataGrid.RenderSize.Height),
                        false);
                    break;
                case ScrollType.Refresh:
                    await DrawVisualsAsync(
                        new Rect(
                            Points.HorizontalOffset,
                            Points.VerticalOffset,
                            DataGrid.RenderSize.Width,
                            DataGrid.RenderSize.Height),
                        true);
                    break;
                case ScrollType.Invalid:
                    break;
            }
        }

        private async Task DrawVisualsAsync(Rect visualViewport, bool refresh) {
            using (var elapsed = new Elapsed("PullDataAndDraw:")) {
                GridRange newViewport = Points.ComputeDataViewport(visualViewport);

                // pull data from provider
                var data = await DataProvider.GetAsync(newViewport);

                // actual drawing runs in UI thread
                await Task.Factory.StartNew(
                    () => DrawVisuals(newViewport, data, refresh),
                    CancellationToken.None,
                    TaskCreationOptions.None,
                    ui);
            }
        }

        private void DrawVisuals(GridRange dataViewport, IGridData<string> data, bool refresh) {
            if (DataGrid != null) {
                DataGrid.DrawVisuals(dataViewport, data.Grid, refresh);
            }

            if (ColumnHeader != null) {
                GridRange columnViewport = new GridRange(
                    new Range(0, 1),
                    dataViewport.Columns);

                ColumnHeader.DrawVisuals(
                    columnViewport,
                    new RangeToGrid<string>(dataViewport.Columns, data.ColumnHeader, true),
                    refresh);
            }

            if (RowHeader != null) {
                GridRange rowViewport = new GridRange(
                    dataViewport.Rows,
                    new Range(0, 1));

                RowHeader.DrawVisuals(
                    rowViewport,
                    new RangeToGrid<string>(dataViewport.Rows, data.RowHeader, false),
                    refresh);
            }
        }

        private async Task SetVerticalOffsetAsync(double offset) {
            Points.VerticalOffset = offset;

            await DrawVisualsAsync(
                new Rect(
                    Points.HorizontalOffset,
                    Points.VerticalOffset,
                    DataGrid.RenderSize.Width,
                    DataGrid.RenderSize.Height),
                false);
        }

        private Task LineUpAsync() {
            return SetVerticalOffsetAsync(Points.VerticalOffset - 10.0);    // TODO: do not hard-code the number here.
        }

        private Task LineDownAsync() {
            return SetVerticalOffsetAsync(Points.VerticalOffset + 10.0);    // TODO: do not hard-code the number here.
        }

        private Task PageUpAsync() {
            return SetVerticalOffsetAsync(Points.VerticalOffset - 100.0);    // TODO: do not hard-code the number here.
        }

        private Task PageDownAsync() {
            return SetVerticalOffsetAsync(Points.VerticalOffset + 100.0);    // TODO: do not hard-code the number here.
        }

        private async Task SetHorizontalOffsetAsync(double offset) {
            Points.HorizontalOffset = offset;

            await DrawVisualsAsync(
                new Rect(
                    Points.HorizontalOffset,
                    Points.VerticalOffset,
                    DataGrid.RenderSize.Width,
                    DataGrid.RenderSize.Height),
                false);
        }

        private void LineRight() {
            throw new NotImplementedException();
        }

        private void LineLeft() {
            throw new NotImplementedException();
        }

        private void PageRight() {
            throw new NotImplementedException();
        }

        private void PageLeft() {
            throw new NotImplementedException();
        }

        private Task SetMouseWheelAsync(double delta) {
            return SetVerticalOffsetAsync(Points.VerticalOffset - delta);
        }
    }

    internal enum ScrollType {
        Invalid,
        LineUp,
        LineDown,
        LineLeft,
        LineRight,
        PageUp,
        PageDown,
        PageLeft,
        PageRight,
        SetHorizontalOffset,
        SetVerticalOffset,
        MouseWheel,
        SizeChange,
        Refresh,
    }

    internal struct ScrollCommand {
        private static Lazy<ScrollCommand> _empty = new Lazy<ScrollCommand>(() => new ScrollCommand(ScrollType.Invalid, 0));
        public static ScrollCommand Empty { get { return _empty.Value; } }

        internal ScrollCommand(ScrollType code, double param) {
            Debug.Assert(code != ScrollType.SizeChange);

            Code = code;
            Param = param;
            Size = Size.Empty;
        }

        internal ScrollCommand(ScrollType code, Size size) {
            Code = code;
            Param = double.NaN;
            Size = size;
        }

        public ScrollType Code { get; set; }

        public double Param { get; set; }

        public Size Size { get; set; }
    }
}
