using Microsoft.Common.Core;
using Microsoft.Common.Core.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Microsoft.R.Common.Core.Linux {
    public class InstalledPackageInfo {
        public string PackageName { get; }
        public string Version { get; }
        public string Architecture { get; }

        private readonly Version _parsedVersion;
        public Version GetVersion() => _parsedVersion;

        public InstalledPackageInfo(string name, string version, string architecture) {
            PackageName = name;
            Version = version;
            Architecture = architecture;
            _parsedVersion = ParseVersion(version);
        }

        public IEnumerable<string> GetPackageFiles(IFileSystem fs) {
            string path = $"/var/lib/dpkg/info/{PackageName}.list";
            if (fs.FileExists(path)) {
                return fs.FileReadAllLines(path);
            }

            path = $"/var/lib/dpkg/info/{PackageName}:{Architecture}.list";
            if (fs.FileExists(path)) {
                return fs.FileReadAllLines(path);
            }

            var files = Directory.GetFiles("/var/lib/dpkg/info", $"{PackageName}*.list", SearchOption.TopDirectoryOnly);
            if (files.Length > 0) {
                return fs.FileReadAllLines(files[0]);
            }

            return new string[0];
        }

        public static IEnumerable<InstalledPackageInfo> GetPackages(IFileSystem fs) {
            var list = new List<InstalledPackageInfo>();
            const string packagePart = "Package: ";
            const string versionPart = "Version: ";
            const string architecturePart = "Architecture: ";
            try {
                var lines = fs.FileReadAllLines("/var/lib/dpkg/status").ToArray();
                int i = 0;
                while (i<lines.Length) {
                    string packageName = GetPart(lines, packagePart, ref i);
                    string architecture = GetPart(lines, architecturePart, ref i);
                    string version = GetPart(lines, versionPart, ref i);
                    if (!string.IsNullOrEmpty(packageName) &&
                        !string.IsNullOrEmpty(version) &&
                        !string.IsNullOrEmpty(architecture)) {
                        list.Add(new InstalledPackageInfo(packageName, version, architecture));
                    }

                    // find the next blank line
                    string line = null;
                    do {
                        line = lines[i];
                        ++i;
                    } while (i < lines.Length && !string.IsNullOrWhiteSpace(line));
                }
            } catch (Exception ex) when (!ex.IsCriticalException()) {
            }
            return list;
        }

        private static string GetPart(string[] lines, string part, ref int index) {
            if (index < lines.Length) {
                string line = lines[index];
                while (!line.StartsWith(part)) {
                    if (++index >= lines.Length) {
                        return null;
                    }
                    line = lines[index];
                }
                return line.Substring(part.Length).Trim();
            }
            return null;
        }

        static string[] versionSeperators = { ".", "-", "~", " ", ":", "+" };
        private static Version ParseVersion(string version) {
            // this is linux package version string which might contain text
            // e.g, 3.2.3~pre-6ubuntu8 OR 8c-0ubuntu1 OR 2016asdc-ubuntu02
            // extract major and minor version, only if it exists
            string[] split = version.Split(versionSeperators, StringSplitOptions.RemoveEmptyEntries);

            Version v;
            if (split.Length >= 4 && System.Version.TryParse($"{split[0]}.{split[1]}.{split[2]}.{split[3]}", out v)) {
                return v;
            } else if (split.Length >= 3 && System.Version.TryParse($"{split[0]}.{split[1]}.{split[2]}", out v)) {
                return v;
            } else if(split.Length >= 2 && System.Version.TryParse($"{split[0]}.{split[1]}", out v)) {
                return v;
            } else if (split.Length >= 1 && System.Version.TryParse($"{split[0]}.0", out v)) {
                return v;
            }

            return new Version(0, 0);
        }
    }
}
