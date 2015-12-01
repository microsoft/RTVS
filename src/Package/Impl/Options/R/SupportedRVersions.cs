using System.Diagnostics;
using System.Globalization;
using Microsoft.Languages.Editor.Shell;
using Microsoft.R.Actions.Utility;
using Microsoft.R.Support.Settings;

namespace Microsoft.VisualStudio.R.Package.Options.R {
    internal static class SupportedRVersions {
        // TODO: this probably needs configuration file
        // or another dynamic source of supported versions.
        private const int _minMajorVersion = 3;
        private const int _minMinorVersion = 2;
        private const int _maxMajorVersion = 3;
        private const int _maxMinorVersion = 2;

        public static bool VerifyRIsInstalled(string path, bool showErrors) {

            RInstallData data = RInstallation.GetInstallationData(path,
                        _minMajorVersion, _minMinorVersion, _maxMajorVersion, _maxMinorVersion);

            if (data.Status != RInstallStatus.OK && showErrors) {
                string message = FormatMessage(data);
                EditorShell.Current.ShowErrorMessage(message);
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
                        _minMajorVersion, _minMinorVersion, "*",
                        _maxMajorVersion, _maxMinorVersion, "*");

                case RInstallStatus.ExceptionAccessingPath:
                    return string.Format(CultureInfo.InvariantCulture, Resources.Error_ExceptionAccessingPath, data.Path, data.Exception.Message);

                case RInstallStatus.NoRBinaries:
                    Debug.Assert(!string.IsNullOrEmpty(data.Path));
                    return string.Format(CultureInfo.InvariantCulture, Resources.Error_CannotFindRBinariesFormat, data.Path);

                case RInstallStatus.Undefined:
                    return string.Format(CultureInfo.InvariantCulture, Resources.Error_UnableToFindR);
            }

            return string.Empty;
        }
    }
}
