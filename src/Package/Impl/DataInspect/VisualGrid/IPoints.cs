namespace Microsoft.VisualStudio.R.Package.DataInspect {
    internal interface IPoints {
        /// <summary>
        /// X coordinate, relative to HorizontalOffset
        /// </summary>
        Indexer<double> xPosition { get; }

        /// <summary>
        /// Y coordinate, relative to VerticalOffset
        /// </summary>
        Indexer<double> yPosition { get; }

        /// <summary>
        /// Width in X direction
        /// </summary>
        Indexer<double> Width { get; }

        /// <summary>
        /// Height in Y direction
        /// </summary>
        Indexer<double> Height { get; }
    }
}
