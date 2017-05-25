// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using Microsoft.Common.Core.IO;

namespace Microsoft.Common.Core {
    public static class IOExtensions {
        public static string MakeRelativePath(this string path, string basePath) {
            if (string.IsNullOrWhiteSpace(path) || string.IsNullOrWhiteSpace(basePath)) {
                return path;
            }
            var bp = basePath.EndsWith(Path.DirectorySeparatorChar) ? basePath : basePath + Path.DirectorySeparatorChar;
            if (path.PathEquals(bp)) {
                return string.Empty;
            }
            return path.StartsWithIgnoreCase(bp) ? path.Substring(bp.Length) : path;
        }

        public static bool ExistsOnPath(string fileName) => GetFullPath(fileName) != null;

        public static string GetFullPath(string fileName) {
            if (File.Exists(fileName)) {
                return Path.GetFullPath(fileName);
            }

            var values = Environment.GetEnvironmentVariable("PATH");
            var paths = values.Split(new [] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var path in paths) {
                var fullPath = Path.Combine(path, fileName);
                if (File.Exists(fullPath)) {
                    return fullPath;
                }
            }
            return null;
        }

        /// <summary>
        /// Recursively enumerate sub-directories and gets all files for the given <paramref name="basedir"/>
        /// </summary>
        public static IEnumerable<IFileSystemInfo> GetAllFiles(this IDirectoryInfo basedir) {
            var files = new List<IFileSystemInfo>();
            var dirs = new Queue<IDirectoryInfo>();
            dirs.Enqueue(basedir);
            while (dirs.Count > 0) {
                var dir = dirs.Dequeue();
                foreach (var info in dir.EnumerateFileSystemInfos()) {
                    if (info is IDirectoryInfo subdir) {
                        dirs.Enqueue(subdir);
                    } else {
                        files.Add(info);
                    }
                }
            }
            return files;
        }

        /// <summary>
        /// Returns subdirectories of a directory
        /// </summary>
        public static IEnumerable<string> GetDirectories(this IFileSystem fs, string directory)
            => fs.GetDirectoryInfo(directory).EnumerateFileSystemInfos()
                 .Where(fsi => (fsi.Attributes & FileAttributes.Directory) == FileAttributes.Directory)
                 .Select(f => f.FullName);

        /// <summary>
        /// Returns full paths of files within a directory
        /// </summary>
        public static IEnumerable<string> GetFiles(this IFileSystem fs, string directory)
            => fs.GetDirectoryInfo(directory).EnumerateFileSystemInfos()
                 .Where(fsi => (fsi.Attributes & (FileAttributes.Directory | FileAttributes.Device)) == 0)
                 .Select(f => f.FullName);

        public static string TrimTrailingSlash(this string path) {
            if (!string.IsNullOrEmpty(path) && (path.EndsWith(Path.DirectorySeparatorChar) || path.EndsWith(Path.AltDirectorySeparatorChar))) {
                return path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            }
            return path;
        }

        public static bool PathEquals(this string path, string otherPath) {
            var pathLength = GetTrailingSlashTrimmedPathLength(path);
            var otherPathLength = GetTrailingSlashTrimmedPathLength(otherPath);

            return pathLength == otherPathLength && (pathLength == 0 || string.Compare(path, 0, otherPath, 0, otherPathLength, StringComparison.OrdinalIgnoreCase) == 0);
        }

        private static int GetTrailingSlashTrimmedPathLength(string path) {
            if (string.IsNullOrEmpty(path)) {
                return 0;
            }
            if (path.EndsWith(Path.DirectorySeparatorChar) || path.EndsWith(Path.AltDirectorySeparatorChar)) {
                return path.Length - 1;
            }
            return path.Length;
        }

        public static string EnsureTrailingSlash(this string path) {
            if (!string.IsNullOrEmpty(path)) {
                if (path[path.Length - 1] == Path.DirectorySeparatorChar || path[path.Length - 1] == Path.AltDirectorySeparatorChar) {
                    return path;
                }
                var slash = path.IndexOf(Path.AltDirectorySeparatorChar) >= 0 ? Path.AltDirectorySeparatorChar : Path.DirectorySeparatorChar;
                return path + slash;
            }
            return Path.DirectorySeparatorChar.ToString();
        }
    }
}
