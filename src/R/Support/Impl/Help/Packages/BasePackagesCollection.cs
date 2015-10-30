using System.IO;
using Microsoft.R.Support.Settings;
using Microsoft.R.Support.Utility;

namespace Microsoft.R.Support.Help.Packages {
    public sealed class BasePackagesCollection : PackageCollection {
        public BasePackagesCollection() :
            base(GetInstallPath()) {
        }

        private static string GetInstallPath() {
            string rInstallPath = RInstallation.GetRInstallPath();
            return Path.Combine(rInstallPath, "library");
        }
    }
}
