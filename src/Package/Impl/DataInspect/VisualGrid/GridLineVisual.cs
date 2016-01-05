using System.Windows;
using System.Windows.Media;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    internal class GridLineVisual : DrawingVisual {
        public double GridLineThickness { get { return 1.0; } }

        public Brush GridLineBrush { get { return Brushes.Black; } }

        public void Draw(
            GridRange range,
            GridPoints points) {

            DrawingContext drawingContext = RenderOpen();
            DoubleCollection xCollection = new DoubleCollection();
            DoubleCollection yCollection = new DoubleCollection();

            try {
                // vertical line
                double renderHeight = points.GetHeight(range.Rows);
                Rect verticalLineRect = new Rect(new Size(GridLineThickness, renderHeight));
                foreach (int i in range.Columns.GetEnumerable()) {
                    verticalLineRect.X = points.xPosition(i + 1) - GridLineThickness;
                    drawingContext.DrawRectangle(GridLineBrush, null, verticalLineRect);
                    xCollection.Add(verticalLineRect.X);
                }

                // horizontal line
                double renderWidth = points.GetWidth(range.Columns);
                Rect horizontalLineRect = new Rect(new Size(renderWidth, GridLineThickness));
                foreach (int i in range.Rows.GetEnumerable()) {
                    horizontalLineRect.Y = points.yPosition(i + 1) - GridLineThickness;
                    drawingContext.DrawRectangle(GridLineBrush, null, horizontalLineRect);
                    yCollection.Add(horizontalLineRect.Y);
                }

                XSnappingGuidelines = xCollection;
                YSnappingGuidelines = yCollection;
            } finally {
                drawingContext.Close();
            }
        }
    }
}
