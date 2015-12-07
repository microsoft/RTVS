using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    /// <summary>
    /// Grid item container that maps to a cell in grid
    /// </summary>
    public class DynamicGridCell : ContentControl {
        static DynamicGridCell() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DynamicGridCell), new FrameworkPropertyMetadata(typeof(DynamicGridCell)));
        }

        internal LinkedListNode<DynamicGridCell> Track { get; private set; }

        internal DynamicGridStripe ColumnStripe { get; set; }

        internal virtual void Prepare(DynamicGridStripe columnStipe) {
            if (ColumnStripe != null) {
                ColumnStripe.LayoutSize.MaxChanged -= LayoutSize_MaxChanged;
            }

            ColumnStripe = columnStipe;
            ColumnStripe.LayoutSize.MaxChanged += LayoutSize_MaxChanged;

            Track = new LinkedListNode<DynamicGridCell>(this);
        }

        private void LayoutSize_MaxChanged(object sender, EventArgs e) {
            InvalidateMeasure();
        }

        /// <summary>
        /// Clean up data when virtualized
        /// </summary>
        internal virtual void CleanUp() {
            this.Content = null;

            if (ColumnStripe != null) {
                ColumnStripe.LayoutSize.MaxChanged -= LayoutSize_MaxChanged;
            }
            ColumnStripe = null;
        }

        protected override Size MeasureOverride(Size constraint) {
            if (ColumnStripe != null) {
                constraint.Width = Math.Min(constraint.Width, ColumnStripe.GetSizeConstraint());
            }

            Size desired = base.MeasureOverride(constraint);

            if (ColumnStripe != null) {
                ColumnStripe.LayoutSize.Max = desired.Width;
                desired.Width = ColumnStripe.LayoutSize.Max;
            }

            return desired;
        }

        protected override Size ArrangeOverride(Size arrangeBounds) {
            if (ColumnStripe != null) {
                arrangeBounds.Width = ColumnStripe.LayoutSize.Max;
            }

            return base.ArrangeOverride(arrangeBounds);
        }
    }
}
