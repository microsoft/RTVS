// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Microsoft.VisualStudio.R.Package.Telemetry.Data {
    internal static class FolderUtility {
        /// <summary>
        /// Returns names of subfolders (relative paths) in a given directory.
        /// </summary>
        /// <param name="directory"></param>
        /// <returns></returns>
        public static IEnumerable<string> GetSubfolderRelativePaths(string directory) {
            if (Directory.Exists(directory)) {
                return Directory.EnumerateDirectories(directory).Select(x => x.Substring(directory.Length + 1));
            }
            return Enumerable.Empty<string>();
        }

        /// <summary>
        /// Counts files in a folder and its subfolders
        /// </summary>
        internal static int CountFiles(string path) {
            try {
                return Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories).Count();
            } catch (IOException) { }
            return 0;
        }
    }
}
