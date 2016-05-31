// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.Globalization;
using Microsoft.Common.Core.Shell;

namespace Microsoft.Common.Core.Install {
    public static class RInstallationHelper {
        public static bool VerifyRIsInstalled(ICoreShell coreShell, string path, bool showErrors = true) {
            var data = RInstallation.GetInstallationData(path,
                SupportedRVersionList.MinMajorVersion, SupportedRVersionList.MinMinorVersion,
                SupportedRVersionList.MaxMajorVersion, SupportedRVersionList.MaxMinorVersion,
                coreShell);

            if (data.Status == RInstallStatus.OK) {
                return true;
            }

            if (showErrors) {
                if (ShowMessage(coreShell, data) == MessageButtons.Yes) {
                    var exception = RInstallation.LaunchRClientSetup();
                    if(exception != null) {
                        coreShell.ShowErrorMessage(string.Format(CultureInfo.InvariantCulture,
                            Resources.Error_UnableToDownloadRClient, exception.Message));
                    } else {

                    }
                }
            }

            return false;
        }

        private static MessageButtons ShowMessage(ICoreShell coreShell, RInstallData data) {
            Debug.Assert(data.Status != RInstallStatus.OK);

            switch (data.Status) {
                case RInstallStatus.UnsupportedVersion:
                    return coreShell.ShowMessage(
                        string.Format(CultureInfo.InvariantCulture, Resources.Error_UnsupportedRVersion,
                        data.Version.Major, data.Version.Minor, data.Version.Build,
                        SupportedRVersionList.MinMajorVersion, SupportedRVersionList.MinMinorVersion, "*",
                        SupportedRVersionList.MaxMajorVersion, SupportedRVersionList.MaxMinorVersion, "*",
                        Environment.NewLine + Environment.NewLine),
                        MessageButtons.YesNo);

                case RInstallStatus.ExceptionAccessingPath:
                    coreShell.ShowErrorMessage(
                        string.Format(CultureInfo.InvariantCulture, Resources.Error_ExceptionAccessingPath, 
                        data.Path, data.Exception.Message));
                    return MessageButtons.OK;

                case RInstallStatus.NoRBinaries:
                    coreShell.ShowErrorMessage(
                        string.Format(CultureInfo.InvariantCulture, Resources.Error_CannotFindRBinariesFormat, 
                        data.Path));
                    return MessageButtons.OK;

                case RInstallStatus.PathNotSpecified:
                    return coreShell.ShowMessage(string.Format(CultureInfo.InvariantCulture, 
                        Resources.Error_UnableToFindR, Environment.NewLine + Environment.NewLine), MessageButtons.YesNo);
            }
            return MessageButtons.OK;
        }
    }
}