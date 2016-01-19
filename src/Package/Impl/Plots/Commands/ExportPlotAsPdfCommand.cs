using System;
using Microsoft.Languages.Editor;
using Microsoft.VisualStudio.R.Package.Commands;

namespace Microsoft.VisualStudio.R.Package.Plots.Commands {
    internal sealed class ExportPlotAsPdfCommand : PlotWindowCommand {
        public ExportPlotAsPdfCommand(PlotWindowPane pane) :
            base(pane, RPackageCommandId.icmdExportPlotAsPdf) {
        }

        public override CommandResult Invoke(Guid group, int id, object inputArg, ref object outputArg) {
            _pane.ExportPlotAsPdf();
            return CommandResult.Executed;
        }
    }
}
