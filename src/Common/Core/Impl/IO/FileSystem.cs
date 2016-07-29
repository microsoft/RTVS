// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

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

        public string ReadAllText(string path) {
            return File.ReadAllText(path);
        }

        public void WriteAllText(string path, string content) {
            File.WriteAllText(path, content);
        }

        public IEnumerable<string> FileReadAllLines(string path) {
            return File.ReadLines(path);
        }

        public void FileWriteAllLines(string path, IEnumerable<string> contents) {
            File.WriteAllLines(path, contents);
        }

        public byte[] FileReadAllBytes(string path) {
            return File.ReadAllBytes(path);
        }

        public void FileWriteAllBytes(string path, byte[] bytes) {
            File.WriteAllBytes(path, bytes);
        }

        public bool DirectoryExists(string path) {
            return Directory.Exists(path);
        }

        public FileAttributes GetFileAttributes(string path) {
            return File.GetAttributes(path);
        }

        public string ToLongPath(string path) {
            var sb = new StringBuilder(NativeMethods.MAX_PATH);
            NativeMethods.GetLongPathName(path, sb, sb.Capacity);
            return sb.ToString();
        }

        public string ToShortPath(string path) {
            var sb = new StringBuilder(NativeMethods.MAX_PATH);
            NativeMethods.GetShortPathName(path, sb, sb.Capacity);
            return sb.ToString();
        }

        public IFileVersionInfo GetVersionInfo(string path) {
            var fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(path);
            return new FileVersionInfo(fvi.FileMajorPart, fvi.FileMinorPart);
        }

        public void DeleteFile(string path) {
            File.Delete(path);
        }

        public string[] GetFileSystemEntries(string path, string searchPattern, SearchOption options) {
            return Directory.GetFileSystemEntries(path, searchPattern, options);
        }

        public void CreateDirectory(string path) {
            Directory.CreateDirectory(path);
        }

        private static class NativeMethods {
            public const int MAX_PATH = 260;

            [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
            public static extern uint GetLongPathName([MarshalAs(UnmanagedType.LPWStr)] string lpFileName,
                                                      [MarshalAs(UnmanagedType.LPWStr)] StringBuilder lpBuffer,
                                                      int nBufferLength);

            [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
            public static extern uint GetShortPathName([MarshalAs(UnmanagedType.LPWStr)] string lpFileName,
                                                       [MarshalAs(UnmanagedType.LPWStr)] StringBuilder lpBuffer,
                                                       int nBufferLength);
        }
    }
}