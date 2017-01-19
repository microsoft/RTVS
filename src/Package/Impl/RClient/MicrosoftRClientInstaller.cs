// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Globalization;
using System.IO;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Logging;
using Microsoft.Common.Core.Net;
using Microsoft.Common.Core.OS;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.Telemetry;
using Microsoft.R.Interpreters;
using Microsoft.VisualStudio.R.Package.Telemetry;
using Microsoft.VisualStudio.R.Package.Utilities;

namespace Microsoft.VisualStudio.R.Package.RClient {
    [Export(typeof(IMicrosoftRClientInstaller))]
    internal sealed class MicrosoftRClientInstaller : IMicrosoftRClientInstaller {
        public void LaunchRClientSetup(ICoreShell coreShell, IFileDownloader downloader = null) {
            coreShell.Services.Telemetry.ReportEvent(TelemetryArea.Configuration, RtvsTelemetry.ConfigurationEvents.RClientInstallYes);
            downloader = downloader ?? new FileDownloader();

            string downloadError = null;
            var rClientExe = Path.Combine(Path.GetTempPath(), "RClientSetup.exe");

            LongOperationNotification.ShowWaitingPopup(Resources.DownloadingRClientInstaller, new LongAction[] {
                new LongAction() {
                    Name = Resources.DownloadingRClientInstaller,
                    Action = (o, ct) => {
                        downloadError = downloader.Download("http://go.microsoft.com/fwlink/?LinkId=800048", rClientExe, ct);
                    },
                }, 
            }, coreShell.Services.Log);

            if (!string.IsNullOrEmpty(downloadError)) {
                var errorMessage = string.Format(CultureInfo.InvariantCulture, Resources.Error_UnableToDownloadRClient, downloadError);
                coreShell.ShowErrorMessage(errorMessage);
                coreShell.Services.Telemetry.ReportEvent(TelemetryArea.Configuration, RtvsTelemetry.ConfigurationEvents.RClientDownloadFailed, errorMessage);
                coreShell.Services.Log.Write(LogVerbosity.Minimal, MessageCategory.Error, "Microsoft R Client download error: " + errorMessage);
            } else {
                // Suppress 'Operation canceled by the user' if user clicks 'No' to elevation dialog.
                try {
                    coreShell.ShowMessage(Resources.PleaseRestartVisualStudioAfterRClientSetup, MessageButtons.OK);
                    coreShell.Services.ProcessServices.Start(rClientExe);
                } catch (Win32Exception ex) {
                    if((uint)ex.NativeErrorCode == 0x800704C7) {
                        coreShell.Services.Telemetry.ReportEvent(TelemetryArea.Configuration, RtvsTelemetry.ConfigurationEvents.RClientInstallCancel);
                    }
                }
            }
        }
    }
}
