using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.VisualStudio.R.Package.Wpf;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    internal class DynamicGridColumnHeadersPresenter : ItemsControl, SharedScrollInfo {
        static DynamicGridColumnHeadersPresenter() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DynamicGridColumnHeadersPresenter), new FrameworkPropertyMetadata(typeof(DynamicGridColumnHeadersPresenter)));
        }

        public override void OnApplyTemplate() {
            base.OnApplyTemplate();

            ParentGrid = WpfHelper.FindParent<DynamicGrid>(this);
            ParentGrid.ColumnHeadersPresenter = this;
        }

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
        }

        protected override void ClearContainerForItemOverride(DependencyObject element, object item) {
            base.ClearContainerForItemOverride(element, item);

            var cell = (DynamicGridCell)element;
            cell.CleanUp();
        }

        internal void ScrollChanged() {
            if (SharedScrollChanged != null) {
                SharedScrollChanged(this, EventArgs.Empty);
            }
        }

        #region SharedScrollInfo

        public event EventHandler SharedScrollChanged;

        public LayoutInfo GetLayoutInfo(Size size) {
            return ParentGrid.GetLayoutInfo(size);
        }

        #endregion

        internal DynamicGrid ParentGrid { get; private set; }
    }
}
