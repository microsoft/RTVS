// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.R.Packages.R;

namespace Microsoft.VisualStudio.R.Package.Commands {
    internal abstract class SessionCommand : PackageCommand {
        protected IRSession RSession { get; }

        public SessionCommand(int id, IRSession session) :
            base(RGuidList.RCmdSetGuid, id) {
            RSession = session;
        }

        protected override void SetStatus() {
            Enabled = RSession.IsHostRunning;
        }
    }
}
