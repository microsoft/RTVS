using System;
using Microsoft.Languages.Editor;
using Microsoft.Languages.Editor.Controller.Command;
using Microsoft.VisualStudio.R.Packages.R;

namespace Microsoft.VisualStudio.R.Package.Plots.Commands {
    internal class PlotWindowCommand : Command {
        protected PlotWindowPane _pane;

        public PlotWindowCommand(PlotWindowPane pane, int id) : base(new CommandId(RGuidList.RCmdSetGuid, id), false) {
            _pane = pane;
            CurrentStatus = CommandStatus.Supported;
        }

        public override CommandStatus Status(Guid group, int id) {
            return CurrentStatus;
        }

        protected CommandStatus CurrentStatus { get; private set; }

        public void Enable() {
            CurrentStatus |= CommandStatus.Enabled;
        }

        public void Disable() {
            CurrentStatus &= ~CommandStatus.Enabled;
        }
    }
}
