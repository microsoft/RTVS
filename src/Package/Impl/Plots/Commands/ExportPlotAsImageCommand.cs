using System;
using Microsoft.Languages.Editor;
using Microsoft.VisualStudio.R.Package.Commands;

namespace Microsoft.VisualStudio.R.Package.Plots.Commands {
    internal sealed class ExportPlotAsImageCommand : PlotWindowCommand {
        public ExportPlotAsImageCommand(PlotWindowPane pane) :
            base(pane, RPackageCommandId.icmdExportPlotAsImage) {
        }

        public override CommandResult Invoke(Guid group, int id, object inputArg, ref object outputArg) {
            _pane.ExportPlotAsImage();
            return CommandResult.Executed;
        }
    }
}
