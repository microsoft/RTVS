using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.R.Support.Settings;

namespace Microsoft.R.Support.Help.Packages
{
    public sealed class UserPackagesCollection : PackagesCollection
    {
        public override string InstallPath
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
    }
}
