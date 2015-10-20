using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;

namespace Microsoft.VisualStudio.R.Package.RPackages.Mirrors
{
    /// <summary>
    /// Represents collection of CRAN mirrors. Default list is packaged
    /// as a CSV file in resources. Up to date list is downloaded and
    /// cached in TEMP folder when it is possible.
    /// </summary>
    internal static class CranMirrorList
    {
        private static string _cranCsvFileTempPath;
        private static CranMirrorEntry[] _mirrors = new CranMirrorEntry[0];

        /// <summary>
        /// Returns list of mirror names such as [Cloud 0] or 'Brazil'
        /// </summary>
        public static string[] MirrorNames
        {
            get { return _mirrors.Select(m => m.Name).ToArray(); }
        }

        /// <summary>
        /// Retrieves list of CRAN mirror URLs
        /// </summary>
        public static string[] MirrorUrls
        {
            get { return _mirrors.Select(m => m.Url).ToArray(); }
        }

        /// <summary>
        /// Given CRAN mirror name returns its URL.
        /// If no mirror found, returns default URL
        /// of RStudio CRAN redirector.
        /// </summary>
        public static string UrlFromName(string name)
        {
            CranMirrorEntry e = _mirrors.FirstOrDefault((x) => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            return e != null ? e.Url : "https://cran.rstudio.com";
        }

        /// <summary>
        /// Initiates download of the CRAN mirror list
        /// fro the CRAN project site.
        /// </summary>
        public static void Download()
        {
            if (_mirrors.Length > 0)
            {
                return;
            }

            // First set up local copy since download is async
            // and we may need data before it completes. Try file
            // downloaded earlier, if any. If there is none, try
            // reading local resource.

            _cranCsvFileTempPath = Path.Combine(Path.GetTempPath(), "CRAN_mirrors.csv");
            string content;

            if (File.Exists(_cranCsvFileTempPath))
            {
                using (var streamReader = new StreamReader(_cranCsvFileTempPath))
                {
                    content = streamReader.ReadToEnd();
                }
            }
            else
            {
                var assembly = Assembly.GetExecutingAssembly();
                var resourceStream = assembly.GetManifestResourceStream("Microsoft.VisualStudio.R.Package.RPackages.Mirrors.CranMirrors.csv");
                using (var streamReader = new StreamReader(resourceStream))
                {
                    content = streamReader.ReadToEnd();
                }
            }

            ReadCsv(content);

            using (WebClient webClient = new WebClient())
            {
                webClient.DownloadFileCompleted += OnDownloadFileCompleted;
                webClient.DownloadFileAsync(new Uri("https://cran.r-project.org/CRAN_mirrors.csv", UriKind.Absolute), _cranCsvFileTempPath);
            }
        }

        private static void OnDownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            string content = null;

            if (!e.Cancelled && e.Error != null)
            {
                try
                {
                    if (File.Exists(_cranCsvFileTempPath))
                    {
                        var sr = new StreamReader(_cranCsvFileTempPath);
                        content = sr.ReadToEnd();
                    }
                }
                catch (IOException) { }
            }

            if (content != null)
            {
                ReadCsv(content);
            }
        }

        private static void ReadCsv(string content)
        {
            string[] lines = content.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            char[] comma = new char[] { ',' };

            List<CranMirrorEntry> entries = new List<CranMirrorEntry>();

            for (int i = 1; i < lines.Length; i++)
            {
                // Name, Country, City, URL, Host, Maintainer, OK, CountryCode, Comment
                string[] items = lines[i].Split(comma);
                if (items.Length >= 4)
                {
                    CranMirrorEntry e = new CranMirrorEntry()
                    {
                        Name = items[0].Replace("\"", string.Empty),
                        Country = items[1].Replace("\"", string.Empty),
                        City = items[2].Replace("\"", string.Empty),
                        Url = items[3].Replace("\"", string.Empty),
                    };

                    entries.Add(e);
                }
            }

            _mirrors = entries.ToArray();
        }
    }
}
