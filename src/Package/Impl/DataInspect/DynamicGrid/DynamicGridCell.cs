using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    /// <summary>
    /// Item container in <see cref="DynamicGridRow"/>
    /// </summary>
    public class DynamicGridCell : ContentControl {
        static DynamicGridCell() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DynamicGridCell), new FrameworkPropertyMetadata(typeof(DynamicGridCell)));
        }

        public DynamicGridCell() {
            Track = new LinkedListNode<DynamicGridCell>(this);
        }

        internal DynamicGridRow ParentRow { get; private set; }

        internal DynamicGrid ParentGrid { get { return ParentRow?.ParentGrid; } }

        internal LinkedListNode<DynamicGridCell> Track { get; private set; }

        internal MaxDouble ColumnWidth { get; set; }

        internal virtual void Prepare(DynamicGridRow owner, MaxDouble columnWidth) {
            ParentRow = owner;

            if (ColumnWidth != null) {
                ColumnWidth.MaxChanged -= LayoutSize_MaxChanged;
            }

            ColumnWidth = columnWidth;
            ColumnWidth.MaxChanged += LayoutSize_MaxChanged;
        }

        private void LayoutSize_MaxChanged(object sender, EventArgs e) {
            if (!object.Equals(sender, ColumnWidth)) {
                InvalidateMeasure();
            }
        }

        /// <summary>
        /// Clean up data when virtualized
        /// </summary>
        internal virtual void CleanUp() {
            this.Content = null;

            if (ColumnWidth != null) {
                ColumnWidth.MaxChanged -= LayoutSize_MaxChanged;
            }
            ColumnWidth = null;

            this.ParentRow = null;
        }

        private double LineThickness = 1.0; // TODO: configurable
        protected override Size MeasureOverride(Size constraint) {

            Size adjustedConstraint = DynamicGridUtilities.DecreaseSize(constraint, LineThickness);

            Size desired = base.MeasureOverride(adjustedConstraint);

            desired.Height += LineThickness;
            desired.Width += LineThickness;

            ColumnWidth.Max = desired.Width;
            desired.Width = ColumnWidth.Max;

            return desired;
        }

        protected override Size ArrangeOverride(Size arrangeBounds) {
            Size adjustedBounds = DynamicGridUtilities.DecreaseSize(arrangeBounds, LineThickness);

            Size arrangedSize = base.ArrangeOverride(adjustedBounds);

            arrangedSize.Height += LineThickness;
            arrangedSize.Width += LineThickness;

            return arrangedSize;
        }



        protected override void OnRender(DrawingContext drawingContext) {
            base.OnRender(drawingContext);

            // vertical line
            {
                Rect rect = new Rect(new Size(LineThickness, RenderSize.Height));
                rect.X = RenderSize.Width - LineThickness;

                drawingContext.DrawRectangle(ParentGrid.GridLinesBrush, null, rect);
            }

            // horizontal line
            {
                Rect rect = new Rect(new Size(RenderSize.Width, LineThickness));
                rect.Y = RenderSize.Height - LineThickness;

                drawingContext.DrawRectangle(ParentGrid.GridLinesBrush, null, rect);
            }
        }
    }
}
