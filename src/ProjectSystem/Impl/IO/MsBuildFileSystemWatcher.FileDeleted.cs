using System;
using Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.Utilities;
using Microsoft.VisualStudio.ProjectSystem.Utilities;

namespace Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.IO {
    public sealed partial class MsBuildFileSystemWatcher {
        private class FileDeleted : IFileSystemChange {
            private readonly string _rootDirectory;
            private readonly string _fullPath;

            public FileDeleted(string rootDirectory, string fullPath) {
                _rootDirectory = rootDirectory;
                _fullPath = fullPath;
            }

            public void Apply(Changeset changeset) {
                if (!_fullPath.StartsWith(_rootDirectory, StringComparison.OrdinalIgnoreCase)) {
                    return;
                }

                var relativePath = PathHelper.MakeRelative(_rootDirectory, _fullPath);

                // If the file with the same name was previously added, just remove it from the AddedFiles set
                if (changeset.AddedFiles.Remove(relativePath)) {
                    return;
                }

                // if the file was renamed into relativePath, put the oldRelativePath into RemovedFiles instead.
                var oldRelativePath = changeset.RenamedFiles.GetFirstKeyByValueIgnoreCase(relativePath);
                if (oldRelativePath != null) {
                    changeset.RenamedFiles.Remove(oldRelativePath);
                    changeset.RemovedFiles.Add(oldRelativePath);
                    return;
                }

                changeset.RemovedFiles.Add(relativePath);
            }

            public override string ToString() {
                return $"File deleted: {_fullPath}";
            }
        }

    }
}