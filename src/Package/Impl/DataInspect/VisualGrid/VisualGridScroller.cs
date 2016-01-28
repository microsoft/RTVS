using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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

        private CancellationTokenSource _cancelSource;
        private MatrixView _owner;
        private Task _handlerTask;

        public VisualGridScroller(MatrixView owner) {
            _ui = TaskScheduler.FromCurrentSynchronizationContext();

            _owner = owner;
            Points = owner.Points;

            _cancelSource = new CancellationTokenSource();
            _scrollCommands = new BlockingCollection<ScrollCommand>();

            // silence every exception and don't wait
            _handlerTask = ScrollCommandsHandler(_cancelSource.Token).SilenceException<Exception>();
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
            _cancelSource.Cancel();
        }

        internal void EnqueueCommand(ScrollType code, double param) {
            _scrollCommands.Add(new ScrollCommand(code, param));
        }

        internal void EnqueueCommand(ScrollType code, double offset, ThumbTrack track) {
            _scrollCommands.Add(new ScrollCommand(code, offset, track));
        }

        internal void EnqueueCommand(ScrollType code, Size size) {
            _scrollCommands.Add(new ScrollCommand(code, size));
        }

        private static ScrollType[] RepeatSkip = new ScrollType[] { ScrollType.SizeChange, ScrollType.SetHorizontalOffset, ScrollType.SetVerticalOffset, ScrollType.Refresh };
        private static ScrollType[] RepeatAccum = new ScrollType[] { ScrollType.MouseWheel, ScrollType.LineUp, ScrollType.LineDown, ScrollType.PageUp, ScrollType.PageDown, ScrollType.LineLeft, ScrollType.LineRight, ScrollType.PageLeft, ScrollType.PageRight };

        private async Task ScrollCommandsHandler(CancellationToken cancellationToken) {
            await TaskUtilities.SwitchToBackgroundThread();

            const int ScrollCommandUpperBound = 50;
            List<ScrollCommand> batch = new List<ScrollCommand>();

            foreach (var command in _scrollCommands.GetConsumingEnumerable(cancellationToken)) {
                try {
                    batch.Add(command);
                    if (_scrollCommands.Count > 0
                        && _scrollCommands.Count < ScrollCommandUpperBound) {
                        // another command has been queued already. continue to next
                        // upperbound 50 prevents infinite loop in case scroll commands is queued fast and endlessly, which happens only in theory
                        continue;
                    } else {
                        for (int i = 0; i < batch.Count; i++) {
                            if (cancellationToken.IsCancellationRequested) {
                                break;
                            }

                            bool execute = true;
                            // if next command is same the current one, skip to next (new one) for optimization
                            if (i < (batch.Count - 1)) {
                                if (IsRepeating(batch, i, RepeatSkip)) {
                                    execute = false;
                                } else if (IsRepeating(batch, i, RepeatAccum)) {
                                    batch[i + 1].Param = (double)batch[i + 1].Param + (double)batch[i].Param;
                                    execute = false;
                                }
                            }

                            if (execute) {
                                await ExecuteCommandAsync(batch[i], cancellationToken);
                            }
                        }
                        batch.Clear();
                    }
                } catch (Exception ex) {
                    Debug.Fail(ex.ToString());
                    batch.Clear();
                }
            }
        }

        private bool IsRepeating(List<ScrollCommand> commands, int index, ScrollType[] scrollTypes) {
            return commands[index].Code == commands[index + 1].Code && scrollTypes.Contains(commands[index].Code);
        }

        private const double LineDelta = 10.0;
        private const double PageDelta = 100.0;

        private async Task ExecuteCommandAsync(ScrollCommand cmd, CancellationToken token) {
            bool drawVisual = true;
            bool refresh = false;
            bool suppress = false;
            switch (cmd.Code) {
                case ScrollType.LineUp:
                    Points.VerticalOffset -= LineDelta * (double)cmd.Param;
                    break;
                case ScrollType.LineDown:
                    Points.VerticalOffset += LineDelta * (double)cmd.Param;
                    break;
                case ScrollType.LineLeft:
                    Points.HorizontalOffset -= LineDelta * (double)cmd.Param;
                    break;
                case ScrollType.LineRight:
                    Points.HorizontalOffset += LineDelta * (double)cmd.Param;
                    break;
                case ScrollType.PageUp:
                    Points.VerticalOffset -= PageDelta * (double)cmd.Param;
                    break;
                case ScrollType.PageDown:
                    Points.VerticalOffset += PageDelta * (double)cmd.Param;
                    break;
                case ScrollType.PageLeft:
                    Points.HorizontalOffset -= PageDelta * (double)cmd.Param;
                    break;
                case ScrollType.PageRight:
                    Points.HorizontalOffset += PageDelta * (double)cmd.Param;
                    break;
                case ScrollType.SetHorizontalOffset: {
                        var args = (Tuple<double, ThumbTrack>)cmd.Param;
                        Points.HorizontalOffset = args.Item1;
                        suppress = args.Item2 == ThumbTrack.Track;
                    }
                    break;
                case ScrollType.SetVerticalOffset: {
                        var args = (Tuple<double, ThumbTrack>)cmd.Param;
                        Points.VerticalOffset = args.Item1;
                        suppress = args.Item2 == ThumbTrack.Track;
                    }
                    break;
                case ScrollType.MouseWheel:
                    Points.VerticalOffset -= (double)cmd.Param;
                    break;
                case ScrollType.SizeChange:
                    Points.ViewportWidth = ((Size)cmd.Param).Width;
                    Points.ViewportHeight = ((Size)cmd.Param).Height;
                    refresh = false;
                    break;
                case ScrollType.Refresh:
                    refresh = true;
                    break;
                case ScrollType.Invalid:
                default:
                    drawVisual = false;
                    break;
            }

            if (drawVisual) {
                await DrawVisualsAsync(refresh, suppress, token);
            }
        }

        private async Task DrawVisualsAsync(bool refresh, bool suppressNotification, CancellationToken token) {
            using (var elapsed = new Elapsed("PullDataAndDraw:")) {

                ScrollDirection overflowDirection = ScrollDirection.None;

                Rect visualViewport = new Rect(
                        Points.HorizontalOffset,
                        Points.VerticalOffset,
                        DataGrid.RenderSize.Width,
                        DataGrid.RenderSize.Height);

                GridRange newViewport = Points.ComputeDataViewport(visualViewport, ref overflowDirection);

                if (newViewport.Rows.Count < 1 || newViewport.Columns.Count < 1) {
                    return;
                }

                // adjust Offset in case of overflow
                if (overflowDirection.HasFlag(ScrollDirection.Horizontal)) {
                    Points.HorizontalOffset = Points.HorizontalExtent - visualViewport.Width;
                } else if (overflowDirection.HasFlag(ScrollDirection.Vertical)) {
                    Points.VerticalOffset = Points.VerticalExtent - visualViewport.Height;
                }

                // pull data from provider
                var data = await DataProvider.GetAsync(newViewport);
                if (!data.Grid.Range.Contains(newViewport)
                    || !data.ColumnHeader.Range.Contains(newViewport.Columns)
                    || !data.RowHeader.Range.Contains(newViewport.Rows)) {
                    throw new InvalidOperationException("Couldn't acquire enough data");
                }

                // actual drawing runs in UI thread
                await Task.Factory.StartNew(
                    () => DrawVisuals(newViewport, data, refresh, overflowDirection, visualViewport, suppressNotification),
                    token,
                    TaskCreationOptions.None,
                    _ui);
            }
        }

        private void DrawVisuals(
            GridRange dataViewport,
            IGridData<string> data,
            bool refresh,
            ScrollDirection overflowDirection,
            Rect visualViewport,
            bool suppressNotification) {

            using (var deferal = Points.DeferChangeNotification(suppressNotification)) {
                // measure points
                ColumnHeader?.MeasurePoints(
                    Points.GetAccessToPoints(ColumnHeader.ScrollDirection),
                    new GridRange(new Range(0, 1), dataViewport.Columns),
                    new RangeToGrid<string>(dataViewport.Columns, data.ColumnHeader, true),
                    refresh);

                RowHeader?.MeasurePoints(
                    Points.GetAccessToPoints(RowHeader.ScrollDirection),
                    new GridRange(dataViewport.Rows, new Range(0, 1)),
                    new RangeToGrid<string>(dataViewport.Rows, data.RowHeader, false),
                    refresh);

                DataGrid?.MeasurePoints(
                    Points.GetAccessToPoints(DataGrid.ScrollDirection),
                    dataViewport,
                    data.Grid,
                    refresh);

                // adjust Offset in case of overflow
                if (overflowDirection.HasFlag(ScrollDirection.Horizontal)) {
                    Points.HorizontalOffset = Points.HorizontalExtent - visualViewport.Width;
                }
                if (overflowDirection.HasFlag(ScrollDirection.Vertical)) {
                    Points.VerticalOffset = Points.VerticalExtent - visualViewport.Height;
                }

                // arrange and draw gridline
                ColumnHeader?.ArrangeVisuals(Points.GetAccessToPoints(ColumnHeader.ScrollDirection));
                RowHeader?.ArrangeVisuals(Points.GetAccessToPoints(RowHeader.ScrollDirection));
                DataGrid?.ArrangeVisuals(Points.GetAccessToPoints(DataGrid.ScrollDirection));
            }
        }
    }
}
