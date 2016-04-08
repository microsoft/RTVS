using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Common.Core;
using Microsoft.Common.Wpf;
using Microsoft.Languages.Core.Formatting;
using Microsoft.R.Components.PackageManager.Model;
using Microsoft.R.Components.PackageManager.ViewModel;

namespace Microsoft.R.Components.PackageManager.Implementation.ViewModel {
    internal class RPackageViewModel : BindableBase, IRPackageViewModel {
        private readonly IRPackageManagerViewModel _owner;
        private bool _hasDetails;
        private bool _isSelected;
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
        private string _built;
        private bool _isUpdateAvailable;
        private bool _canUninstall;

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
            get { return _title; }
            private set { SetProperty(ref _title, value); }
        }

        public string Description {
            get { return _description; }
            private set { SetProperty(ref _description, value); }
        }

        public string LatestVersion {
            get { return _latestVersion; }
            private set { SetProperty(ref _latestVersion, value); }
        }

        public string InstalledVersion {
            get { return _installedVersion; }
            private set { SetProperty(ref _installedVersion, value); }
        }

        public string Authors {
            get { return _authors; }
            private set { SetProperty(ref _authors, value); }
        }

        public string License { get; private set; }

        public ICollection<string> Urls {
            get { return _urls; }
            private set { SetProperty(ref _urls, value); }
        }

        public bool NeedsCompilation { get; private set; }

        public string LibraryPath {
            get { return _libraryPath; }
            private set { SetProperty(ref _libraryPath, value); }
        }

        public string RepositoryText { get; private set; }
        public Uri RepositoryUri { get; private set; }

        public string Built {
            get { return _built; }
            private set { SetProperty(ref _built, value); }
        }

        public string Depends { get; private set; }
        public string Imports { get; private set; }
        public string Suggests { get; private set; }

        public bool IsUpdateAvailable {
            get { return _isUpdateAvailable; }
            private set { SetProperty(ref _isUpdateAvailable, value); }
        }

        public bool IsInstalled {
            get { return _isInstalled; }
            set { SetProperty(ref _isInstalled, value); }
        }

        public bool IsLoaded {
            get { return _isLoaded; }
            set { SetProperty(ref _isLoaded, value); }
        }

        public bool CanUninstall {
            get { return _isInstalled && _canUninstall; }
            set { SetProperty(ref _canUninstall, value); }
        }

        public bool HasDetails {
            get { return _hasDetails; }
            private set { SetProperty(ref _hasDetails, value); }
        }

        public bool IsSelected {
            get { return _isSelected; }
            set { SetProperty(ref _isSelected, value); }
        }

        public bool IsChanging {
            get { return _isChanging; }
            set { SetProperty(ref _isChanging, value); }
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

        public void Install() {
            _owner.Install(this);
        }

        public void Uninstall() {
            _owner.Uninstall(this);
        }

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
    }
}