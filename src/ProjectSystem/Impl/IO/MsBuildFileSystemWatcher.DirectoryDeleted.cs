// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core;
using Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.Utilities;
using Microsoft.VisualStudio.ProjectSystem.Utilities;

namespace Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.IO {
    public sealed partial class MsBuildFileSystemWatcher {
        private class DirectoryDeleted : IFileSystemChange {
            private readonly string _rootDirectory;
            private readonly string _fullPath;

            public DirectoryDeleted(string rootDirectory, string fullPath) {
                _rootDirectory = rootDirectory;
                _fullPath = fullPath;
            }

            public void Apply(Changeset changeset) {
                if (!_fullPath.StartsWithIgnoreCase(_rootDirectory)) {
                    return;
                }

                var relativePath = PathHelper.EnsureTrailingSlash(PathHelper.MakeRelative(_rootDirectory, _fullPath));

                // Remove all the files and directories that start with relativePath
                changeset.AddedDirectories.RemoveWhere(d => d.StartsWithIgnoreCase(relativePath));
                changeset.AddedFiles.RemoveWhere(f => f.StartsWithIgnoreCase(relativePath));

                // If directory was previously added to AddedDirectories, we need to remove all its content as well
                if (changeset.AddedDirectories.Remove(relativePath)) {
                    return;
                }

                // If directory was renamed into relativePath, put the oldRelativePath into RemovedFiles instead.
                var oldRelativePath = changeset.RenamedDirectories.GetFirstKeyByValueIgnoreCase(relativePath);
                if (oldRelativePath != null) {
                    changeset.RenamedDirectories.Remove(oldRelativePath);
                    changeset.RemovedDirectories.Add(oldRelativePath);
                    return;
                }

                changeset.RemovedDirectories.Add(relativePath);
            }

            public override string ToString() {
                return $"Directory deleted: {_fullPath}";
            }
        }
    }
}