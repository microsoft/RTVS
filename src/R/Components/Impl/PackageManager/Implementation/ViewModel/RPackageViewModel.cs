using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Common.Core;
using Microsoft.R.Components.PackageManager.Model;
using Microsoft.R.Components.PackageManager.ViewModel;

namespace Microsoft.R.Components.PackageManager.Implementation.ViewModel {
    internal class RPackageViewModel : IRPackageViewModel {
        public static RPackageViewModel CreateAvailable(RPackage package) {
            return new RPackageViewModel(package.Package) {
                LatestVersion = package.Version,
                Depends = package.Depends,
                Imports = package.Imports,
                Suggests = package.Suggests,
                License = package.License,
                NeedsCompilation = package.NeedsCompilation != null && !package.NeedsCompilation.EqualsIgnoreCase("no"),
                //Repository = package.Repository,
                //Description = package.Description,
            };
        }

        public static RPackageViewModel CreateInstalled(RPackage package) {
            return new RPackageViewModel(package.Package) {
                Title = package.Title,
                Authors = package.Author,
                License = package.License,
                Urls = package.URL?.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim())
                    .ToArray() ?? new string[0],
                NeedsCompilation = package.NeedsCompilation != null && !package.NeedsCompilation.EqualsIgnoreCase("no"),
                LibraryPath = package.LibPath,
                //Repository = package.Repository,
                //Description = package.Description,
                Built = package.Built,
                Depends = package.Depends,
                Imports = package.Imports,
                Suggests = package.Suggests,
                LatestVersion = package.Version,
                InstalledVersion = package.Version,
                IsInstalled = true
            };
        }

        public string Name { get; }
        public string Title { get; private set; }
        public string Description { get; }
        public string LatestVersion { get; private set; }
        public string InstalledVersion { get; private set; }
        public string Authors { get; private set; }
        public string License { get; private set; }
        public ICollection<string> Urls { get; private set; }
        public bool NeedsCompilation { get; private set; }
        public string LibraryPath { get; private set; }
        public string Repository { get; private set; }
        public string Built { get; set; }
        public string Depends { get; set; }
        public string Imports { get; set; }
        public string Suggests { get; set; }

        public bool IsInstalled { get; private set; }
        public bool IsUpdateAvailable { get; private set; }
        public bool IsSelected { get; set; }

        public RPackageViewModel(string name) {
            Name = name;
        }

        public void AddDataFromInstalledPackage(RPackage package) {
            Title = package.Title;
            Authors = package.Author;
            InstalledVersion = package.Version;
            Urls = package.URL?.Split(new[] {","}, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .ToArray() ?? new string[0];
            LibraryPath = package.LibPath;
            Built = package.Built;

            IsInstalled = true;
            IsUpdateAvailable = InstalledVersion != LatestVersion;
        }
    }
}