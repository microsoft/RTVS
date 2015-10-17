using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

namespace Microsoft.VisualStudio.R.Package.RPackages.Mirrors
{
    internal static class CranMirrorList
    {
        private static string _cranCsvFileTempPath;
        private static CranMirrorEntry[] _mirrors = new CranMirrorEntry[0];
        private static bool localCopy;

        public static string[] MirrorNames
        {
            get
            {
                Init();

                List<string> names = new List<string>();
                foreach (var m in _mirrors)
                {
                    names.Add(m.Name);
                }

                return names.ToArray();
            }
        }

        public static string[] MirrorUrls
        {
            get
            {
                Init();

                List<string> urls = new List<string>();
                foreach (var m in _mirrors)
                {
                    urls.Add(m.Url);
                }

                return urls.ToArray();
            }
        }

        public static string UrlFromName(string name)
        {
            Init();

            CranMirrorEntry e = _mirrors.FirstOrDefault((x) => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            return e != null ? e.Name : null;
        }

        private static void Init()
        {
            if (_mirrors.Length > 0 && !localCopy)
            {
                return;
            }

            Task.Run(() =>
            {
                IReadOnlyCollection<string> mirrors = new string[] { "https://cran.rstudio.com" };
                _cranCsvFileTempPath = Path.Combine(Path.GetTempPath(), "CRAN_mirrors.csv");

                using (WebClient webClient = new WebClient())
                {
                    webClient.DownloadFile(new Uri("https://cran.r-project.org/CRAN_mirrors.csv", UriKind.Absolute), _cranCsvFileTempPath);
                }
            }).Wait(1000);

            string content = null;
            try
            {
                if (File.Exists(_cranCsvFileTempPath))
                {
                    var sr = new StreamReader(_cranCsvFileTempPath);
                    content = sr.ReadToEnd();
                    localCopy = false;
                }
            }
            catch (IOException) { }

            if (content == null)
            {
                // read local resource
                var assembly = Assembly.GetExecutingAssembly();
                var resourceStream = assembly.GetManifestResourceStream("Microsoft.VisualStudio.R.Package.RPackages.Mirrors.CranMirrors.csv");
                var streamReader = new StreamReader(resourceStream);
                content = streamReader.ReadToEnd();
                localCopy = true;
            }

            ReadCsv(content);
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
