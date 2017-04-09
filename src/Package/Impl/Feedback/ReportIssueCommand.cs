// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using Microsoft.Common.Core.Logging;
using Microsoft.Common.Core.OS;
using Microsoft.Common.Core.Services;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.Logging;
using Microsoft.VisualStudio.R.Packages.R;

namespace Microsoft.VisualStudio.R.Package.Feedback {
    internal class ReportIssueCommand : PackageCommand {
        private const string _url = @"https://go.microsoft.com/fwlink/?LinkID=760668&body={0}";
        private readonly ILoggingPermissions _permissions;
        private readonly IProcessServices _pss;

        public ReportIssueCommand(IServiceContainer services)
            : base(RGuidList.RCmdSetGuid, RPackageCommandId.icmdReportIssue) {
            _permissions = services.GetService<ILoggingPermissions>();
            _pss = services.GetService<IProcessServices>();
        }

        protected override void SetStatus() {
            Enabled = Visible = _permissions.IsFeedbackPermitted;
        }

        protected override void Handle() {
            var generalData = new StringWriter(CultureInfo.InvariantCulture);
            DiagnosticLogs.WriteGeneralData(generalData, detailed: false);

            var body = string.Format(CultureInfo.InvariantCulture, Resources.ReportIssueBody, generalData.ToString());

            var psi = new ProcessStartInfo {
                UseShellExecute = true,
                FileName = string.Format(CultureInfo.InvariantCulture, _url, Uri.EscapeDataString(body))
            };
            _pss.Start(psi);
        }
    }
}
