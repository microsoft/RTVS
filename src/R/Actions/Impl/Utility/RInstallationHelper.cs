using System;
using System.Diagnostics;
using System.Globalization;
using Microsoft.Common.Core.Shell;

namespace Microsoft.R.Actions.Utility {
    public static class RInstallationHelper {
        public static bool VerifyRIsInstalled(ICoreShell coreShell, string path, bool showErrors = true) {
            var data = RInstallation.GetInstallationData(path,
                SupportedRVersionList.MinMajorVersion, SupportedRVersionList.MinMinorVersion,
                SupportedRVersionList.MaxMajorVersion, SupportedRVersionList.MaxMinorVersion);

            if (data.Status == RInstallStatus.OK) {
                return true;
            }

            if (showErrors) {
                string message = FormatMessage(data);
                coreShell.ShowErrorMessage(message);
                if (data.Status == RInstallStatus.PathNotSpecified || data.Status == RInstallStatus.UnsupportedVersion) {
                    RInstallation.GoToRInstallPage();
                }
            }

            return false;
        }

        private static string FormatMessage(RInstallData data) {
            Debug.Assert(data.Status != RInstallStatus.OK);

            switch (data.Status) {
                case RInstallStatus.UnsupportedVersion:
                    return string.Format(CultureInfo.InvariantCulture, Resources.Error_UnsupportedRVersion, 
                        data.Version.Major, data.Version.Minor, data.Version.Build,
                        SupportedRVersionList.MinMajorVersion, SupportedRVersionList.MinMinorVersion, "*",
                        SupportedRVersionList.MaxMajorVersion, SupportedRVersionList.MaxMinorVersion, "*");

                case RInstallStatus.ExceptionAccessingPath:
                    return string.Format(CultureInfo.InvariantCulture, Resources.Error_ExceptionAccessingPath, data.Path, data.Exception.Message);

                case RInstallStatus.NoRBinaries:
                    Debug.Assert(!string.IsNullOrEmpty(data.Path));
                    return string.Format(CultureInfo.InvariantCulture, Resources.Error_CannotFindRBinariesFormat, data.Path);

                case RInstallStatus.PathNotSpecified:
                    return string.Format(CultureInfo.InvariantCulture, Resources.Error_UnableToFindR);
            }

            return String.Empty;
        }
    }
}