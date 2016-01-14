using System.Windows;
using System.Windows.Media;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    /// <summary>
    /// DrawingVisual for grid's line
    /// </summary>
    internal class GridLineVisual : DrawingVisual {
        public GridLineVisual(VisualGrid owner) {
            Owner = owner;
        }

        internal VisualGrid Owner { get; }

        private ScrollDirection ScrollDirection {
            get {
                return Owner.ScrollDirection;
            }
        }

        public double GridLineThickness { get { return 1.0; } }


        private Brush _gridLineBrush = Brushes.Black;
        public Brush GridLineBrush {
            get {
                return _gridLineBrush;
            }
            set {
                _gridLineBrush = value;
            }
        }

        public void Draw(
            GridRange range,
            GridPoints points) {

            DrawingContext drawingContext = RenderOpen();
            DoubleCollection xCollection = new DoubleCollection();
            DoubleCollection yCollection = new DoubleCollection();

            try {
                // vertical line
                double xBias = ScrollDirection == ScrollDirection.Vertical ? points.HorizontalOffset : 0;
                xBias -= GridLineThickness;

                double renderHeight = points.GetHeight(range.Rows);
                Rect verticalLineRect = new Rect(new Size(GridLineThickness, renderHeight));
                foreach (int i in range.Columns.GetEnumerable()) {
                    verticalLineRect.X = points.xPosition(i + 1) + xBias;
                    drawingContext.DrawRectangle(GridLineBrush, null, verticalLineRect);
                    xCollection.Add(verticalLineRect.X);
                }

                // horizontal line
                double yBias = ScrollDirection == ScrollDirection.Horizontal ? points.VerticalOffset : 0;
                yBias -= GridLineThickness;

                double renderWidth = points.GetWidth(range.Columns);
                Rect horizontalLineRect = new Rect(new Size(renderWidth, GridLineThickness));
                foreach (int i in range.Rows.GetEnumerable()) {
                    horizontalLineRect.Y = points.yPosition(i + 1) + yBias;
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
