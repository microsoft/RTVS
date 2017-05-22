// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using Microsoft.Common.Core;

namespace Microsoft.R.Components.Settings.Mirrors {
    /// <summary>
    /// Represents collection of CRAN mirrors. Default list is packaged
    /// as a CSV file in resources. Up to date list is downloaded and
    /// cached in TEMP folder when it is possible.
    /// </summary>
    public static class CranMirrorList {
        private static string _cranCsvFileTempPath;
        private static CranMirrorEntry[] _mirrors = new CranMirrorEntry[0];

        public static EventHandler DownloadComplete;

        /// <summary>
        /// Returns list of mirror names such as [Cloud 0] or 'Brazil'
        /// </summary>
        public static string[] MirrorNames {
            get { return _mirrors.Select(m => m.Name).ToArray(); }
        }

        /// <summary>
        /// Retrieves list of CRAN mirror URLs
        /// </summary>
        public static string[] MirrorUrls {
            get { return _mirrors.Select(m => m.Url).ToArray(); }
        }

        /// <summary>
        /// Given CRAN mirror name returns its URL.
        /// </summary>
        public static string UrlFromName(string name) {
            return _mirrors.FirstOrDefault((x) => x.Name.EqualsIgnoreCase(name))?.Url;
        }

        /// <summary>
        /// Initiates download of the CRAN mirror list
        /// fro the CRAN project site.
        /// </summary>
        public static void Download() {
            if (_mirrors.Length > 0) {
                if (DownloadComplete != null) {
                    DownloadComplete(null, EventArgs.Empty);
                }
                return;
            }

            // First set up local copy since download is async
            // and we may need data before it completes. Try file
            // downloaded earlier, if any. If there is none, try
            // reading local resource.

            _cranCsvFileTempPath = Path.Combine(Path.GetTempPath(), "CRAN_mirrors.csv");
            string content;

            if (File.Exists(_cranCsvFileTempPath)) {
                using (var fs = File.Open(_cranCsvFileTempPath, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                    var buffer = new byte[fs.Length];
                    fs.Read(buffer, 0, (int)fs.Length);
                    content = Encoding.UTF8.GetString(buffer);
                }
            } else {
                var assembly = Assembly.GetExecutingAssembly();
                var resourceStream = assembly.GetManifestResourceStream("Microsoft.R.Components.Settings.Mirrors.CranMirrors.csv");
                using (var streamReader = new StreamReader(resourceStream)) {
                    content = streamReader.ReadToEnd();
                }
            }

            ReadCsv(content);

            using (var webClient = new WebClient()) {
                var tempFile = Path.GetTempFileName();
                webClient.DownloadFileCompleted += OnDownloadFileCompleted;
                webClient.DownloadFileAsync(new Uri("https://cran.r-project.org/CRAN_mirrors.csv", UriKind.Absolute), tempFile, tempFile);
            }
        }

        private static void OnDownloadFileCompleted(object sender, AsyncCompletedEventArgs e) {
            var file = e.UserState as string;
            string content = null;

            if (!e.Cancelled && e.Error == null) {
                try {
                    if (File.Exists(file)) {
                        var sr = new StreamReader(file);
                        content = sr.ReadToEnd();
                        File.Delete(_cranCsvFileTempPath);
                        File.Copy(file, _cranCsvFileTempPath);
                    }
                } catch (IOException) { }
            }

            if (content != null) {
                ReadCsv(content);
            }

            if(DownloadComplete != null) {
                DownloadComplete(null, EventArgs.Empty);
            }
        }

        private static void ReadCsv(string content) {
            var lines = content.Split(CharExtensions.LineBreakChars, StringSplitOptions.RemoveEmptyEntries);
            char[] comma = { ',' };

            var entries = new List<CranMirrorEntry>();

            for (var i = 1; i < lines.Length; i++) {
                // Name, Country, City, URL, Host, Maintainer, OK, CountryCode, Comment
                var items = lines[i].Split(comma);
                if (items.Length >= 4) {
                    var e = new CranMirrorEntry() {
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
