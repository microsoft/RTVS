using System;
using System.Windows;

namespace Microsoft.VisualStudio.R.Package.Plots
{
    /// <summary>
    /// Plot content provider to load and consume plot content
    /// </summary>
    internal interface IPlotContentProvider
    {
        /// <summary>
        /// Event raised when UIElement is loaded, content presenter listens to this event
        /// </summary>
        event EventHandler<PlotChangedEventArgs> PlotChanged;

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
    }

    /// <summary>
    /// provide data ralated to PlotChanged event
    /// </summary>
    internal class PlotChangedEventArgs : EventArgs
    {
        /// <summary>
        /// new plot UIElement
        /// </summary>
        public UIElement NewPlotElement { get; set; }
    }
}
