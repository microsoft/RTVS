using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    internal class DynamicGridRow : ItemsControl, SharedScrollInfo {
        private LinkedList<DynamicGridCell> _realizedCells = new LinkedList<DynamicGridCell>();

        static DynamicGridRow() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DynamicGridRow), new FrameworkPropertyMetadata(typeof(DynamicGridRow)));
        }

        public DynamicGridRow() {
            Track = new LinkedListNode<DynamicGridRow>(this);
        }

        internal LinkedListNode<DynamicGridRow> Track { get; }

        public static readonly DependencyProperty HeaderProperty =
                DependencyProperty.Register(
                        "Header",
                        typeof(object),
                        typeof(DynamicGridRow),
                        new FrameworkPropertyMetadata(null));

        public object Header {
            get { return GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        internal DynamicGrid ParentGrid { get; private set; }

        #region SharedScrollInfo support

        public LayoutInfo GetLayoutInfo(Size size) {
            Debug.Assert(ParentGrid != null);
            return ParentGrid.GetLayoutInfo(size);
        }

        public event EventHandler SharedScrollChanged;

        #endregion

        protected override DependencyObject GetContainerForItemOverride() {
            return new DynamicGridCell();
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item) {
            base.PrepareContainerForItemOverride(element, item);

            var cell = (DynamicGridCell)element;
            int column = this.Items.IndexOf(item);
            if (column == -1) {
                throw new InvalidOperationException("Item is not found in collection");
            }
            cell.Prepare(ParentGrid.GetColumn(column));

            _realizedCells.AddFirst(cell.Track);
        }

        protected override void ClearContainerForItemOverride(DependencyObject element, object item) {
            base.ClearContainerForItemOverride(element, item);

            var cell = (DynamicGridCell)element;
            _realizedCells.Remove(cell.Track);
            cell.CleanUp();
        }

        internal void Prepare(DynamicGrid owner, object item) {
            if (!(item is IList)) {
                throw new NotSupportedException("JointCollectionGridRow supports only IList for item");
            }

            ParentGrid = owner;

            var items = (IList)item;
            ItemsSource = items;
        }

        internal void CleanUp(DynamicGrid owner, object item) {
            // when VirtualizationMode == Recycling, next lines must not be called as system calls them
            var mode = VirtualizingPanel.GetVirtualizationMode(ParentGrid);
            if (mode != VirtualizationMode.Recycling) {
                foreach (var cell in _realizedCells) {
                    cell.CleanUp();
                }
                _realizedCells.Clear();
            }
        }

        internal void ScrollChanged() {
            if (SharedScrollChanged != null) {
                SharedScrollChanged(this, EventArgs.Empty);
            }
        }

        internal DynamicGridRowHeader RowHeader { get; set; }

        internal void NotifyRowHeader() {
            if (RowHeader != null) {
                RowHeader.InvalidateMeasure();
            }
        }
    }
}
