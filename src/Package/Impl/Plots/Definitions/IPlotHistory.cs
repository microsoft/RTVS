using System;
using System.Threading.Tasks;

namespace Microsoft.VisualStudio.R.Package.Plots.Definitions {
    internal interface IPlotHistory: IDisposable {
        int PlotCount { get; }
        int ActivePlotIndex { get; }
        IPlotContentProvider PlotContentProvider { get; }
        Task RefreshHistoryInfo();

        event EventHandler HistoryChanged;
    }
}
