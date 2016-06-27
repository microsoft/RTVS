// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using Microsoft.R.Host.Client.Install;
using Microsoft.R.Support.Help.Definitions;
using Microsoft.R.Support.Settings;
using static System.FormattableString;

namespace Microsoft.R.Support.Help.Packages {
    [Export(typeof(IPackageCollection))]
    public sealed class UserPackagesCollection : PackageCollection {
        internal static string RLibraryPath { get; set; } = @"R\win-library";

        [ImportingConstructor]
        public UserPackagesCollection(IFunctionIndex functionIndex) :
            base(functionIndex, GetInstallPath()) {
        }

        internal static string GetInstallPath() {
            string libraryPath = string.Empty;

            try {
                string userDocumentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                libraryPath = Path.Combine(userDocumentsPath, RLibraryPath);

                if (Directory.Exists(libraryPath)) {
                    IEnumerable<string> directories = Directory.EnumerateDirectories(libraryPath);
                    if (directories.Count() == 1) {
                        // Most common case
                        libraryPath = Path.Combine(libraryPath, directories.First());
                    } else {
                        // Multiple library folders. Try and match to R version
                        // specified in the Tools | Options
                        var version = new RInstallation().GetRVersion(RToolsSettings.Current.RBasePath);
                        var versionString = Invariant($"{version.Major}.{version.Minor}");
                        string path = Path.Combine(libraryPath, versionString);
                        if (Directory.Exists(path)) {
                            libraryPath = path;
                        }
                    }
                }
            } catch (IOException) { }

            return libraryPath;
        }
   }
}