// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core;
using Microsoft.Common.Core.IO;
using Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.Utilities;
using Microsoft.VisualStudio.ProjectSystem.Utilities;

namespace Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.IO {
    public sealed partial class MsBuildFileSystemWatcher {
        private class FileRenamed : IFileSystemChange {
            private readonly string _rootDirectory;
            private readonly IFileSystem _fileSystem;
            private readonly IMsBuildFileSystemFilter _fileSystemFilter;
            private readonly string _oldFullPath;
            private readonly string _fullPath;

            public FileRenamed(string rootDirectory, IFileSystem fileSystem, IMsBuildFileSystemFilter fileSystemFilter, string oldFullPath, string fullPath) {
                _rootDirectory = rootDirectory;
                _fileSystem = fileSystem;
                _fileSystemFilter = fileSystemFilter;
                _oldFullPath = oldFullPath;
                _fullPath = fullPath;
            }

            public void Apply(Changeset changeset) {
                string newRelativePath;
                if (!IsFileAllowed(_rootDirectory, _fullPath, _fileSystem, _fileSystemFilter, out newRelativePath)) {
                    return;
                }

                var oldRelativePath = PathHelper.MakeRelative(_rootDirectory, _oldFullPath);
                var isRename = true;

                // If file with the oldRelativePath was previously added, remove it from the AddedFiles and add newRelativePath
                if (changeset.AddedFiles.Remove(oldRelativePath)) {
                    changeset.AddedFiles.Add(newRelativePath);
                    isRename = false;
                }

                // if file with the newRelativePath was previously deleted, remove it from the RemovedFiles and add oldRelativePath
                if (changeset.RemovedFiles.Remove(newRelativePath)) {
                    changeset.RemovedFiles.Add(oldRelativePath);
                    isRename = false;
                }

                if (!isRename) {
                    return;
                }

                // if there is a file that was renamed into oldRelativePath, rename it to newRelativePath instead
                // or remove from RenamedFiles if previouslyRenamedRelativePath equal to newRelativePath
                var previouslyRenamedRelativePath = changeset.RenamedFiles.GetFirstKeyByValueIgnoreCase(oldRelativePath);
                if (string.IsNullOrEmpty(previouslyRenamedRelativePath)) {
                    changeset.RenamedFiles[oldRelativePath] = newRelativePath;
                } else if (previouslyRenamedRelativePath.EqualsIgnoreCase(newRelativePath)) {
                    changeset.RenamedFiles.Remove(previouslyRenamedRelativePath);
                } else {
                    changeset.RenamedFiles[previouslyRenamedRelativePath] = newRelativePath;
                }
            }

            public override string ToString() {
                return $"File renamed: {_oldFullPath} -> {_fullPath}";
            }
        }
    }
}