// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel.Design;
using Microsoft.R.Components.Controller;
using Microsoft.VisualStudio.Shell;

namespace Microsoft.VisualStudio.R.Package.Commands {
    internal class MenuCommandToOleMenuCommandShim : PackageCommand {
        private readonly IMenuCommand _command;

        public MenuCommandToOleMenuCommandShim(IMenuCommand command, Guid group, int id)
            : base(group, id) {
            if (command == null) {
                throw new ArgumentNullException(nameof(command));
            }
            _command = command;
        }

        protected override void SetStatus() {
            var status = _command.Status;
            Supported = status.HasFlag(CommandStatus.Supported);
            Enabled = status.HasFlag(CommandStatus.Enabled);
            Visible = !status.HasFlag(CommandStatus.Invisible);
        }

        protected override void Handle(object inArg, out object outArg) {
            outArg = null;
            _command.Invoke(inArg, ref outArg);
        }
    }
}
