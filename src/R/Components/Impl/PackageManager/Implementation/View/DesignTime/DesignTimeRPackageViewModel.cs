using Microsoft.R.Components.PackageManager.Model;

namespace Microsoft.R.Components.PackageManager.Implementation.View.DesignTime {
    internal class DesignTimeRPackageViewModel : IRPackageViewModel {
        public DesignTimeRPackageViewModel(string name = null
            , string latestVersion = null
            , string installedVersion = null
            , string dependencies = null
            , string license = null
            , bool isInstalled = false
            , bool isUpdateAvailable = false
            , bool isSelected = false) {

            Name = name;
            LatestVersion = latestVersion != null ? new RPackageVersion(latestVersion) : null;
            InstalledVersion = installedVersion != null ? new RPackageVersion(installedVersion) : null;
            Dependencies = dependencies;
            License = license;
            IsInstalled = isInstalled;
            IsUpdateAvailable = isUpdateAvailable;
            IsSelected = isSelected;
        }

        public string Name { get; }
        public string Title { get; }
        public RPackageVersion LatestVersion { get; }
        public RPackageVersion InstalledVersion { get; }
        public string Dependencies { get; }
        public string License { get; }
        public bool IsInstalled { get; }
        public bool IsUpdateAvailable { get; }
        public bool IsSelected { get; set; }
    }
}
