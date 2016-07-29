// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.IO;

namespace Microsoft.Common.Core.IO {
    public interface IFileSystem {
        IFileSystemWatcher CreateFileSystemWatcher(string directory, string filter);
        IDirectoryInfo GetDirectoryInfo(string directoryPath);
        bool FileExists(string fullPath);
        bool DirectoryExists(string fullPath);
        FileAttributes GetFileAttributes(string fullPath);
        string ToLongPath(string path);
        string ToShortPath(string path);

        string ReadAllText(string path);
        void WriteAllText(string path, string content);

        IEnumerable<string> FileReadAllLines(string path);
        void FileWriteAllLines(string path, IEnumerable<string> contents);

        byte[] FileReadAllBytes(string path);
        void FileWriteAllBytes(string path, byte[] bytes);

        IFileVersionInfo GetVersionInfo(string path);
        void DeleteFile(string path);
        string[] GetFileSystemEntries(string path, string searchPattern, SearchOption options);
        void CreateDirectory(string path);
    }
}
