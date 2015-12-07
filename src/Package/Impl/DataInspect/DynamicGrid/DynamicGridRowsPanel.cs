using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    internal class DynamicGridRowsPanel : VirtualizingStackPanel {
        private DynamicGrid _owningGrid;

        internal void HookOwner() {
            if (_owningGrid == null) {
                DynamicGrid owner = ItemsControl.GetItemsOwner(this) as DynamicGrid;
                if (owner == null) {
                    throw new NotSupportedException("hey!");
                }
                _owningGrid = owner;
            }
        }

        internal void ReportSize(Size availableSize) {
            HookOwner();

            _owningGrid.OnReportPanelSize(availableSize);
        }

        protected override Size MeasureOverride(Size constraint) {
            ReportSize(constraint);

            return base.MeasureOverride(constraint);
        }
    }
}
