using System;
using System.Windows.Controls;
using Microsoft.Languages.Editor;
using Microsoft.Languages.Editor.Controller.Command;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Packages.R;

namespace Microsoft.VisualStudio.R.Package.Help {
    internal sealed class HelpHomeCommand : Command {
        public const string HomeUrl = "http://rtvs";

        private WebBrowser _browser;

        public HelpHomeCommand(WebBrowser browser) :
            base(new CommandId(RGuidList.RCmdSetGuid, RPackageCommandId.icmdHelpHome)) {
            _browser = browser;
        }

        public override CommandStatus Status(Guid group, int id) {
            return _browser != null ? CommandStatus.SupportedAndEnabled : CommandStatus.Supported;
        }

        public override CommandResult Invoke(Guid group, int id, object inputArg, ref object outputArg) {
            if (_browser != null) {
                _browser.Navigate(HelpHomeCommand.HomeUrl);
            }
            return CommandResult.Executed;
        }
    }
}
