// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using Microsoft.Common.Core;

namespace Microsoft.UnitTests.Core.XUnit {
    [ExcludeFromCodeCoverage]
    public abstract class DeployFilesFixture : IDisposable {
        /// <summary>
        /// Implementation assumes that repository has structure
        /// > [repo root]
        ///   > bin
        ///     > [build artifacts]
        ///   > src
        ///   > TestFiles
        ///     > [yyyy-MM-dd HH-mm-ss]
        /// and Microsoft.UnitTests.Core.dll is somewhere in [repo root]\bin\[build artifacts]
        /// </summary>
        private static readonly Lazy<string> RepoRootLazy = new Lazy<string>(() => {
            var path = Assembly.GetExecutingAssembly().GetAssemblyPath();
            var directory = Path.GetDirectoryName(path);
            var indexOfBin = directory.LastIndexOf(@"\bin\", StringComparison.Ordinal);
            return directory.Substring(0, indexOfBin);
        });

        private static readonly Lazy<string> TestFilesRootLazy = new Lazy<string>(() => Path.Combine(RepoRootLazy.Value, "TestFiles", DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss")));
        private static readonly Lazy<string> SolutionRootLazy = new Lazy<string>(() => Path.Combine(RepoRootLazy.Value, "src"));
        private static readonly object CopyFilesLock = new object();

        public static string SolutionRoot => SolutionRootLazy.Value;
        public static string TestFilesRoot => TestFilesRootLazy.Value;

        public string SourcePath { get; }
        public string DestinationPath { get; }

        public string GetSourcePath(string fileName) => Path.Combine(SourcePath, fileName);
        public string GetDestinationPath(string fileName) => Path.Combine(DestinationPath, fileName);

        public string LoadDestinationFile(string fileName) {
            var filePath = GetDestinationPath(fileName);

            using (var sr = new StreamReader(filePath)) {
                return sr.ReadToEnd();
            }
        }

        protected DeployFilesFixture(string relativeSource, string relativeDestination) {
            SourcePath = Path.Combine(SolutionRoot, relativeSource);
            DestinationPath = Path.Combine(TestFilesRoot, relativeDestination);

            try {
                CopyDirectory(SourcePath, DestinationPath);
            } catch (IOException) {
                // Swallow IO exceptions, let tests fail because of missing test files
            }
        }

        public void Dispose() {
            // We want to preserve input data for future comparison
        }

        protected static void CopyDirectory(string src, string dst) {
            CopyDirectory(src, dst, "*");
        }

        protected static void CopyDirectory(string src, string dst, string searchPattern) {
            lock (CopyFilesLock) {
                if (!Directory.Exists(src)) {
                    return;
                }

                Directory.CreateDirectory(dst);

                var dirs = Directory.GetDirectories(src);
                foreach (var srcDir in dirs) {
                    string srcDirName = Path.GetFileName(srcDir);
                    string dstDir = Path.Combine(dst, srcDirName);

                    CopyDirectory(srcDir, dstDir, searchPattern);
                }

                var srcFiles = Directory.GetFiles(src, searchPattern);
                var expectedDestinationFileSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var filePath in srcFiles) {
                    var fileName = Path.GetFileName(filePath);
                    var dstPath = Path.Combine(dst, fileName);
                    expectedDestinationFileSet.Add(dstPath);

                    if (!File.Exists(dstPath) || File.GetLastWriteTimeUtc(dstPath) < File.GetLastWriteTimeUtc(filePath)) {
                        File.Copy(filePath, dstPath, overwrite: true);
                        File.SetAttributes(dstPath, FileAttributes.Normal);
                    }
                }
            }
        }
    }
}