// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Windows;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Logging;
using Microsoft.Common.Core.Shell;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.R.Package.Shell;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    /// <summary>
    /// Handles scroll command
    /// </summary>
    internal sealed class VisualGridScroller {
        private readonly BufferBlock<ScrollCommand> _scrollCommands;

        private readonly CancellationTokenSource _cancelSource;
        private readonly MatrixView _owner;
        private readonly Task _handlerTask;

        public VisualGridScroller(MatrixView owner) {
            _owner = owner;
            if (_owner.ColumnHeader != null) {
                _owner.ColumnHeader.SortOrderChanged += OnSortOrderChanged;
            }
            _cancelSource = new CancellationTokenSource();
            _scrollCommands = new BufferBlock<ScrollCommand>();

            // silence every exception and don't wait
            _handlerTask = ScrollCommandsHandler(_cancelSource.Token).SilenceException<Exception>();
        }

        public GridPoints Points => _owner.Points;
        public VisualGrid ColumnHeader => _owner.ColumnHeader;
        public VisualGrid RowHeader => _owner.RowHeader;
        public VisualGrid DataGrid => _owner.Data;
        public IGridProvider<string> DataProvider => _owner.DataProvider;
        internal void StopScroller() => _cancelSource.Cancel();

        internal void EnqueueCommand(GridUpdateType code, double param) {
            _scrollCommands.Post(new ScrollCommand(code, param));
        }

        internal void EnqueueCommand(GridUpdateType code, double offset, ThumbTrack track) {
            _scrollCommands.Post(new ScrollCommand(code, offset, track));
        }

        internal void EnqueueCommand(GridUpdateType code, Size size) {
            _scrollCommands.Post(new ScrollCommand(code, size));
        }

        private static GridUpdateType[] RepeatSkip = new GridUpdateType[] { GridUpdateType.SizeChange, GridUpdateType.SetHorizontalOffset, GridUpdateType.SetVerticalOffset, GridUpdateType.Refresh };
        private static GridUpdateType[] RepeatAccum = new GridUpdateType[] { GridUpdateType.MouseWheel, GridUpdateType.LineUp, GridUpdateType.LineDown, GridUpdateType.PageUp, GridUpdateType.PageDown, GridUpdateType.LineLeft, GridUpdateType.LineRight, GridUpdateType.PageLeft, GridUpdateType.PageRight };

        private async Task ScrollCommandsHandler(CancellationToken cancellationToken) {
            await TaskUtilities.SwitchToBackgroundThread();

            const int ScrollCommandUpperBound = 100;
            List<ScrollCommand> batch = new List<ScrollCommand>();

            while (true) {
                var command = await _scrollCommands.ReceiveAsync(cancellationToken);
                try {
                    batch.Add(command);
                    if (_scrollCommands.Count > 0 && _scrollCommands.Count < ScrollCommandUpperBound) {
                        // another command has been queued already. continue to next
                        // upperbound prevents infinite loop in case scroll commands is queued fast and endlessly, which happens only in theory
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
                    VsAppShell.Current.Log().Write(LogVerbosity.Normal, MessageCategory.Error, "VisualGridScroller exception: " + ex);
                    batch.Clear();
                }
            }
        }

        private bool IsRepeating(List<ScrollCommand> commands, int index, GridUpdateType[] scrollTypes) {
            return commands[index].UpdateType == commands[index + 1].UpdateType && scrollTypes.Contains(commands[index].UpdateType);
        }

        private async Task ExecuteCommandAsync(ScrollCommand cmd, CancellationToken token) {
            bool drawVisual = true;
            bool suppress = false;
            switch (cmd.UpdateType) {
                case GridUpdateType.LineUp:
                    Points.VerticalOffset -= Points.AverageItemHeight * (double)cmd.Param;
                    break;
                case GridUpdateType.LineDown:
                    Points.VerticalOffset += Points.AverageItemHeight * (double)cmd.Param;
                    break;
                case GridUpdateType.LineLeft:
                    Points.HorizontalOffset -= Points.AverageItemHeight * (double)cmd.Param;    // for horizontal line increment, use vertical size
                    break;
                case GridUpdateType.LineRight:
                    Points.HorizontalOffset += Points.AverageItemHeight * (double)cmd.Param;    // for horizontal line increment, use vertical size
                    break;
                case GridUpdateType.PageUp:
                    Points.VerticalOffset -= Points.ViewportHeight * (double)cmd.Param;
                    break;
                case GridUpdateType.PageDown:
                    Points.VerticalOffset += Points.ViewportHeight * (double)cmd.Param;
                    break;
                case GridUpdateType.PageLeft:
                    Points.HorizontalOffset -= Points.ViewportWidth * (double)cmd.Param;
                    break;
                case GridUpdateType.PageRight:
                    Points.HorizontalOffset += Points.ViewportWidth * (double)cmd.Param;
                    break;
                case GridUpdateType.SetHorizontalOffset: {
                        var args = (Tuple<double, ThumbTrack>)cmd.Param;
                        Points.HorizontalOffset = args.Item1 * (Points.HorizontalExtent - Points.ViewportWidth);
                        suppress = args.Item2 == ThumbTrack.Track;
                    }
                    break;
                case GridUpdateType.SetVerticalOffset: {
                        var args = (Tuple<double, ThumbTrack>)cmd.Param;
                        Points.VerticalOffset = args.Item1 * (Points.VerticalExtent - Points.ViewportHeight);
                        suppress = args.Item2 == ThumbTrack.Track;
                    }
                    break;
                case GridUpdateType.MouseWheel:
                    Points.VerticalOffset -= (double)cmd.Param;
                    break;
                case GridUpdateType.SizeChange:
                    Points.ViewportWidth = ((Size)cmd.Param).Width;
                    Points.ViewportHeight = ((Size)cmd.Param).Height;
                    break;
                case GridUpdateType.Refresh:
                case GridUpdateType.Sort:
                    break;
                case GridUpdateType.Invalid:
                default:
                    drawVisual = false;
                    break;
            }

            if (drawVisual) {
                await DrawVisualsAsync(cmd.UpdateType, suppress, token, _owner.ColumnHeader?.SortOrder);
            }
        }

        private async Task DrawVisualsAsync(GridUpdateType updateType, bool suppressNotification, CancellationToken token, ISortOrder sortOrder) {
            Rect visualViewport = new Rect(
                    Points.HorizontalOffset,
                    Points.VerticalOffset,
                    Points.ViewportWidth,
                    Points.ViewportHeight);

            GridRange newViewport = Points.ComputeDataViewport(visualViewport);

            if (newViewport.Rows.Count < 1 || newViewport.Columns.Count < 1) {
                return;
            }

            // pull data from provider
            try {
                var data = await DataProvider.GetAsync(newViewport, sortOrder);
                if (data == null) {
                    throw new InvalidOperationException("Couldn't fetch grid data");
                }

                if (!data.Grid.Range.Contains(newViewport)
                    || !data.ColumnHeader.Range.Contains(newViewport.Columns)
                    || !data.RowHeader.Range.Contains(newViewport.Rows)) {
                    throw new InvalidOperationException("Couldn't acquire enough data");
                }

                await DrawVisualsAsync(newViewport, data, updateType, visualViewport, suppressNotification, token);
            } catch (OperationCanceledException) { }
        }

        private async Task DrawVisualsAsync(
            GridRange dataViewport,
            IGridData<string> data,
            GridUpdateType updateType,
            Rect visualViewport,
            bool suppressNotification,
            CancellationToken token) {

            await VsAppShell.Current.SwitchToMainThreadAsync(token);

            using (var deferal = Points.DeferChangeNotification(suppressNotification)) {
                // measure points
                ColumnHeader?.MeasurePoints(
                    Points.GetAccessToPoints(ColumnHeader.ScrollDirection),
                    new GridRange(new Range(0, 1), dataViewport.Columns),
                    new RangeToGrid<string>(dataViewport.Columns, data.ColumnHeader, true),
                    updateType);

                RowHeader?.MeasurePoints(
                    Points.GetAccessToPoints(RowHeader.ScrollDirection),
                    new GridRange(dataViewport.Rows, new Range(0, 1)),
                    new RangeToGrid<string>(dataViewport.Rows, data.RowHeader, false),
                    updateType == GridUpdateType.Sort ? GridUpdateType.Refresh : updateType);

                DataGrid?.MeasurePoints(
                    Points.GetAccessToPoints(DataGrid.ScrollDirection),
                    dataViewport,
                    data.Grid,
                    updateType == GridUpdateType.Sort ? GridUpdateType.Refresh : updateType);

                // adjust Offset in case of overflow
                if ((Points.HorizontalOffset + visualViewport.Width).GreaterThanOrClose(Points.HorizontalExtent)) {
                    Points.HorizontalOffset = Points.HorizontalExtent - visualViewport.Width;
                }
                if ((Points.VerticalOffset + visualViewport.Height).GreaterThanOrClose(Points.VerticalExtent)) {
                    Points.VerticalOffset = Points.VerticalExtent - visualViewport.Height;
                }

                // arrange and draw gridline
                ColumnHeader?.ArrangeVisuals(Points.GetAccessToPoints(ColumnHeader.ScrollDirection));
                RowHeader?.ArrangeVisuals(Points.GetAccessToPoints(RowHeader.ScrollDirection));
                DataGrid?.ArrangeVisuals(Points.GetAccessToPoints(DataGrid.ScrollDirection));

                Points.ViewportHeight = DataGrid.RenderSize.Height;
                Points.ViewportWidth = DataGrid.RenderSize.Width;
            }
        }
        private void OnSortOrderChanged(object sender, EventArgs e) {
            if (_owner?.Data == null || _owner.ColumnHeader == null) {
                return;
            }
            _owner?.UpdateSort();
        }
    }
}
