using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Languages.Core.Formatting;
using Microsoft.R.Common.Wpf.Controls;
using Microsoft.R.Components.PackageManager.Model;
using Microsoft.R.Components.PackageManager.ViewModel;

namespace Microsoft.R.Components.PackageManager.Implementation.ViewModel {
    internal class RPackageViewModel : BindableBase, IRPackageViewModel {
        private readonly IRPackageManagerViewModel _owner;
        private bool _hasDetails;
        private bool _isChecked;
        private bool _isChanging;
        private bool _isInstalled;
        private bool _isLoaded;
        private string _title;
        private string _description;
        private string _authors;
        private string _installedVersion;
        private string _latestVersion;
        private ICollection<string> _urls;
        private string _libraryPath; 
        private Uri _repositoryUri;
        private string _repositoryText;
        private string _built;
        private bool _isUpdateAvailable;
        private bool _canBeUninstalled;
        private bool _isRemoteSession;

        public static RPackageViewModel CreateAvailable(RPackage package, IRPackageManagerViewModel owner) {
            Uri repositoryUri;
            Uri.TryCreate(package.Repository, UriKind.Absolute, out repositoryUri);

            return new RPackageViewModel(package.Package, owner) {
                LatestVersion = package.Version,
                Depends = package.Depends.NormalizeWhitespace(),
                Imports = package.Imports.NormalizeWhitespace(),
                Suggests = package.Suggests.NormalizeWhitespace(),
                License = package.License.NormalizeWhitespace(),
                NeedsCompilation = package.NeedsCompilation != null && !package.NeedsCompilation.EqualsIgnoreCase("no"),
                RepositoryUri = repositoryUri,
                RepositoryText = repositoryUri != null ? null : package.Repository,
                IsRemoteSession = owner.IsRemoteSession,
                Title = package.Title.NormalizeWhitespace(),
                Description = package.Description.NormalizeWhitespace(),
                Built = package.Built,
                Urls = package.URL?.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim())
                    .ToArray() ?? new string[0],
                Authors = package.Author.NormalizeWhitespace(),
                HasDetails = true,
            };
        }

        public static RPackageViewModel CreateInstalled(RPackage package, IRPackageManagerViewModel owner) {
            Uri repositoryUri;
            Uri.TryCreate(package.Repository, UriKind.Absolute, out repositoryUri);

            return new RPackageViewModel(package.Package, owner) {
                Title = package.Title.NormalizeWhitespace(),
                Authors = package.Author.NormalizeWhitespace(),
                License = package.License.NormalizeWhitespace(),
                Urls = package.URL?.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim())
                    .ToArray() ?? new string[0],
                NeedsCompilation = package.NeedsCompilation != null && !package.NeedsCompilation.EqualsIgnoreCase("no"),
                LibraryPath = package.LibPath,
                IsRemoteSession = owner.IsRemoteSession,
                RepositoryUri = repositoryUri,
                RepositoryText = repositoryUri != null ? null : package.Repository,
                Description = package.Description.NormalizeWhitespace(),
                Built = package.Built,
                Depends = package.Depends.NormalizeWhitespace(),
                Imports = package.Imports.NormalizeWhitespace(),
                Suggests = package.Suggests.NormalizeWhitespace(),
                LatestVersion = package.Version,
                InstalledVersion = package.Version,
                IsInstalled = true,
                HasDetails = true
            };
        }

        public string Name { get; }

        public string Title {
            get => _title;
            private set => SetProperty(ref _title, value);
        }

        public string Description {
            get => _description;
            private set => SetProperty(ref _description, value);
        }

        public string LatestVersion {
            get => _latestVersion;
            private set => SetProperty(ref _latestVersion, value);
        }

        public string InstalledVersion {
            get => _installedVersion;
            private set => SetProperty(ref _installedVersion, value);
        }

        public string Authors {
            get => _authors;
            private set => SetProperty(ref _authors, value);
        }

        public string License { get; private set; }

        public ICollection<string> Urls {
            get => _urls;
            private set => SetProperty(ref _urls, value);
        }

        public bool NeedsCompilation { get; private set; }

        public bool IsRemoteSession {
            get => _isRemoteSession;
            private set => SetProperty(ref _isRemoteSession, value);
        }
        public string LibraryPath {
            get => _libraryPath;
            private set => SetProperty(ref _libraryPath, value);
        }

        public string RepositoryText {
            get => _repositoryText;
            private set => SetProperty(ref _repositoryText, value);
        }

        public Uri RepositoryUri {
            get => _repositoryUri;
            private set => SetProperty(ref _repositoryUri, value);
        }

        public string Built {
            get => _built;
            private set => SetProperty(ref _built, value);
        }

        public string Depends { get; private set; }
        public string Imports { get; private set; }
        public string Suggests { get; private set; }

        public bool IsUpdateAvailable {
            get => _isUpdateAvailable;
            private set => SetProperty(ref _isUpdateAvailable, value);
        }

        public bool IsInstalled {
            get => _isInstalled;
            set => SetProperty(ref _isInstalled, value);
        }

        public bool IsLoaded {
            get => _isLoaded;
            set => SetProperty(ref _isLoaded, value);
        }

        public bool CanBeUninstalled {
            get => _canBeUninstalled;
            set => SetProperty(ref _canBeUninstalled, value);
        }

        public bool HasDetails {
            get => _hasDetails;
            private set => SetProperty(ref _hasDetails, value);
        }

        public bool IsChecked {
            get => _isChecked;
            set => SetProperty(ref _isChecked, value);
        }

        public bool IsChanging {
            get => _isChanging;
            set => SetProperty(ref _isChanging, value);
        }

        public RPackageViewModel(string name, IRPackageManagerViewModel owner) {
            _owner = owner;
            Name = name;
        }

        public void UpdateAvailablePackageDetails(RPackage package) {
            Uri repositoryUri;
            Uri.TryCreate(package.Repository, UriKind.Absolute, out repositoryUri);

            LatestVersion = package.Version;
            Depends = package.Depends;
            Imports = package.Imports;
            Suggests = package.Suggests;
            License = package.License;
            RepositoryUri = repositoryUri;
            RepositoryText = repositoryUri != null ? null : package.Repository;
            IsUpdateAvailable = new RPackageVersion(LatestVersion).CompareTo(new RPackageVersion(InstalledVersion)) > 0;
        }

        public Task InstallAsync() => _owner.InstallAsync(this);
        public Task UninstallAsync() => _owner.UninstallAsync(this);
        public Task UpdateAsync() => _owner.UpdateAsync(this);

        public void AddDetails(RPackage package, bool isInstalled) {
            Title = package.Title.NormalizeWhitespace();
            Description = package.Description.NormalizeWhitespace();
            Authors = package.Author.NormalizeWhitespace();
            Urls = package.URL?.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .ToArray() ?? new string[0];
            LibraryPath = package.LibPath;
            Built = package.Built;

            if (isInstalled) {
                InstalledVersion = package.Version;
                IsInstalled = true;
                IsUpdateAvailable = new RPackageVersion(LatestVersion).CompareTo(new RPackageVersion(InstalledVersion)) > 0;
            }

            HasDetails = true;
        }

        public string AccessibleDescription {
            get {
                string installed = IsInstalled ? Resources.Package_AccessibleState_Installed : Resources.Package_AccessibleState_NotInstalled;
                string loaded = string.Empty;
                if (IsInstalled) {
                    loaded = IsLoaded ? Resources.Package_AccessibleState_Loaded : Resources.Package_AccessibleState_NotLoaded;
                }
                return Resources.Package_AccessibleDescription.FormatInvariant(Name, installed, loaded, Description);
            }
        }
    }
}