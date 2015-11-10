using System;
using System.Windows.Controls;
using Microsoft.Languages.Editor;
using Microsoft.Languages.Editor.Controller.Command;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Packages.R;

namespace Microsoft.VisualStudio.R.Package.Help {
    internal sealed class HelpPreviousCommand : Command {
        private WebBrowser _browser;

        public HelpPreviousCommand(WebBrowser browser) :
            base(new CommandId(RGuidList.RCmdSetGuid, RPackageCommandId.icmdHelpPrevious)) {
            _browser = browser;
        }

        public override CommandStatus Status(Guid group, int id) {
            if (_browser != null && _browser.CanGoBack) {
                return CommandStatus.SupportedAndEnabled;
            }
            return CommandStatus.Supported;
        }

        public override CommandResult Invoke(Guid group, int id, object inputArg, ref object outputArg) {
            _browser.GoBack();
            return CommandResult.Executed;
        }
    }
}
