using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.R.Support.Settings;

namespace Microsoft.R.Support.Packages
{
    public static class InstalledPackages
    {
        public static IEnumerable<PackageInfo> GetBasePackages(bool fetchDescriptions = false)
        {
            try
            {
                string rVersionPath = RToolsSettings.GetRVersionPath();
                string libraryPath = Path.Combine(rVersionPath, "library");
                return new PackageEnumeration(libraryPath, fetchDescriptions);
            }
            catch (IOException) { }

            return new PackageInfo[0];
        }

        public static IEnumerable<PackageInfo> GetUserPackages(bool fetchDescriptions = false)
        {
            string libraryPath = InstalledPackages.UserPackagesPath;
            if (!string.IsNullOrEmpty(libraryPath))
            {
                return new PackageEnumeration(libraryPath, fetchDescriptions);
            }

            return new PackageInfo[0];
        }

        public static string UserPackagesPath
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

        private class PackageEnumeration : IEnumerable<PackageInfo>
        {
            private string _libraryPath;
            private bool _fetchDescriptions;

            public PackageEnumeration(string libraryPath, bool fetchDescriptions = false)
            {
                _libraryPath = libraryPath;
                _fetchDescriptions = fetchDescriptions;
            }

            public IEnumerator<PackageInfo> GetEnumerator()
            {
                return new PackageEnumerator(_libraryPath, _fetchDescriptions);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }
        }

        private class PackageEnumerator : IEnumerator<PackageInfo>
        {
            private IEnumerator<string> _directoriesEnumerator;
            private bool _fetchDescriptions;

            public PackageEnumerator(string libraryPath, bool fetchDescriptions = false)
            {
                _fetchDescriptions = fetchDescriptions;
                _directoriesEnumerator = Directory.EnumerateDirectories(libraryPath).GetEnumerator();
            }

            public PackageInfo Current
            {
                get
                {
                    string directoryPath = _directoriesEnumerator.Current;
                    string name = Path.GetFileName(directoryPath).ToLowerInvariant();

                    PackageInfo packageInfo = new PackageInfo(name, Path.GetDirectoryName(directoryPath), _fetchDescriptions);

                    packageInfo.LoadFunctionsAsync();
                    return packageInfo;
                }
            }

            object IEnumerator.Current
            {
                get { return this.Current; }
            }

            public bool MoveNext()
            {
                return _directoriesEnumerator.MoveNext();
            }

            public void Reset()
            {
                _directoriesEnumerator.Reset();
            }

            public void Dispose()
            {
            }
        }
    }
}
