using System.Collections.Generic;
using System.IO;

namespace Microsoft.VisualStudio.R.Package.Telemetry.Data {
    internal static class FolderUtility {
        public static IEnumerable<string> GetSubfolderNames(string directory) {
            List<string> names = new List<string>();
            if (Directory.Exists(directory)) {
                foreach (string dir in Directory.EnumerateDirectories(directory)) {
                    string subFolderName = dir.Substring(directory.Length + 1);
                    yield return subFolderName;
                }
            }
        }

        /// <summary>
        /// Counts files in a folder and its subfolders
        /// </summary>
        internal static int CountFiles(string path) {
            int count = 0;
            try {
                count = Directory.GetFiles(path).Length;
                foreach (string dir in Directory.EnumerateDirectories(path)) {
                    count += CountFiles(dir);
                }
            } catch (IOException) { }
            return count;
        }
    }
}
