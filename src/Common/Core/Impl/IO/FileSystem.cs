using System.Collections.Generic;
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
    }
}