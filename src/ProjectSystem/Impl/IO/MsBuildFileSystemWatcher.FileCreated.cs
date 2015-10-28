using Microsoft.Common.Core.IO;

namespace Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.IO {
    public sealed partial class MsBuildFileSystemWatcher {
        private class FileCreated : IFileSystemChange {
            private readonly string _rootDirectory;
            private readonly IFileSystem _fileSystem;
            private readonly IMsBuildFileSystemFilter _fileSystemFilter;
            private readonly string _fullPath;

            public FileCreated(string rootDirectory, IFileSystem fileSystem, IMsBuildFileSystemFilter fileSystemFilter, string fullPath) {
                _rootDirectory = rootDirectory;
                _fileSystem = fileSystem;
                _fileSystemFilter = fileSystemFilter;
                _fullPath = fullPath;
            }

            public void Apply(Changeset changeset) {
                string relativePath;
                if (!IsFileAllowed(_rootDirectory, _fullPath, _fileSystem, _fileSystemFilter, out relativePath)) {
                    return;
                }

                // If file with the same name was removed, just remove it from the RemovedFiles set
                if (changeset.RemovedFiles.Remove(relativePath)) {
                    return;
                }

                // If file had this name before renaming, remove it from RenamedFiles and add to AddedFiles instead
                string renamedFile;
                if (changeset.RenamedFiles.TryGetValue(relativePath, out renamedFile)) {
                    changeset.RenamedFiles.Remove(relativePath);
                    changeset.AddedFiles.Add(renamedFile);
                    return;
                }

                changeset.AddedFiles.Add(relativePath);
            }

            public override string ToString() {
                return $"File created: {_fullPath}";
            }
        }
    }
}