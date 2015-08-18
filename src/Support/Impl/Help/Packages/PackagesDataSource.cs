using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.R.Support.Help.Definitions;
using Microsoft.R.Support.Settings;

namespace Microsoft.R.Support.Help.Packages
{
    public class PackagesDataSource : IPackageCollection
    {
        #region IPackagesDataSource
        public string BasePackagesPath
        {
            get
            {
                try
                {
                    string rVersionPath = RToolsSettings.GetRVersionPath();
                    return Path.Combine(rVersionPath, "library");
                }
                catch (IOException) { }

                return string.Empty;
            }
        }

        public IEnumerable<IPackageInfo> BasePackages
        {
            get
            {
                try
                {
                    string libraryPath = this.BasePackagesPath;
                    if (string.IsNullOrEmpty(libraryPath))
                    {
                        return new PackageEnumeration(libraryPath, isBase: true);
                    }
                }
                catch (IOException) { }

                return new IPackageInfo[0];
            }
        }

        public string UserPackagesPath
        {
            get
            {
                string libraryPath = string.Empty;

                try
                {
                    string userDocumentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    libraryPath = Path.Combine(userDocumentsPath, @"R\win-library");

                    IEnumerable<string> directories = Directory.EnumerateDirectories(libraryPath);
                    if (directories.Count() == 1)
                    {
                        // Most common case
                        libraryPath = Path.Combine(libraryPath, directories.First());
                    }
                    else
                    {
                        string version = GetReducedVersion();
                        libraryPath = Path.Combine(libraryPath, version);
                    }
                }
                catch (IOException) { }

                return libraryPath;
            }
        }

        public IEnumerable<IPackageInfo> UserPackages
        {
            get
            {
                string libraryPath = this.UserPackagesPath;
                if (!string.IsNullOrEmpty(libraryPath))
                {
                    return new PackageEnumeration(libraryPath, isBase: false);
                }

                return new IPackageInfo[0];
            }
        }

        public IEnumerable<IPackageInfo> InstalledPackages
        {
            get
            {
                string libraryPath = this.UserPackagesPath;
                if (!string.IsNullOrEmpty(libraryPath))
                {
                    return new PackageEnumeration(libraryPath, isBase: false);
                }

                return new IPackageInfo[0];
            }
        }
        #endregion

        private static string GetReducedVersion()
        {
            string rVersionPath = RToolsSettings.GetRVersionPath();
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
