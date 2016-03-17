// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.IO;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.Logging;
using Microsoft.VisualStudio.R.Packages.R;

namespace Microsoft.VisualStudio.R.Package.Feedback {
    internal sealed class SendFrownCommand : SendMailCommand {
        public SendFrownCommand() :
            base(RGuidList.RCmdSetGuid, RPackageCommandId.icmdSendFrown) {
        }

        protected override void SetStatus() {
            Enabled = true;
        }

        protected override void Handle() {
            string zipPath = DiagnosticLogs.Collect();

            var generalData = new StringWriter();
            DiagnosticLogs.WriteGeneralData(generalData, detailed: false);

            SendMail(string.Format(Resources.SendFrownEmailBody, zipPath, generalData.ToString(), zipPath), "RTVS Frown", zipPath);
        }
    }
}
