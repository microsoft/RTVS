using System;
using Microsoft.Languages.Editor;
using Microsoft.Languages.Editor.Controller.Command;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Packages.R;

namespace Microsoft.VisualStudio.R.Package.Help {
    internal sealed class HelpRefreshCommand : Command {
        private HelpWindowPane _pane;

        public HelpRefreshCommand(HelpWindowPane pane) :
            base(new CommandId(RGuidList.RCmdSetGuid, RPackageCommandId.icmdHelpRefresh)) {
            _pane = pane;
        }

        public override CommandStatus Status(Guid group, int id) {
            if (_pane.Browser != null && _pane.Browser.Source != null) {
                return CommandStatus.SupportedAndEnabled;
            }
            return CommandStatus.Supported;
        }

        public override CommandResult Invoke(Guid group, int id, object inputArg, ref object outputArg) {
            _pane.Browser.Refresh();
            return CommandResult.Executed;
        }
    }
}
