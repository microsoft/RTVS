using System;
using System.Threading.Tasks;
using System.Windows;

namespace Microsoft.VisualStudio.R.Package.Plots {
    /// <summary>
    /// Plot content provider to load and consume plot content
    /// </summary>
    internal interface IPlotContentProvider : IDisposable {
        /// <summary>
        /// Event raised when UIElement is loaded, content presenter listens to this event
        /// </summary>
        event EventHandler<PlotChangedEventArgs> PlotChanged;

        /// <summary>
        /// Load a file to create plot UIElement
        /// </summary>
        /// <param name="filePath">path to a file</param>
        void LoadFile(string filePath);

        /// <summary>
        /// Export plot as an image
        /// </summary>
        /// <param name="fileName">the destination filepath</param>
        void ExportFile(string fileName);

        /// <summary>
        /// Resize the current plot or set the default size for future plots.
        /// </summary>
        /// <param name="width">Width in pixels.</param>
        /// <param name="height">Height in pixels.</param>
        void ResizePlot(int width, int height);

        /// <summary>
        /// Navigate to the next plot in the plot history.
        /// </summary>
        void NextPlot();

        /// <summary>
        /// Navigate to the previous plot in the plot history.
        /// </summary>
        void PreviousPlot();

        /// <summary>
        /// Get the plot history information.
        /// </summary>
        /// <returns>Tuple of active index (0-based, -1 for none) and number of plots.</returns>
        Task<Tuple<int, int>> GetHistoryInfo();
    }

    /// <summary>
    /// provide data ralated to PlotChanged event
    /// </summary>
    internal class PlotChangedEventArgs : EventArgs {
        /// <summary>
        /// new plot UIElement
        /// </summary>
        public UIElement NewPlotElement { get; set; }
    }
}
