// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Common.Core;
using Microsoft.R.Components.Plots.ViewModel;

namespace Microsoft.R.Components.Plots.Implementation.View {
    /// <summary>
    /// Interaction logic for RPlotHistoryControl.xaml
    /// </summary>
    public partial class RPlotHistoryControl {
        private readonly DragSurface _dragSurface = new DragSurface();

        /// <summary>
        /// Show context menu at the specified point, which is relative
        /// to the visual component control (ie. this control).
        /// </summary>
        public event EventHandler<PointEventArgs> ContextMenuRequested;


        public RPlotHistoryControl() {
            InitializeComponent();
        }

        private void item_MouseDoubleClick(object sender, MouseEventArgs e) {
            var entry = (IRPlotHistoryEntryViewModel)((ListViewItem)sender).Content;
            ActivatePlot(entry);
        }

        private void item_KeyUp(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                var entry = (IRPlotHistoryEntryViewModel)(((ListViewItem)sender).Content);
                ActivatePlot(entry);
            } else if (e.Key == Key.Apps || (e.SystemKey == Key.F10 && (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)) {
                var item = (ListViewItem)sender;
                var point = item.TranslatePoint(new Point(1, 1), this);
                ContextMenuRequested?.Invoke(this, new PointEventArgs(point));
                e.Handled = true;
            }
        }

        private void ActivatePlot(IRPlotHistoryEntryViewModel entry)
            => entry.ActivatePlotAsync().DoNotWait();

        private void HistoryListView_MouseMove(object sender, MouseEventArgs e) {
            if (_dragSurface.IsMouseMoveStartingDrag(e)) {
                var plots = (DataContext as IRPlotHistoryViewModel)?.SelectedPlots?.Select(x => x.Plot).ToArray();
                if (plots != null && plots.Length > 0) {
                    var obj = PlotClipboardData.ToDataObject(plots);
                    DragDrop.DoDragDrop(HistoryListView, obj, DragDropEffects.Copy | DragDropEffects.Move);
                    return;
                }
            }
            _dragSurface.MouseMove(e);
        }

        private void HistoryListView_MouseLeave(object sender, MouseEventArgs e) => _dragSurface.MouseLeave(e);

        private void HistoryListView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            _dragSurface.MouseDown(e);
            var count = SelectedPlots.Count();
            if (count == 0) {
                return; // Allow selection
            }

            var control = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
            if (!control) {
                // Regular click clear selection then reselects
                HistoryListView.SelectedItems.Clear();
            }
        }

        private void HistoryListView_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e) => e.Handled = true;

        private void HistoryListView_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e) {
            var point = e.GetPosition(this);
            ContextMenuRequested?.Invoke(this, new PointEventArgs(point));
            e.Handled = true;
        }

        private void HistoryListView_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e) => e.Handled = true;

        private IEnumerable<IRPlot> SelectedPlots {
            get {
                var selection = (DataContext as IRPlotHistoryViewModel)?.SelectedPlots;
                return selection?.Select(p => p.Plot) ?? Enumerable.Empty<IRPlot>();
            }
        }
    }
}
