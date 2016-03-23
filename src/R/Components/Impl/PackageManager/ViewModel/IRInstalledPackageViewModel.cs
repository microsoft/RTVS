namespace Microsoft.R.Components.PackageManager.ViewModel {
    public interface IRInstalledPackageViewModel : IRPackageViewModel {
        string Description { get; }
        string[] Authors { get; }
        string Url { get; }
        string Repository { get; }
        string BuildRVersion { get; }
    }
}