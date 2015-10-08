using Microsoft.Languages.Editor.Controller.Command;
using Microsoft.VisualStudio.R.Packages.R;

namespace Microsoft.VisualStudio.R.Package.Plots.Commands
{
    internal class PlotWindowCommand: Command
    {
        protected PlotWindowPane _pane;

        public PlotWindowCommand(PlotWindowPane pane, int id):
            base(new CommandId(RGuidList.RCmdSetGuid, id), false)
        {
            _pane = pane;
        }
    }
}
