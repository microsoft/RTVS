// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Globalization;
using System.IO;
using Microsoft.Common.Core.Shell;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.Logging;
using Microsoft.VisualStudio.R.Packages.R;

namespace Microsoft.VisualStudio.R.Package.Feedback {
    internal sealed class SendFrownCommand : SendMailCommand {
        public SendFrownCommand(ICoreShell coreShell) :
            base(RGuidList.RCmdSetGuid, RPackageCommandId.icmdSendFrown, coreShell) {
        }

        protected override void Handle() {
            string zipPath = DiagnosticLogs.Collect(Shell.GetLog());

            var generalData = new StringWriter(CultureInfo.InvariantCulture);
            DiagnosticLogs.WriteGeneralData(generalData, detailed: false);

            SendMail(string.Format(CultureInfo.InvariantCulture, Resources.SendFrownEmailBody, zipPath, generalData.ToString()), "RTVS Frown", zipPath);
        }
    }
}
