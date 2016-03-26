// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using Microsoft.R.Components.PackageManager.ViewModel;

namespace Microsoft.R.Components.PackageManager.Implementation.View.DesignTime {
    internal class DesignTimeRPackageViewModel : IRPackageViewModel {
        public DesignTimeRPackageViewModel() {
            Name = "ggplot2";
            Title = "An Implementation of the Grammar of Graphics";
            Description = "An implementation of the grammar of graphics in R. It combines the advantages of both base and lattice graphics: conditioning and shared axes  are handled automatically, and you can still build up a plot step by step from multiple data sources. It also implements a sophisticated multidimensional conditioning system and a consistent interface to map data to aesthetic attributes. See http://ggplot2.org for more information, documentation and examples.";
            LatestVersion = "2.0.4";
            InstalledVersion = "2.0.0";
            Depends = "R (>= 2.14)";
            Imports = "digest, grid, gtable (>= 0.1.1), MASS, plyr (>= 1.7.1), reshape2, scales(>= 0.3.0), stats";
            Suggests = "ggplot2movies, hexbin, Hmisc, mapproj, maps, maptools, mgcv, multcomp, nlme, testthat, quantreg, knitr";
            License = "GPL (>= 2)";
            Urls = new [] { "http://ggplot2.org", "https://github.com/hadley/ggplot2" };
            NeedsCompilation = false;
            Authors = "Hadley Wickham [aut, cre], Winston Chang[aut], RStudio[cph]";
            LibraryPath = "~/LibPath"; 
            Repository = "CRAN";
            Built = "R 3.3.0; ; 2016-02-16 11:24:44 UTC; windows";

            IsInstalled = true;
            IsUpdateAvailable = true;
            IsSelected = true;
        }

        public DesignTimeRPackageViewModel(string name
            , string latestVersion = null
            , string installedVersion = null
            , string depends = null
            , string license = null
            , bool isInstalled = false
            , bool isUpdateAvailable = false
            , bool isSelected = false) {

            Name = name;
            LatestVersion = latestVersion;
            InstalledVersion = installedVersion;
            Depends = depends;
            License = license;
            IsInstalled = isInstalled;
            IsUpdateAvailable = isUpdateAvailable;
            IsSelected = isSelected;
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
        public string Repository { get; }
        public string Built { get; }
        public string Depends { get; }
        public string Imports { get; }
        public string Suggests { get; }
        public bool IsInstalled { get; }

        public bool IsUpdateAvailable { get; }
        public bool IsSelected { get; set; }
    }
}
