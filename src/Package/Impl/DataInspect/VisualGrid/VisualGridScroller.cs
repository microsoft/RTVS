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
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Threading;
using Microsoft.Common.Wpf.Extensions;
using static Microsoft.VisualStudio.R.Package.DataInspect.GridUpdateType;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    /// <summary>
    /// Handles scroll command
    /// </summary>
    internal sealed class VisualGridScroller {
        private readonly BufferBlock<ScrollCommand> _scrollCommands;

        private readonly CancellationTokenSource _cancelSource;
        private readonly MatrixView _owner;
        private readonly IServiceContainer _services;
        private readonly Task _handlerTask;

        public VisualGridScroller(MatrixView owner, IServiceContainer services) {
            _owner = owner;
            _services = services;
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
        private VisualGrid DataGrid => _owner.Data;
        public GridRange DataViewport { get; private set; }
        public IGridData<string> Data { get; private set; }

        public IGridProvider<string> DataProvider => _owner.DataProvider;
        internal void StopScroller() => _cancelSource.Cancel();

        public async Task ScrollIntoViewAsync(long row, long column, CancellationToken cancellationToken = default(CancellationToken)) {
            var viewportRect = new Rect(DataGrid.RenderSize);
            var focusRect = Points.GetBounds(row, column);
            while (!viewportRect.ContainsOrCloseTo(focusRect)) {
                Points.HorizontalOffset += focusRect.Right.GreaterThan(viewportRect.Width)
                    ? focusRect.Right - viewportRect.Width
                    : focusRect.Left.LessThan(0) ? focusRect.Left : 0;
                Points.VerticalOffset += focusRect.Bottom.GreaterThan(viewportRect.Height)
                    ? focusRect.Bottom - viewportRect.Height
                    : focusRect.Top.LessThan(0) ? focusRect.Top : 0;

                await DrawVisualsAsync(ScrollIntoView, false, cancellationToken, _owner.ColumnHeader?.SortOrder);

                viewportRect = new Rect(DataGrid.RenderSize);
                focusRect = Points.GetBounds(row, column);
            }
        }

        internal void EnqueueCommand(GridUpdateType code, object param) => _scrollCommands.Post(new ScrollCommand(code, param));

        private static readonly GridUpdateType[] RepeatSkip = { SizeChange, SetHorizontalOffset, SetVerticalOffset, Refresh };
        private static readonly GridUpdateType[] RepeatAccum = {
            MouseWheel,
            LineUp,
            LineDown,
            PageUp,
            PageDown,
            LineLeft,
            LineRight,
            PageLeft,
            PageRight,
            FocusUp,
            FocusDown,
            FocusPageUp,
            FocusPageDown,
            FocusLeft,
            FocusRight,
            HeaderFocusLeft,
            HeaderFocusRight
        };

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
                                    batch[i + 1].Param = Convert.ToDouble(batch[i + 1].Param) + Convert.ToDouble(batch[i].Param);
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
                    _services.Log().Write(LogVerbosity.Normal, MessageCategory.Error, "VisualGridScroller exception: " + ex);
                    batch.Clear();
                }
            }
        }

        private bool IsRepeating(List<ScrollCommand> commands, int index, GridUpdateType[] scrollTypes) {
            return commands[index].UpdateType == commands[index + 1].UpdateType && scrollTypes.Contains(commands[index].UpdateType);
        }

        private async Task ExecuteCommandAsync(ScrollCommand cmd, CancellationToken token) {
            var suppress = false;
            switch (cmd.UpdateType) {
                case LineUp:
                    Points.VerticalOffset -= Points.AverageItemHeight * Convert.ToInt32(cmd.Param);
                    break;
                case LineDown:
                    Points.VerticalOffset += Points.AverageItemHeight * Convert.ToInt32(cmd.Param);
                    break;
                case LineLeft:
                    Points.HorizontalOffset -= Points.AverageItemHeight * Convert.ToInt32(cmd.Param);    // for horizontal line increment, use vertical size
                    break;
                case LineRight:
                    Points.HorizontalOffset += Points.AverageItemHeight * Convert.ToInt32(cmd.Param);    // for horizontal line increment, use vertical size
                    break;
                case PageUp:
                    Points.VerticalOffset -= Points.ViewportHeight * Convert.ToInt32(cmd.Param);
                    break;
                case PageDown:
                    Points.VerticalOffset += Points.ViewportHeight * Convert.ToInt32(cmd.Param);
                    break;
                case PageLeft:
                    Points.HorizontalOffset -= Points.ViewportWidth * Convert.ToInt32(cmd.Param);
                    break;
                case PageRight:
                    Points.HorizontalOffset += Points.ViewportWidth * Convert.ToInt32(cmd.Param);
                    break;
                case SetHorizontalOffset: {
                        var (offset, thumbtrack) = ((double, ThumbTrack))cmd.Param;
                        Points.HorizontalOffset = offset * (Points.HorizontalExtent - Points.ViewportWidth);
                        suppress = thumbtrack == ThumbTrack.Track;
                    }
                    break;
                case SetVerticalOffset: {
                        var (offset, thumbtrack) = ((double, ThumbTrack))cmd.Param;
                        Points.VerticalOffset = offset * (Points.VerticalExtent - Points.ViewportHeight);
                        suppress = thumbtrack == ThumbTrack.Track;
                    }
                    break;
                case MouseWheel:
                    Points.VerticalOffset -= Convert.ToInt32(cmd.Param);
                    break;
                case SizeChange:
                    Points.ViewportWidth = ((Size)cmd.Param).Width;
                    Points.ViewportHeight = ((Size)cmd.Param).Height;
                    break;
                case Refresh:
                case Sort:
                    break;
                case FocusUp:
                    DataGrid.SelectedIndex = new GridIndex(Math.Max(DataGrid.SelectedIndex.Row - Convert.ToInt32(cmd.Param), 0), DataGrid.SelectedIndex.Column);
                    await BringFocusedCellIntoViewAsync(token);
                    return;
                case FocusDown:
                    DataGrid.SelectedIndex = new GridIndex(Math.Min(DataGrid.SelectedIndex.Row + Convert.ToInt32(cmd.Param), DataProvider.RowCount - 1), DataGrid.SelectedIndex.Column);
                    await BringFocusedCellIntoViewAsync(token);
                    return;
                case FocusLeft:
                    DataGrid.SelectedIndex = new GridIndex(DataGrid.SelectedIndex.Row, Math.Max(DataGrid.SelectedIndex.Column - Convert.ToInt32(cmd.Param), 0));
                    await BringFocusedCellIntoViewAsync(token);
                    return;
                case FocusRight:
                    DataGrid.SelectedIndex = new GridIndex(DataGrid.SelectedIndex.Row, Math.Min(DataGrid.SelectedIndex.Column + Convert.ToInt32(cmd.Param), DataProvider.RowCount - 1));
                    await BringFocusedCellIntoViewAsync(token);
                    return;
                case FocusPageUp:
                    DataGrid.SelectedIndex = new GridIndex(Math.Max(DataGrid.SelectedIndex.Row - Convert.ToInt32(cmd.Param), 0), DataGrid.SelectedIndex.Column);
                    await BringFocusedCellIntoViewAsync(token);
                    return;
                case FocusPageDown:
                    DataGrid.SelectedIndex = new GridIndex(Math.Min(DataGrid.SelectedIndex.Row + Convert.ToInt32(cmd.Param), DataProvider.RowCount - 1), DataGrid.SelectedIndex.Column);
                    await BringFocusedCellIntoViewAsync(token);
                    return;
                case SetFocus:
                    DataGrid.SelectedIndex = (GridIndex)cmd.Param;
                    await BringFocusedCellIntoViewAsync(token);
                    return;
                case HeaderFocusLeft:
                    ColumnHeader.SelectedIndex = new GridIndex(ColumnHeader.SelectedIndex.Row, Math.Max(ColumnHeader.SelectedIndex.Column - Convert.ToInt32(cmd.Param), 0));
                    await BringFocusedHeaderIntoViewAsync(token);
                    return;
                case HeaderFocusRight:
                    ColumnHeader.SelectedIndex = new GridIndex(ColumnHeader.SelectedIndex.Row, Math.Min(ColumnHeader.SelectedIndex.Column + Convert.ToInt32(cmd.Param), DataProvider.RowCount - 1));
                    await BringFocusedHeaderIntoViewAsync(token);
                    return;
                case SetHeaderFocus:
                    ColumnHeader.SelectedIndex = (GridIndex)cmd.Param;
                    await BringFocusedHeaderIntoViewAsync(token);
                    return;
                case Invalid:
                default:
                    return;
            }

            await DrawVisualsAsync(cmd.UpdateType, suppress, token, _owner.ColumnHeader?.SortOrder);
        }

        private async Task DrawVisualsAsync(GridUpdateType updateType, bool suppressNotification, CancellationToken token, ISortOrder sortOrder) {
            var visualViewport = new Rect(Points.HorizontalOffset, Points.VerticalOffset, Points.ViewportWidth, Points.ViewportHeight);
            var newViewport = Points.ComputeDataViewport(visualViewport);

            if (newViewport.Rows.Count < 1 || newViewport.Columns.Count < 1) {
                return;
            }

            // pull data from provider
            try {
                var data = await DataProvider.GetAsync(newViewport, sortOrder);
                if (data == null) {
                    throw new InvalidOperationException("Couldn't fetch grid data");
                }

                if (!data.Grid.Range.Contains(newViewport) ||
                    !data.ColumnHeader.Range.Contains(newViewport.Columns) ||
                    !data.RowHeader.Range.Contains(newViewport.Rows)) {
                    throw new InvalidOperationException("Couldn't acquire enough data");
                }

                await DrawVisualsAsync(newViewport, data, updateType, visualViewport, suppressNotification, token);
            } catch (OperationCanceledException) { }
        }

        private async Task DrawVisualsAsync(GridRange dataViewport, IGridData<string> data, GridUpdateType updateType, Rect visualViewport, bool suppressNotification, CancellationToken token) {
            await _services.MainThread().SwitchToAsync(token);

            DataGrid.SelectedIndex = new GridIndex(Math.Min(DataGrid.SelectedIndex.Row, DataProvider.RowCount - 1), Math.Min(DataGrid.SelectedIndex.Column, DataProvider.ColumnCount - 1));
            var columnsData = new RangeToGrid<string>(dataViewport.Columns, data.ColumnHeader, true);
            var rowsData = new RangeToGrid<string>(dataViewport.Rows, data.RowHeader, false);
            var cellsData = data.Grid;

            using (Points.DeferChangeNotification(suppressNotification)) {
                // measure points
                ColumnHeader.MeasurePoints(
                    Points.GetAccessToPoints(ColumnHeader.ScrollDirection),
                    new GridRange(new Range(0, 1), dataViewport.Columns),
                    columnsData,
                    updateType);

                RowHeader.MeasurePoints(
                    Points.GetAccessToPoints(RowHeader.ScrollDirection),
                    new GridRange(dataViewport.Rows, new Range(0, 1)),
                    rowsData,
                    updateType == Sort ? Refresh : updateType);

                DataGrid.MeasurePoints(
                    Points.GetAccessToPoints(DataGrid.ScrollDirection),
                    dataViewport,
                    cellsData,
                    updateType == Sort ? Refresh : updateType);

                // adjust Offset in case of overflow
                if ((Points.HorizontalOffset + visualViewport.Width).GreaterOrCloseTo(Points.HorizontalExtent)) {
                    Points.HorizontalOffset = Points.HorizontalExtent - visualViewport.Width;
                }
                if ((Points.VerticalOffset + visualViewport.Height).GreaterOrCloseTo(Points.VerticalExtent)) {
                    Points.VerticalOffset = Points.VerticalExtent - visualViewport.Height;
                }

                if (updateType == ScrollIntoView) {
                    AdjustFocusedCellOffset(visualViewport);
                }

                // arrange and draw gridline
                ColumnHeader.ArrangeVisuals(Points.GetAccessToPoints(ColumnHeader.ScrollDirection));
                RowHeader.ArrangeVisuals(Points.GetAccessToPoints(RowHeader.ScrollDirection));
                DataGrid.ArrangeVisuals(Points.GetAccessToPoints(DataGrid.ScrollDirection));

                Points.ViewportHeight = DataGrid.RenderSize.Height;
                Points.ViewportWidth = DataGrid.RenderSize.Width;
            }

            DataViewport = dataViewport;
            Data = data;
            _owner.AutomationPeer?.Update();
        }

        private void AdjustFocusedCellOffset(Rect visualViewport) {
            var focusRect = Points.GetBounds(DataGrid.SelectedIndex.Row, DataGrid.SelectedIndex.Column);
            if (!visualViewport.ContainsOrCloseTo(focusRect) && visualViewport.IntersectsWith(focusRect)) {
                if (focusRect.Top.LessThan(0)) {
                    Points.VerticalOffset += focusRect.Top;
                } else if (focusRect.Bottom.GreaterThan(visualViewport.Height)) {
                    Points.VerticalOffset += focusRect.Bottom - visualViewport.Height;
                }

                if (focusRect.Left.LessThan(0)) {
                    Points.HorizontalOffset += focusRect.Left;
                } else if (focusRect.Right.GreaterThan(visualViewport.Width)) {
                    Points.HorizontalOffset += focusRect.Right - visualViewport.Width;
                }
            }
        }

        private Task BringFocusedCellIntoViewAsync(CancellationToken cancellationToken) {
            var focusRect = Points.GetBounds(DataGrid.SelectedIndex.Row, DataGrid.SelectedIndex.Column);
            var viewportSize = new Size(Math.Min(DataGrid.ActualWidth, Points.ViewportWidth), Math.Min(DataGrid.ActualHeight, Points.ViewportHeight));
            return BringIntoViewAsync(focusRect, viewportSize, DataGrid.SelectedIndex.Row, DataGrid.SelectedIndex.Column, cancellationToken);
        }

        private Task BringFocusedHeaderIntoViewAsync(CancellationToken cancellationToken) {
            var column = ColumnHeader.SelectedIndex.Column;
            var headerFocusRect = new Rect(Points.xPosition(column), 0, Points.GetWidth(column), Points.ColumnHeaderHeight);
            var viewportSize = new Size(Math.Min(DataGrid.ActualWidth, Points.ViewportWidth), Points.ColumnHeaderHeight);
            return BringIntoViewAsync(headerFocusRect, viewportSize, DataGrid.SelectedIndex.Row, ColumnHeader.SelectedIndex.Column, cancellationToken);
        }

        private async Task BringIntoViewAsync(Rect focusRect, Size viewportSize, long row, long column, CancellationToken cancellationToken) {
            if (new Rect(viewportSize).ContainsOrCloseTo(focusRect)) {
                var visualViewport = new Rect(Points.HorizontalOffset, Points.VerticalOffset, Points.ViewportWidth, Points.ViewportHeight);
                await DrawVisualsAsync(DataViewport, Data, ScrollIntoView, visualViewport, false, cancellationToken);
            } else {
                await ScrollIntoViewAsync(row, column, cancellationToken);
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
