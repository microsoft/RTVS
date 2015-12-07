using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.VisualStudio.R.Package.Wpf;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    internal class DynamicGridRowHeader : ContentControl {
        public override void OnApplyTemplate() {
            base.OnApplyTemplate();

            DynamicGridRow row = ParentRow;
            if (row != null) {
                row.RowHeader = this;
            }
        }

        protected override Size MeasureOverride(Size constraint) {
            Size baseSize = base.MeasureOverride(constraint);

            var grid = ParentGrid;
            if (grid == null) {
                return baseSize;
            }

            if (baseSize.Width > grid.RowHeaderActualWidth) {
                grid.RowHeaderActualWidth = baseSize.Width;
            }

            return new Size(grid.RowHeaderActualWidth, baseSize.Height);
        }

        internal DynamicGridRow ParentRow {
            get {
                return WpfHelper.FindParent<DynamicGridRow>(this);
            }
        }

        internal DynamicGrid ParentGrid {
            get {
                return ParentRow.ParentGrid;
            }
        }
    }
}
