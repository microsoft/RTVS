// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core;
using Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.Utilities;
using Microsoft.VisualStudio.ProjectSystem.Utilities;

namespace Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.IO {
    public sealed partial class MsBuildFileSystemWatcher {
        private class DirectoryDeleted : IFileSystemChange {
            private readonly MsBuildFileSystemWatcherEntries _entries;
            private readonly string _rootDirectory;
            private readonly string _fullPath;

            public DirectoryDeleted(MsBuildFileSystemWatcherEntries entries, string rootDirectory, string fullPath) {
                _entries = entries;
                _rootDirectory = rootDirectory;
                _fullPath = fullPath;
            }

            public void Apply() {
                if (!_fullPath.StartsWithIgnoreCase(_rootDirectory)) {
                    return;
                }

                var relativePath = PathHelper.EnsureTrailingSlash(PathHelper.MakeRelative(_rootDirectory, _fullPath));
                _entries.DeleteDirectory(relativePath);
            }

            public override string ToString() {
                return $"Directory deleted: {_fullPath}";
            }
        }
    }
}