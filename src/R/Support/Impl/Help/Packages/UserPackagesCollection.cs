using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using Microsoft.R.Support.Help.Definitions;
using Microsoft.R.Support.Settings;

namespace Microsoft.R.Support.Help.Packages {
    [Export(typeof(IPackageCollection))]
    public sealed class UserPackagesCollection : PackageCollection {
        internal static string RLibraryPath { get; set; } = @"R\win-library";

        public UserPackagesCollection() :
            base(GetInstallPath()) {
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
                    }
                    else
                    {
                        // Multiple library folders. Try and match to R version
                        // specified in the Tools | Options
                        string version = GetReducedVersion();
                        string path = Path.Combine(libraryPath, version);
                        if (Directory.Exists(path)) {
                            libraryPath = path;
                        }
                    }
                }
            } catch (IOException) { }

            return libraryPath;
        }

        /// <summary>
        /// Based on a typical R binaries install path pattern
        /// such as 'C:\Program Files\R\R-3.2.2' constructs path
        /// to the user package library folder which looks like
        /// C:\Users\[USER_NAME]\Documents\R\win-library\3.2
        /// </summary>
        /// <returns></returns>
        internal static string GetReducedVersion()
        {
            string rBasePath = RToolsSettings.Current.RBasePath;
            string version = string.Empty;

            // TODO: this probably possible to get from R.Host instead
            int index = rBasePath.IndexOf("R-", StringComparison.Ordinal);
            if (index >= 0)
            {
                version = version.Substring(index + 2);
                int nextSlashIndex = version.IndexOf('\\');
                if(nextSlashIndex > 0) {
                    version = version.Substring(0, nextSlashIndex);
                }

                // Now version should look like a.b.c where a, b and c
                // are 1 or 2 digit numbers. ~\Documents\R\win-library\3.2
                // ignores .c part.
                string[] parts = version.Split(new char[] { '.' });
                if(parts.Length >= 2) {
                    version = parts[0] + '.' + parts[1];
                }
            }

            return version;
        }
    }
}
