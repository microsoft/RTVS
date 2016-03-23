using Microsoft.R.Components.PackageManager.Model;

namespace Microsoft.R.Components.PackageManager {
    public interface IRPackageViewModel {
        string Name { get; }
        string Title { get; }
        RPackageVersion LatestVersion { get; }
        RPackageVersion InstalledVersion { get; }
        string Dependencies { get; }
        string License { get; }

        bool IsInstalled { get; }
        bool IsUpdateAvailable { get; }
        bool IsSelected { get; set; }
    }
}
