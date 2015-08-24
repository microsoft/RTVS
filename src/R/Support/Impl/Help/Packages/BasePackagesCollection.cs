using System.IO;
using Microsoft.R.Support.Settings;

namespace Microsoft.R.Support.Help.Packages
{
    public sealed class BasePackagesCollection : PackageCollection
    {
        public BasePackagesCollection() : 
            base(GetInstallPath())
        {
        }

        private static string GetInstallPath()
        {
            string rVersionPath = RToolsSettings.GetRVersionPath();
            return Path.Combine(rVersionPath, "library");
        }
    }
}
