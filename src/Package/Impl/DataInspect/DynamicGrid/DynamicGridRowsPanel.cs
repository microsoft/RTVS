using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    internal class DynamicGridRowsPanel : VirtualizingStackPanel {
        private DynamicGrid _owningGrid;
        internal DynamicGrid OwningGrid {
            get {
                if (_owningGrid == null) {
                    _owningGrid = ItemsControl.GetItemsOwner(this) as DynamicGrid;

                    Debug.Assert(_owningGrid != null, "DynamicGridRowsPanel supports only DynamicGrid");
                }
                return _owningGrid;
            }
        }

        protected override void OnViewportSizeChanged(Size oldViewportSize, Size newViewportSize) {
            base.OnViewportSizeChanged(oldViewportSize, newViewportSize);

            DynamicGrid grid = OwningGrid;
            if (grid != null) {
                grid.OnViewportSizeChanged(newViewportSize);
            }
        }
    }
}
