using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.VisualStudio.R.Package.Wpf;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    /// <summary>
    /// A control that contains row header content
    /// Width is synched with other row's header width through owning grid's property
    /// </summary>
    internal class DynamicGridRowHeader : ContentControl {
        public override void OnApplyTemplate() {
            base.OnApplyTemplate();

            DynamicGridRow row = ParentRow;
            if (row != null) {
                row.RowHeader = this;
            }
        }

        protected override Size MeasureOverride(Size constraint) {
            Size adjusted = DynamicGridUtilities.DecreaseSize(constraint, LineThickness);

            Size baseSize = base.MeasureOverride(adjusted);

            baseSize.Width += LineThickness;
            baseSize.Height += LineThickness;

            var grid = ParentGrid;
            if (grid == null) {
                return baseSize;
            }

            if (baseSize.Width > grid.RowHeaderActualWidth) {
                grid.RowHeaderActualWidth = baseSize.Width;
            }

            return new Size(grid.RowHeaderActualWidth, baseSize.Height);
        }

        private double LineThickness = 1.0;
        protected override Size ArrangeOverride(Size arrangeBounds) {
            Size adjustedBounds = DynamicGridUtilities.DecreaseSize(arrangeBounds, LineThickness);
            
            Size arranged = base.ArrangeOverride(adjustedBounds);

            arranged.Width += LineThickness;
            arranged.Height += LineThickness;

            return arranged;
        }

        protected override void OnRender(DrawingContext drawingContext) {
            base.OnRender(drawingContext);

            // vertical line
            {
                Rect rect = new Rect(new Size(LineThickness, RenderSize.Height));
                rect.X = RenderSize.Width - LineThickness;

                drawingContext.DrawRectangle(ParentGrid.HeaderLinesBrush, null, rect);
            }

            // horizontal line
            {
                Rect rect = new Rect(new Size(RenderSize.Width, LineThickness));
                rect.Y = RenderSize.Height - LineThickness;

                drawingContext.DrawRectangle(ParentGrid.HeaderLinesBrush, null, rect);
            }
        }

        private DynamicGridRow _parentRow;
        internal DynamicGridRow ParentRow {
            get {
                if (_parentRow == null) {
                    _parentRow = WpfHelper.FindParent<DynamicGridRow>(this);
                }
                return _parentRow;
            }
        }

        internal DynamicGrid ParentGrid {
            get {
                return ParentRow?.ParentGrid;
            }
        }
    }
}
