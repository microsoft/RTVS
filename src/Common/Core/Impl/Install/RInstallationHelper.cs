// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.Globalization;
using Microsoft.Common.Core.Shell;

namespace Microsoft.Common.Core.Install {
    public static class RInstallationHelper {
        public static bool VerifyRIsInstalled(ICoreShell coreShell, ISupportedRVersionList svl, string path, bool showErrors = true) {
            svl = svl ?? new SupportedRVersionList();
            var data = RInstallation.GetInstallationData(path, svl, coreShell);
            if (data.Status == RInstallStatus.OK) {
                return true;
            }

            if (showErrors) {
                if (ShowMessage(coreShell, data, svl) == MessageButtons.Yes) {
                    var exception = RInstallation.LaunchRClientSetup();
                    if(exception != null) {
                        coreShell.ShowErrorMessage(string.Format(CultureInfo.InvariantCulture,
                            Resources.Error_UnableToDownloadRClient, exception.Message));
                    }
                }
            }

            return false;
        }

        private static MessageButtons ShowMessage(ICoreShell coreShell, RInstallData data, ISupportedRVersionList svl) {
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