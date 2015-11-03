using System.IO;
using Microsoft.R.Actions.Utility;
using Microsoft.R.Support.Settings;

namespace Microsoft.R.Support.Help.Packages {
    public sealed class BasePackagesCollection : PackageCollection {
        public BasePackagesCollection() :
            base(GetInstallPath()) {
        }

        private static string GetInstallPath() {
            string rInstallPath = RInstallation.GetRInstallPath(RToolsSettings.Current != null ? RToolsSettings.Current.RBasePath : null);
            return Path.Combine(rInstallPath, "library");
        }
    }
}
