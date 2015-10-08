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

        /// <summary>
        /// Load a file to create plot UIElement
        /// </summary>
        /// <param name="filePath">path to a file</param>
        void LoadFile(string filePath);

        /// <summary>
        /// Load XAML string to parse and create plot UIElement
        /// </summary>
        /// <param name="xamlText">XAML string</param>
        void LoadXaml(string xamlText);
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
