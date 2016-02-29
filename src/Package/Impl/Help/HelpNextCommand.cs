// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Languages.Editor;
using Microsoft.Languages.Editor.Controller.Command;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Packages.R;

namespace Microsoft.VisualStudio.R.Package.Help {
    internal sealed class HelpNextCommand : Command {
        private IHelpWindowVisualComponent _component;

        public HelpNextCommand(IHelpWindowVisualComponent component) :
            base(new CommandId(RGuidList.RCmdSetGuid, RPackageCommandId.icmdHelpNext)) {
            _component = component;
        }

        public override CommandStatus Status(Guid group, int id) {
            if (_component.Browser != null && _component.Browser.CanGoForward) {
                return CommandStatus.SupportedAndEnabled;
            }
            return CommandStatus.Supported;
        }

        public override CommandResult Invoke(Guid group, int id, object inputArg, ref object outputArg) {
            _component.Browser.GoForward();
            return CommandResult.Executed;
        }
    }
}
