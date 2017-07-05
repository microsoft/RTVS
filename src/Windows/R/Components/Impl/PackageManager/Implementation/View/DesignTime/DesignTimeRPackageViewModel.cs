// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.R.Components.PackageManager.Model;
using Microsoft.R.Components.PackageManager.ViewModel;

namespace Microsoft.R.Components.PackageManager.Implementation.View.DesignTime {
#if DEBUG
    internal class DesignTimeRPackageViewModel : IRPackageViewModel {
        public DesignTimeRPackageViewModel() {
            Name = "rtvs";
            Title = "An Implementation of the RTVS package";
            Description = "";
            LatestVersion = "2.0.4";
            InstalledVersion = "2.0.0";
            Depends = "R (>= 3.2.2)";
            Imports = "digest, grid, gtable (>= 0.1.1), MASS, plyr (>= 1.7.1), reshape2, scales(>= 0.3.0), stats";
            Suggests = "rtvs1, rtvs2, rtvs3";
            License = "GPL (>= 2)";
            Urls = new [] { "https://github.com/Microsoft/RTVS", "https://microsoft.github.io/RTVS-docs/" };
            NeedsCompilation = false;
            Authors = "Microsoft Corporation";
            LibraryPath = "~/LibPath"; 
            RepositoryUri = new Uri("https://cran.rstudio.com");
            RepositoryText = "https://cran.rstudio.com";
            Built = "R 3.3.0; ; 2016-02-16 11:24:44 UTC; windows";
            AccessibleDescription = "Accessible description";

            IsInstalled = true;
            IsUpdateAvailable = true;
            IsChecked = true;
            HasDetails = true;
            CanBeUninstalled = true;
        }

        public DesignTimeRPackageViewModel(string name
            , string latestVersion = null
            , string installedVersion = null
            , string depends = null
            , string license = null
            , bool isInstalled = false
            , bool isUpdateAvailable = false
            , bool isChecked = false) {

            Name = name;
            LatestVersion = latestVersion;
            InstalledVersion = installedVersion;
            Depends = depends;
            License = license;
            IsInstalled = isInstalled;
            IsUpdateAvailable = isUpdateAvailable;
            IsChecked = isChecked;
        }

        public string Name { get; }
        public string Title { get; }
        public string Description { get; }
        public string LatestVersion { get; }
        public string InstalledVersion { get; }
        public string Authors { get; set; }
        public string License { get; }
        public ICollection<string> Urls { get; }
        public bool NeedsCompilation { get; }
        public string LibraryPath { get; }
        public string RepositoryText { get; }
        public Uri RepositoryUri { get; }
        public string Built { get; }
        public string Depends { get; }
        public string Imports { get; }
        public string Suggests { get; }
        public bool IsInstalled { get; set; }
        public bool IsLoaded { get; set; }
        public bool CanBeUninstalled { get; set; }
        public bool IsChanging { get; set; }
        public bool IsRemoteSession { get; set; }
        public bool IsUpdateAvailable { get; }
        public bool HasDetails { get; }
        public bool IsChecked { get; set; }
        public void AddDetails(RPackage package, bool isInstalled) {}
        public void UpdateAvailablePackageDetails(RPackage package) {}
        public Task InstallAsync() => Task.CompletedTask;
        public Task UninstallAsync() => Task.CompletedTask;
        public Task UpdateAsync() => Task.CompletedTask;
        public string AccessibleDescription { get; }
    }
#endif
}
