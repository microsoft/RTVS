// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
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

        public void DeleteDirectory(string path, bool recursive) {
            Directory.Delete(path, recursive);
        }

        public string[] GetFileSystemEntries(string path, string searchPattern, SearchOption options) {
            return Directory.GetFileSystemEntries(path, searchPattern, options);
        }

        public void CreateDirectory(string path) {
            Directory.CreateDirectory(path);
        }

        public string GetDownloadsPath(string fileName) {
            if (string.IsNullOrWhiteSpace(fileName)) {
                return GetKnownFolder(KnownFolderGuids.Downloads);
            } else {
                return Path.Combine(GetKnownFolder(KnownFolderGuids.Downloads), fileName);
            }
        }

        private string GetKnownFolder(string knownFolder) {
            IntPtr knownFolderPath;
            uint flags = NativeMethods.KnownFolderflags.KF_FLAG_DEFAULT_PATH;
            int result = NativeMethods.SHGetKnownFolderPath(new Guid(knownFolder), flags, IntPtr.Zero, out knownFolderPath);
            if (result >= 0) {
                return Marshal.PtrToStringUni(knownFolderPath);
            } else {
                return string.Empty;
            }
        }

        private static class NativeMethods {
            public const int MAX_PATH = 260;

            public class KnownFolderflags {
                public const uint KF_FLAG_DEFAULT = 0x00000000;
                public const uint KF_FLAG_SIMPLE_IDLIST = 0x00000100;
                public const uint KF_FLAG_NOT_PARENT_RELATIVE = 0x00000200;
                public const uint KF_FLAG_DEFAULT_PATH = 0x00000400;
                public const uint KF_FLAG_INIT = 0x00000800;
                public const uint KF_FLAG_NO_ALIAS = 0x00001000;
                public const uint KF_FLAG_DONT_UNEXPAND = 0x00002000;
                public const uint KF_FLAG_DONT_VERIFY = 0x00004000;
                public const uint KF_FLAG_CREATE = 0x00008000;
                public const uint KF_FLAG_NO_APPCONTAINER_REDIRECTION = 0x00010000;
                public const uint KF_FLAG_ALIAS_ONLY = 0x80000000;
            }

            [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
            public static extern uint GetLongPathName([MarshalAs(UnmanagedType.LPWStr)] string lpFileName,
                                                      [MarshalAs(UnmanagedType.LPWStr)] StringBuilder lpBuffer,
                                                      int nBufferLength);

            [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
            public static extern uint GetShortPathName([MarshalAs(UnmanagedType.LPWStr)] string lpFileName,
                                                       [MarshalAs(UnmanagedType.LPWStr)] StringBuilder lpBuffer,
                                                       int nBufferLength);

            [DllImport("Shell32.dll")]
            public static extern int SHGetKnownFolderPath([MarshalAs(UnmanagedType.LPStruct)] Guid rfid, 
                                                          uint dwFlags, 
                                                          IntPtr hToken,
                                                          out IntPtr ppszPath);
        }
    }
}