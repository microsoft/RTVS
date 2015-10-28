using System;
using Microsoft.Languages.Editor;
using Microsoft.VisualStudio.R.Package.Commands;

namespace Microsoft.VisualStudio.R.Package.Plots.Commands {
    internal sealed class SavePlotCommand : PlotWindowCommand {
        public SavePlotCommand(PlotWindowPane pane) :
            base(pane, RPackageCommandId.icmdSavePlot) { }

        public override CommandResult Invoke(Guid group, int id, object inputArg, ref object outputArg) {
            _pane.SavePlot();

            return CommandResult.Executed;
        }
    }
}
