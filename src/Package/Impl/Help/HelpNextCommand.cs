using System;
using System.Windows.Controls;
using Microsoft.Languages.Editor;
using Microsoft.Languages.Editor.Controller.Command;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Packages.R;

namespace Microsoft.VisualStudio.R.Package.Help {
    internal sealed class HelpNextCommand : Command {
        private WebBrowser _browser;

        public HelpNextCommand(WebBrowser browser) :
            base(new CommandId(RGuidList.RCmdSetGuid, RPackageCommandId.icmdHelpNext)) {
            _browser = browser;
        }

        public override CommandStatus Status(Guid group, int id) {
            if (_browser != null && _browser.CanGoForward) {
                return CommandStatus.SupportedAndEnabled;
            }
            return CommandStatus.Supported;
        }

        public override CommandResult Invoke(Guid group, int id, object inputArg, ref object outputArg) {
            _browser.GoForward();
            return CommandResult.Executed;
        }
    }
}
