// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Microsoft.Common.Core.IO {
    public sealed class FileSystem : IFileSystem {
        public IFileSystemWatcher CreateFileSystemWatcher(string path, string filter) {
            return new FileSystemWatcherProxy(path, filter);
        }

        public IDirectoryInfo GetDirectoryInfo(string directoryPath) {
            return new DirectoryInfoProxy(directoryPath);
        }

        public bool FileExists(string path) {
            return File.Exists(path);
        }

        public IEnumerable<string> FileReadAllLines(string path) {
            return File.ReadLines(path);
        }

        public void FileWriteAllLines(string path, IEnumerable<string> contents) {
            File.WriteAllLines(path, contents);
        }

        public bool DirectoryExists(string path) {
            return Directory.Exists(path);
        }

        public FileAttributes GetFileAttributes(string path) {
            return File.GetAttributes(path);
        }
        public IFileVersionInfo GetVersionInfo(string path) {
            var fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(path);
            return new FileVersionInfo(fvi.FileMajorPart, fvi.FileMinorPart);
        }
    }
}