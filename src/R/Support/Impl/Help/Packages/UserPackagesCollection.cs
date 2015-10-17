using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using Microsoft.R.Support.Help.Definitions;
using Microsoft.R.Support.Settings;

namespace Microsoft.R.Support.Help.Packages
{
    [Export(typeof(IPackageCollection))]
    public sealed class UserPackagesCollection : PackageCollection
    {
        internal static string RLibraryPath { get; set; } = @"R\win-library";

        public UserPackagesCollection() :
            base(GetInstallPath())
        {
        }

        internal static string GetInstallPath()
        {
            string libraryPath = string.Empty;

            try
            {
                string userDocumentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                libraryPath = Path.Combine(userDocumentsPath, RLibraryPath);

                if (Directory.Exists(libraryPath))
                {
                    IEnumerable<string> directories = Directory.EnumerateDirectories(libraryPath);
                    if (directories.Count() == 1)
                    {
                        // Most common case
                        libraryPath = Path.Combine(libraryPath, directories.First());
                    }
                    else
                    {
                        string version = GetReducedVersion();
                        string path = Path.Combine(libraryPath, version);
                        if (Directory.Exists(path))
                        {
                            libraryPath = path;
                        }
                    }
                }
            }
            catch (IOException) { }

            return libraryPath;
        }

        internal static string GetReducedVersion()
        {
            string rVersionPath = RToolsSettings.Current.RVersionPath;
            string version = Path.GetFileName(rVersionPath);

            int index = version.IndexOf("R-", StringComparison.Ordinal);
            if (index >= 0)
            {
                version = version.Substring(index + 2);
                if (version.EndsWith(".0", StringComparison.Ordinal))
                {
                    version = version.Substring(0, version.Length - 2);
                }
            }

            return version;
        }
    }
}
