using System;
using Microsoft.Languages.Editor;
using Microsoft.VisualStudio.R.Package.Commands;

namespace Microsoft.VisualStudio.R.Package.Plots.Commands {
    internal sealed class HistoryNextPlotCommand : PlotWindowCommand {
        public HistoryNextPlotCommand(PlotWindowPane pane) :
            base(pane, RPackageCommandId.icmdNextPlot) {
        }

        public override CommandResult Invoke(Guid group, int id, object inputArg, ref object outputArg) {
            _pane.NextPlot();
            return CommandResult.Executed;
        }
    }
}
