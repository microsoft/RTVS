using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Common.Core;
using Microsoft.Common.Core.IO;
using Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.Utilities;
using Microsoft.VisualStudio.ProjectSystem.Utilities;

namespace Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.IO {
    public sealed partial class MsBuildFileSystemWatcher {
        private class DirectoryRenamed : IFileSystemChange {
            private readonly string _rootDirectory;
            private readonly IFileSystem _fileSystem;
            private readonly IMsBuildFileSystemFilter _fileSystemFilter;
            private readonly string _oldFullPath;
            private readonly string _fullPath;

            public DirectoryRenamed(string rootDirectory, IFileSystem fileSystem, IMsBuildFileSystemFilter fileSystemFilter, string oldFullPath, string fullPath) {
                _rootDirectory = rootDirectory;
                _fileSystem = fileSystem;
                _fileSystemFilter = fileSystemFilter;
                _oldFullPath = oldFullPath;
                _fullPath = fullPath;
            }

            public void Apply(Changeset changeset) {
                if (!_fullPath.StartsWithIgnoreCase(_rootDirectory)) {
                    return;
                }

                var newDirectoryInfo = _fileSystem.GetDirectoryInfo(_fullPath);
                var newRelativePath = PathHelper.MakeRelative(_rootDirectory, _fullPath);
                if (!newDirectoryInfo.Exists || !_fileSystemFilter.IsDirectoryAllowed(newRelativePath, newDirectoryInfo.Attributes)) {
                    return;
                }

                newRelativePath = PathHelper.EnsureTrailingSlash(newRelativePath);
                var oldRelativePath = PathHelper.EnsureTrailingSlash(PathHelper.MakeRelative(_rootDirectory, _oldFullPath));

                // If directory with the oldRelativePath was previously added, remove it from the AddedDirectories, add newRelativePath and change all its content paths:
                if (changeset.AddedDirectories.Contains(oldRelativePath)) {
                    UpdatePrefix(changeset.AddedDirectories, oldRelativePath, newRelativePath);
                    UpdatePrefix(changeset.AddedFiles, oldRelativePath, newRelativePath);
                    return;
                }

                // if directory with the newRelativePath was previously deleted, keep both changes (delete and rename),
                // cause the content of the directory to be deleted is different from the content of renamed directory

                // if there is a directory that was renamed into oldRelativePath, rename it to newRelativePath instead
                // or remove from RenamedDirectories if previouslyRenamedRelativePath equal to newRelativePath
                var previouslyRenamedRelativePath = changeset.RenamedDirectories.GetFirstKeyByValueIgnoreCase(oldRelativePath);
                if (string.IsNullOrEmpty(previouslyRenamedRelativePath)) {
                    changeset.RenamedDirectories[oldRelativePath] = newRelativePath;
                } else if (previouslyRenamedRelativePath.EqualsIgnoreCase(newRelativePath)) {
                    changeset.RenamedDirectories.Remove(previouslyRenamedRelativePath);
                } else {
                    changeset.RenamedDirectories[previouslyRenamedRelativePath] = newRelativePath;
                }
            }

            private void UpdatePrefix(HashSet<string> items, string oldPrefix, string newPrefix) {
                var itemsToUpdate = items.Where(a => a.StartsWithIgnoreCase(oldPrefix)).ToList();
                foreach (var item in itemsToUpdate) {
                    items.Remove(item);
                    items.Add(newPrefix + item.Substring(oldPrefix.Length));
                }
            }

            public override string ToString() {
                return $"Directory renamed: {_oldFullPath} -> {_fullPath}";
            }
        }
    }
}