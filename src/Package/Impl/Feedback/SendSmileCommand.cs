// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Packages.R;

namespace Microsoft.VisualStudio.R.Package.Feedback {
    internal sealed class SendSmileCommand : SendMailCommand {
        public SendSmileCommand() :
            base(RGuidList.RCmdSetGuid, RPackageCommandId.icmdSendSmile) {
        }

        internal override void SetStatus() {
            Enabled = true;
        }

        internal override void Handle() {
            SendMail(":-)", "RTVS Smile", null);
        }
    }
}
