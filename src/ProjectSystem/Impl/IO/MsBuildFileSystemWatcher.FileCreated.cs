// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Common.Core.IO;

namespace Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.IO {
    public sealed partial class MsBuildFileSystemWatcher {
        private class FileCreated : IFileSystemChange {
            private readonly MsBuildFileSystemWatcherEntries _entries;
            private readonly string _rootDirectory;
            private readonly IFileSystem _fileSystem;
            private readonly IMsBuildFileSystemFilter _fileSystemFilter;
            private readonly string _fullPath;

            public FileCreated(MsBuildFileSystemWatcherEntries entries, string rootDirectory, IFileSystem fileSystem, IMsBuildFileSystemFilter fileSystemFilter, string fullPath) {
                _entries = entries;
                _rootDirectory = rootDirectory;
                _fileSystem = fileSystem;
                _fileSystemFilter = fileSystemFilter;
                _fullPath = fullPath;
            }

            public void Apply() {
                string relativePath;
                if (!IsFileAllowed(_rootDirectory, _fullPath, _fileSystem, _fileSystemFilter, out relativePath)) {
                    return;
                }

                _entries.AddFile(relativePath);
            }

            public override string ToString() {
                return $"File created: {_fullPath}";
            }
        }
    }
}