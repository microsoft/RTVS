// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.Globalization;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.Telemetry;
using Microsoft.R.Core.Install;
using Microsoft.R.Host.Client.Telemetry;

namespace Microsoft.R.Host.Client.Install {
    /// <summary>
    /// Verifies that R is installed in the folder
    /// specified in settings. If nothing is specified
    /// settings try and find highest version.
    /// </summary>
    public static class RInstallationExtensions {
        public static bool VerifyRIsInstalled(this RInstallation rInstallation, ICoreShell coreShell, ISupportedRVersionRange svl, string path, bool showErrors = true) {
            svl = svl ?? new SupportedRVersionRange();
            var data = rInstallation.GetInstallationData(path, svl);
            if (data.Status == RInstallStatus.OK) {
                return true;
            }

            if (showErrors) {
                if (ShowMessage(coreShell, data, svl) == MessageButtons.Yes) {
                    coreShell.TelemetryService.ReportEvent(TelemetryArea.Configuration, MrcTelemetryEvents.RClientInstallPrompt);
                    var installer = coreShell.ExportProvider.GetExportedValue<IMicrosoftRClientInstaller>();
                    installer.LaunchRClientSetup(coreShell);
                }
            }

            return false;
        }

        private static MessageButtons ShowMessage(ICoreShell coreShell, RInstallData data, ISupportedRVersionRange svl) {
            Debug.Assert(data.Status != RInstallStatus.OK);

            switch (data.Status) {
                case RInstallStatus.UnsupportedVersion:
                    return coreShell.ShowMessage(
                        string.Format(CultureInfo.InvariantCulture, Resources.Error_UnsupportedRVersion,
                        data.Version.Major, data.Version.Minor, data.Version.Build,
                        svl.MinMajorVersion, svl.MinMinorVersion, "*",
                        svl.MaxMajorVersion, svl.MaxMinorVersion, "*",
                        Environment.NewLine + Environment.NewLine),
                        MessageButtons.YesNo);

                case RInstallStatus.ExceptionAccessingPath:
                    coreShell.ShowErrorMessage(
                        string.Format(CultureInfo.InvariantCulture, Resources.Error_ExceptionAccessingPath,
                        data.Path, data.Exception.Message));
                    return MessageButtons.OK;

                case RInstallStatus.NoRBinaries:
                    return coreShell.ShowMessage(
                        string.Format(CultureInfo.InvariantCulture, Resources.Error_CannotFindRBinariesFormat,
                            data.Path, Environment.NewLine + Environment.NewLine, Environment.NewLine),
                        MessageButtons.YesNo);

                case RInstallStatus.PathNotSpecified:
                    return coreShell.ShowMessage(string.Format(CultureInfo.InvariantCulture,
                        Resources.Error_UnableToFindR, Environment.NewLine + Environment.NewLine), MessageButtons.YesNo);
            }
            return MessageButtons.OK;
        }
    }
}
