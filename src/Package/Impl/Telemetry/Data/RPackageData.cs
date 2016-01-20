using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.R.Actions.Utility;
using Microsoft.R.Support.Settings;

namespace Microsoft.VisualStudio.R.Package.Telemetry.Data {
    internal static class RPackageData {
        /// <summary>
        /// Retrieves hashes for all R package names in a given folder
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<string> GetInstalledPackageHashes(RPackageType packageType) {

            string rInstallPath = RInstallation.GetRInstallPath(RToolsSettings.Current.RBasePath);
            if (!string.IsNullOrEmpty(rInstallPath)) {
                IEnumerable<string> packageNames = Enumerable.Empty<string>();
                if (packageType == RPackageType.Base) {
                    packageNames = FolderUtility.GetSubfolderNames(Path.Combine(rInstallPath, @"\library"));
                } else {
                    Version v = RInstallation.GetRVersionFromFolderName(rInstallPath.Substring(rInstallPath.LastIndexOf('\\')));
                    if (v.MajorRevision > 0) {
                        string userLibraryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                                                              @"\R\win-library\",
                                                              v.MajorRevision.ToString() + "." + v.MinorRevision.ToString());

                        packageNames = FolderUtility.GetSubfolderNames(userLibraryPath);
                    }
                }

                foreach (string p in packageNames) {
                    string hash = CalculateMD5Hash(p);
                    yield return hash;
                }
            }
        }

        private static string CalculateMD5Hash(string input) {
            MD5 md5 = MD5.Create();
            byte[] inputBytes = Encoding.Unicode.GetBytes(input);
            byte[] hash = md5.ComputeHash(inputBytes);

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++) {
                sb.Append(hash[i].ToString("X2"));
            }
            return sb.ToString();
        }
    }
}
