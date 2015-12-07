using System;
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
        /// Loads file on next idle time. Typically used
        /// in R plotting where several files may get produced
        /// in a fast succession. Eliminates multiple file loads.
        /// </summary>
        /// <param name="filePath"></param>
        void LoadFileOnIdle(string filePath);

        /// <summary>
        /// Load a file to create plot UIElement
        /// </summary>
        /// <param name="filePath">path to a file</param>
        void LoadFile(string filePath);

        /// <summary>
        /// Copy last loaded file to destination
        /// </summary>
        /// <param name="fileName">the destination filepath</param>
        void SaveFile(string fileName);

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
        void NextPlot();
        void PreviousPlot();
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
