using Microsoft.R.Host.Client;

namespace Microsoft.VisualStudio.R.Package.Plots.Definitions {
    internal interface IPlotHistoryProvider {
        IPlotHistory GetPlotHistory(IRSession session);
    }
}
