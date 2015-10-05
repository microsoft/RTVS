using System;
using Microsoft.R.Actions.Logging;

namespace Microsoft.VisualStudio.R.Package.Logging
{
    public sealed class PackagesInstallLog : OutputWindowLog
    {
        private static Guid _windowPaneGuid = new Guid("8DF051DA-60DE-4E2B-AB90-E440EE089A7F");
        private static Lazy<PackagesInstallLog> _instance = new Lazy<PackagesInstallLog>(() => new PackagesInstallLog());

        public static IActionLog Current
        {
            get { return _instance.Value; }
        }

        private PackagesInstallLog() :
            base(_windowPaneGuid, Resources.OutputWindowName_InstallPackages)
        {
        }
    }
}
