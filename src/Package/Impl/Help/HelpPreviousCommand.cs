using System;
using Microsoft.Languages.Editor;
using Microsoft.Languages.Editor.Controller.Command;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Packages.R;

namespace Microsoft.VisualStudio.R.Package.Help {
    internal sealed class HelpPreviousCommand : Command {
        private IHelpWindowVisualComponent _component;

        public HelpPreviousCommand(IHelpWindowVisualComponent component) :
            base(new CommandId(RGuidList.RCmdSetGuid, RPackageCommandId.icmdHelpPrevious)) {
            _component = component;
        }

        public override CommandStatus Status(Guid group, int id) {
            if (_component.Browser != null && _component.Browser.CanGoBack) {
                return CommandStatus.SupportedAndEnabled;
            }
            return CommandStatus.Supported;
        }

        public override CommandResult Invoke(Guid group, int id, object inputArg, ref object outputArg) {
            _component.Browser.GoBack();
            return CommandResult.Executed;
        }
    }
}
