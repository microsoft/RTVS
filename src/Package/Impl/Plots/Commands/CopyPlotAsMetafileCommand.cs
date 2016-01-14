using System;
using Microsoft.Languages.Editor;
using Microsoft.Languages.Editor.Controller.Command;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Packages.R;

namespace Microsoft.VisualStudio.R.Package.Plots.Commands {
    internal sealed class CopyPlotAsMetafileCommand : PlotWindowCommand {
        public CopyPlotAsMetafileCommand(PlotWindowPane pane) :
            base(pane, RPackageCommandId.icmdCopyPlotAsMetafile) {
        }

        public override CommandResult Invoke(Guid group, int id, object inputArg, ref object outputArg) {
            _pane.PlotContentProvider.CopyToClipboardAsMetafile();
            return CommandResult.Executed;
        }
    }
}
