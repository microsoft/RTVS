using System;
using Microsoft.Languages.Editor;
using Microsoft.VisualStudio.R.Package.Commands;

namespace Microsoft.VisualStudio.R.Package.Plots.Commands {
    internal sealed class HistoryPreviousPlotCommand : PlotWindowCommand {
        public HistoryPreviousPlotCommand(PlotWindowPane pane) :
            base(pane, RPackageCommandId.icmdPrevPlot) {
        }

        public override CommandStatus Status(Guid group, int id) {
            return CommandStatus.NotSupported;
            //return CommandStatus.SupportedAndEnabled;
        }

        public override CommandResult Invoke(Guid group, int id, object inputArg, ref object outputArg) {
            _pane.PreviousPlot();
            return CommandResult.Executed;
        }
    }
}
