// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.R.Package.Commands;

namespace Microsoft.VisualStudio.R.Package.DataInspect.Commands {
    internal abstract class SessionCommand : PackageCommand {
        protected IRSession RSession { get; }

        public SessionCommand(IRSession session, Guid group, int id) :
            base(group, id) {
            if (session == null) {
                throw new ArgumentNullException(nameof(session));
            }
            RSession = session;
        }

        protected override void SetStatus() {
            Enabled = RSession.IsHostRunning;
        }

    }
}
