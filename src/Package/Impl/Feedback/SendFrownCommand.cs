// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Globalization;
using System.IO;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Services;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.Logging;
using Microsoft.VisualStudio.R.Packages.R;

namespace Microsoft.VisualStudio.R.Package.Feedback {
    internal sealed class SendFrownCommand : SendMailCommand {
        public SendFrownCommand(IServiceContainer services) :
            base(RGuidList.RCmdSetGuid, RPackageCommandId.icmdSendFrown, services) {
        }

        protected override void Handle() {
            string zipPath = DiagnosticLogs.Collect(Services.Log());

            var generalData = new StringWriter(CultureInfo.InvariantCulture);
            DiagnosticLogs.WriteGeneralData(generalData, detailed: false);

            SendMail(Resources.SendFrownEmailBody.FormatInvariant(zipPath, generalData.ToString()), "RTVS Frown", zipPath);
        }
    }
}
