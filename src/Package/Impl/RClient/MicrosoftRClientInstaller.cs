// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel;
using System.Globalization;
using System.IO;
using Microsoft.Common.Core.Logging;
using Microsoft.Common.Core.Net;
using Microsoft.Common.Core.Network;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Telemetry;
using Microsoft.Common.Core.UI;
using Microsoft.R.Interpreters;
using Microsoft.VisualStudio.R.Package.Telemetry;
using Microsoft.VisualStudio.R.Package.Utilities;

namespace Microsoft.VisualStudio.R.Package.RClient {
    internal sealed class MicrosoftRClientInstaller : IMicrosoftRClientInstaller {
        public void LaunchRClientSetup(IServiceContainer services, IFileDownloader downloader = null) {
            services.Telemetry().ReportEvent(TelemetryArea.Configuration, RtvsTelemetry.ConfigurationEvents.RClientInstallYes);
            downloader = downloader ?? new FileDownloader();

            string downloadError = null;
            var rClientExe = Path.Combine(Path.GetTempPath(), "RClientSetup.exe");

            LongOperationNotification.ShowWaitingPopup(Resources.DownloadingRClientInstaller, new LongAction[] {
                new LongAction() {
                    Name = Resources.DownloadingRClientInstaller,
                    Action = (o, ct) => {
                        downloadError = downloader.Download("https://aka.ms/rclient/download", rClientExe, ct);
                    },
                }, 
            }, services.Log());

            if (!string.IsNullOrEmpty(downloadError)) {
                var errorMessage = string.Format(CultureInfo.InvariantCulture, Resources.Error_UnableToDownloadRClient, downloadError);
                services.UI().ShowErrorMessage(errorMessage);
                services.Telemetry().ReportEvent(TelemetryArea.Configuration, RtvsTelemetry.ConfigurationEvents.RClientDownloadFailed, errorMessage);
                services.Log().Write(LogVerbosity.Minimal, MessageCategory.Error, "Microsoft R Client download error: " + errorMessage);
            } else {
                // Suppress 'Operation canceled by the user' if user clicks 'No' to elevation dialog.
                try {
                    services.UI().ShowMessage(Resources.PleaseRestartVisualStudioAfterRClientSetup, MessageButtons.OK);
                    services.Process().Start(rClientExe);
                } catch (Win32Exception ex) {
                    if((uint)ex.NativeErrorCode == 0x800704C7) {
                        services.Telemetry().ReportEvent(TelemetryArea.Configuration, RtvsTelemetry.ConfigurationEvents.RClientInstallCancel);
                    }
                }
            }
        }
    }
}
