using System;
using Microsoft.Languages.Editor;
using Microsoft.Languages.Editor.Controller.Command;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Packages.R;

namespace Microsoft.VisualStudio.R.Package.Plots.Commands {
    internal sealed class CopyPlotAsBitmapCommand : PlotWindowCommand {
        public CopyPlotAsBitmapCommand(PlotWindowPane pane) :
            base(pane, RPackageCommandId.icmdCopyPlotAsBitmap) {
        }

        public override CommandResult Invoke(Guid group, int id, object inputArg, ref object outputArg) {
            _pane.PlotContentProvider.CopyToClipboardAsBitmap();
            return CommandResult.Executed;
        }
    }
}
