// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
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
    public partial class RPlotHistoryControl : UserControl {
        private IRPlotHistoryViewModel ViewModel => (IRPlotHistoryViewModel)DataContext;
        private DragSurface _dragSurface = new DragSurface();

        public event EventHandler<MouseButtonEventArgs> ContextMenuRequested;


        public RPlotHistoryControl() {
            InitializeComponent();
        }

        private void item_MouseDoubleClick(object sender, MouseEventArgs e) {
            var entry = (IRPlotHistoryEntryViewModel)(((ListViewItem)sender).Content);
            ActivatePlot(entry);
        }

        private void item_KeyUp(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                var entry = (IRPlotHistoryEntryViewModel)(((ListViewItem)sender).Content);
                ActivatePlot(entry);
            }
        }

        private void ActivatePlot(IRPlotHistoryEntryViewModel entry) {
            entry.ActivatePlotAsync().DoNotWait();
        }

        private void HistoryListView_MouseMove(object sender, MouseEventArgs e) {
            if (_dragSurface.IsMouseMoveStartingDrag(e)) {
                var entry = GetSourceListViewItem((DependencyObject)e.OriginalSource)?.DataContext as IRPlotHistoryEntryViewModel;
                if (entry != null) {
                    var data = PlotClipboardData.Serialize(new PlotClipboardData(entry.Plot.ParentDevice.DeviceId, entry.Plot.PlotId, false));
                    var obj = new DataObject(PlotClipboardData.Format, data);
                    DragDrop.DoDragDrop(HistoryListView, obj, DragDropEffects.Copy | DragDropEffects.Move);
                    return;
                }
            }
            _dragSurface.MouseMove(e);
        }

        private void HistoryListView_MouseLeave(object sender, MouseEventArgs e) {
            _dragSurface.MouseLeave(e);
        }

        private void HistoryListView_PreviewMouseDown(object sender, MouseButtonEventArgs e) {
            _dragSurface.MouseDown(e);
        }

        private void HistoryListView_MouseRightButtonUp(object sender, MouseButtonEventArgs e) {
            // TODO: also support keyboard (Shift+F10 or context menu key)
            ContextMenuRequested?.Invoke(sender, e);
        }

        private static ListViewItem GetSourceListViewItem(DependencyObject obj) {
            do {
                var item = obj as ListViewItem;
                if (item != null) {
                    return item;
                }

                obj = VisualTreeHelper.GetParent(obj);
            } while (obj != null);

            return null;
        }
    }
}
