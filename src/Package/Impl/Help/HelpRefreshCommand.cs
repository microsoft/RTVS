using System;
using System.Windows.Controls;
using Microsoft.Languages.Editor;
using Microsoft.Languages.Editor.Controller.Command;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Packages.R;

namespace Microsoft.VisualStudio.R.Package.Help {
    internal sealed class HelpRefreshCommand : Command {
        private WebBrowser _browser;

        public HelpRefreshCommand(WebBrowser browser) :
            base(new CommandId(RGuidList.RCmdSetGuid, RPackageCommandId.icmdHelpRefresh)) {
            _browser = browser;
        }

        public override CommandStatus Status(Guid group, int id) {
            return _browser != null ? CommandStatus.SupportedAndEnabled : CommandStatus.Supported;
        }

        public override CommandResult Invoke(Guid group, int id, object inputArg, ref object outputArg) {
            if (_browser != null) {
                _browser.Refresh();
            }
            return CommandResult.Executed;
        }
    }
}
