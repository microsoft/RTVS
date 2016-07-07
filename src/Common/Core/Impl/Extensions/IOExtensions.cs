// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.IO;

namespace Microsoft.Common.Core {
    public static class IOExtensions {
        public static string MakeRelativePath(this string path, string basePath) {
            if (string.IsNullOrWhiteSpace(path) || string.IsNullOrWhiteSpace(basePath)) {
                return path;
            }
            var bp = basePath.EndsWithOrdinal("\\") ? basePath : basePath + "\\";
            if (path.EqualsIgnoreCase(bp) || bp.EqualsIgnoreCase(path + "\\")) {
                return string.Empty;
            }
            return path.StartsWithIgnoreCase(bp) ? path.Substring(bp.Length) : path;
        }

        public static bool ExistsOnPath(string fileName) {
            return GetFullPath(fileName) != null;
        }

        public static string GetFullPath(string fileName) {
            if (File.Exists(fileName)) {
                return Path.GetFullPath(fileName);
            }

            var values = Environment.GetEnvironmentVariable("PATH");
            var paths = values.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var path in paths) {
                var fullPath = Path.Combine(path, fileName);
                if (File.Exists(fullPath)) {
                    return fullPath;
                }
            }
            return null;
        }
    }
}
