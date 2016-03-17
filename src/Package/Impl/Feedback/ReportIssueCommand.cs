// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.Logging;
using Microsoft.VisualStudio.R.Packages.R;

namespace Microsoft.VisualStudio.R.Package.Documentation {
    internal class ReportIssueCommand : PackageCommand {
        private const string _url = @"http://go.microsoft.com/fwlink/?LinkID=760668&body={0}";

        public ReportIssueCommand()
            : base(RGuidList.RCmdSetGuid, RPackageCommandId.icmdReportIssue) {
        }

        protected override void SetStatus() {
            Enabled = true;
        }

        protected override void Handle() {
            var generalData = new StringWriter();
            DiagnosticLogs.WriteGeneralData(generalData, detailed: false);

            var body = string.Format(CultureInfo.InvariantCulture, Resources.ReportIssueBody, generalData.ToString());

            var psi = new ProcessStartInfo {
                UseShellExecute = true,
                FileName = string.Format(CultureInfo.InvariantCulture, _url, Uri.EscapeDataString(body))
            };
            Process.Start(psi);
        }
    }
}
