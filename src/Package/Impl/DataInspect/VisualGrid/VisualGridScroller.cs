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
        private TaskScheduler _ui;
        private BlockingCollection<ScrollCommand> _scrollCommands;

        private CancellationTokenSource _cancellSource;
        private MatrixView _owner;
        private Task _handlerTask;

        public VisualGridScroller(MatrixView owner) {
            _ui = TaskScheduler.FromCurrentSynchronizationContext();

            _owner = owner;
            Points = owner.Points;

            _cancellSource = new CancellationTokenSource();
            _scrollCommands = new BlockingCollection<ScrollCommand>();

            // silence every exception and don't wait
            _handlerTask = ScrollCommandsHandler(_cancellSource.Token).SilenceException<Exception>();
        }

        public GridPoints Points { get; }

        public VisualGrid ColumnHeader {
            get {
                return _owner.ColumnHeader;
            }
        }

        public VisualGrid RowHeader {
            get {
                return _owner.RowHeader;
            }
        }

        public VisualGrid DataGrid {
            get {
                return _owner.Data;
            }
        }

        public IGridProvider<string> DataProvider {
            get {
                return _owner.DataProvider;
            }
        }

        internal void StopScroller() {
            _cancellSource.Cancel();
        }

        internal void EnqueueCommand(ScrollType code, double param) {
            _scrollCommands.Add(new ScrollCommand(code, param));
        }

        internal void EnqueueCommand(ScrollType code, Size size) {
            _scrollCommands.Add(new ScrollCommand(code, size));
        }

        private async Task ScrollCommandsHandler(CancellationToken cancellatoinToken) {
            await TaskUtilities.SwitchToBackgroundThread();

            const int ScrollCommandUpperBound = 50;
            List<ScrollCommand> batch = new List<ScrollCommand>();

            foreach (var command in _scrollCommands.GetConsumingEnumerable(cancellatoinToken)) {
                try {
                    batch.Add(command);
                    if (_scrollCommands.Count > 0
                        && _scrollCommands.Count < ScrollCommandUpperBound) {
                        // another command has been queued already. continue to next
                        // upperbound 50 prevents infinite loop in case scroll commands is queued fast and endlessly, which happens only in theory
                        continue;
                    } else {
                        for (int i = 0; i < batch.Count; i++) {
                            if (cancellatoinToken.IsCancellationRequested) {
                                break;
                            }

                            // if next command is same the current one, skip to next (new one) for optimization
                            if (i < (batch.Count - 1)
                                && ((batch[i].Code == ScrollType.SizeChange && batch[i + 1].Code == ScrollType.SizeChange)
                                    || (batch[i].Code == ScrollType.SetHorizontalOffset && batch[i + 1].Code == ScrollType.SetHorizontalOffset)
                                    || (batch[i].Code == ScrollType.SetVerticalOffset && batch[i + 1].Code == ScrollType.SetVerticalOffset)
                                    || (batch[i].Code == ScrollType.Refresh && batch[i + 1].Code == ScrollType.Refresh))) {
                                continue;
                            } else {
                                await ExecuteCommandAsync(batch[i], cancellatoinToken);
                            }
                        }
                        batch.Clear();
                    }
                } catch (Exception ex) {
                    Trace.WriteLine(ex);
                    batch.Clear();
                }
            }
        }

        private const double LineDelta = 10.0;
        private const double PageDelta = 100.0;

        private async Task ExecuteCommandAsync(ScrollCommand cmd, CancellationToken token) {
            bool drawvisual = true;
            bool refresh = false;
            switch (cmd.Code) {
                case ScrollType.LineUp:
                    Points.VerticalOffset -= LineDelta;
                    break;
                case ScrollType.LineDown:
                    Points.VerticalOffset += LineDelta;
                    break;
                case ScrollType.LineLeft:
                    Points.HorizontalOffset -= LineDelta;
                    break;
                case ScrollType.LineRight:
                    Points.HorizontalOffset += LineDelta;
                    break;
                case ScrollType.PageUp:
                    Points.VerticalOffset -= PageDelta;
                    break;
                case ScrollType.PageDown:
                    Points.VerticalOffset += PageDelta;
                    break;
                case ScrollType.PageLeft:
                    Points.HorizontalOffset -= PageDelta;
                    break;
                case ScrollType.PageRight:
                    Points.HorizontalOffset += PageDelta;
                    break;
                case ScrollType.SetHorizontalOffset:
                    Points.HorizontalOffset = cmd.Param;
                    break;
                case ScrollType.SetVerticalOffset:
                    Points.VerticalOffset = cmd.Param;
                    break;
                case ScrollType.MouseWheel:
                    Points.VerticalOffset -= cmd.Param;
                    break;
                case ScrollType.SizeChange:
                    refresh = false;
                    break;
                case ScrollType.Refresh:
                    refresh = true;
                    break;
                case ScrollType.Invalid:
                default:
                    drawvisual = false;
                    break;
            }

            if (drawvisual) {
                await DrawVisualsAsync(
                    new Rect(
                        Points.HorizontalOffset,
                        Points.VerticalOffset,
                        DataGrid.RenderSize.Width,
                        DataGrid.RenderSize.Height),
                    refresh,
                    token);
            }
        }

        private async Task DrawVisualsAsync(Rect visualViewport, bool refresh, CancellationToken token) {
            using (var elapsed = new Elapsed("PullDataAndDraw:")) {
                GridRange newViewport = Points.ComputeDataViewport(visualViewport);

                // pull data from provider
                var data = await DataProvider.GetAsync(newViewport);
                if (!data.Grid.Range.Contains(newViewport)
                    || !data.ColumnHeader.Range.Contains(newViewport.Columns)
                    || !data.RowHeader.Range.Contains(newViewport.Rows)) {
                    throw new InvalidOperationException("Couldn't acquire enough data");
                }

                // actual drawing runs in UI thread
                await Task.Factory.StartNew(
                    () => DrawVisuals(newViewport, data, refresh),
                    token,
                    TaskCreationOptions.None,
                    _ui);
            }
        }

        private void DrawVisuals(GridRange dataViewport, IGridData<string> data, bool refresh) {
            using (var deferal = Points.DeferChangeNotification()) {
                // measure points
                ColumnHeader?.MeasurePoints(
                    Points,
                    new GridRange(new Range(0, 1), dataViewport.Columns),
                    new RangeToGrid<string>(dataViewport.Columns, data.ColumnHeader, true),
                    refresh);

                RowHeader?.MeasurePoints(
                    Points,
                    new GridRange(dataViewport.Rows, new Range(0, 1)),
                    new RangeToGrid<string>(dataViewport.Rows, data.RowHeader, false),
                    refresh);

                DataGrid?.MeasurePoints(
                    Points,
                    dataViewport,
                    data.Grid,
                    refresh);

                // arrange and draw gridline
                ColumnHeader?.ArrangeVisuals(Points);
                RowHeader?.ArrangeVisuals(Points);
                DataGrid?.ArrangeVisuals(Points);
            }
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
