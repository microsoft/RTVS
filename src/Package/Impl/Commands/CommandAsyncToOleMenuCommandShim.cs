// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Diagnostics;
using Microsoft.Common.Core.UI.Commands;

namespace Microsoft.VisualStudio.R.Package.Commands {
    internal class CommandAsyncToOleMenuCommandShim : PackageCommand {
        private readonly IAsyncCommand _command;

        public CommandAsyncToOleMenuCommandShim(Guid group, int id, IAsyncCommand command)
            : base(group, id) {
            Check.ArgumentNull(nameof(command), command);
            _command = command;
        }

        protected override void SetStatus() {
            var status = _command.Status;
            Supported = status.HasFlag(CommandStatus.Supported);
            Enabled = status.HasFlag(CommandStatus.Enabled);
            Visible = !status.HasFlag(CommandStatus.Invisible);
            Checked = status.HasFlag(CommandStatus.Latched);
        }

        protected override void Handle(object inArg, out object outArg) {
            outArg = null;
            _command.InvokeAsync().DoNotWait();
        }
    }
}
