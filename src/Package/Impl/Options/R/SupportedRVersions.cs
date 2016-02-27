using System;
using System.Diagnostics;
using System.Globalization;
using Microsoft.R.Actions.Utility;
using Microsoft.R.Support.Settings;
using Microsoft.VisualStudio.R.Package.Shell;

namespace Microsoft.VisualStudio.R.Package.Options.R {
    internal static class SupportedRVersions {
        // TODO: this probably needs configuration file
        // or another dynamic source of supported versions.

        public static bool VerifyRIsInstalled(string path, bool showErrors) {

            RInstallData data = RInstallation.GetInstallationData(path,
                        SupportedRVersionList.MinMajorVersion, SupportedRVersionList.MinMinorVersion,
                        SupportedRVersionList.MaxMajorVersion, SupportedRVersionList.MaxMinorVersion);

            if (data.Status != RInstallStatus.OK) {
                if (showErrors) {
                    string message = FormatMessage(data);
                    VsAppShell.Current.ShowErrorMessage(message);
                    if (data.Status == RInstallStatus.PathNotSpecified || data.Status == RInstallStatus.UnsupportedVersion) {
                        if (!string.IsNullOrEmpty(path)) {
                            // If path is in Tools | Options and yet we can't find proper R
                            // we need to clear this setting.
                            RToolsSettings.Current.RBasePath = null;
                        }
                        RInstallation.GoToRInstallPage();
                    }
                }
                return false;
            }
            return true;
        }

        public static bool VerifyRIsInstalled(bool showErrors = true) {
            return VerifyRIsInstalled(RToolsSettings.Current.RBasePath, showErrors);
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
                    return string.Format(CultureInfo.InvariantCulture, Resources.Error_UnableToFindR, Environment.NewLine + Environment.NewLine);
            }

            return string.Empty;
        }
    }
}
