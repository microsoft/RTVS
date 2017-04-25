// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Common.Core.IO;
using System;
using Microsoft.Extensions.FileSystemGlobbing;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.IO.Compression;
using Microsoft.Common.Core;
using System.Diagnostics;

namespace Microsoft.R.Common.Core.Linux {
    public class FileSystem : IFileSystem {

        public IFileSystemWatcher CreateFileSystemWatcher(string path, string filter) => new FileSystemWatcherProxy(path, filter);

        public IDirectoryInfo GetDirectoryInfo(string directoryPath) => new DirectoryInfoProxy(directoryPath);

        public bool FileExists(string path) => File.Exists(path);

        public long FileSize(string path) {
            var fileInfo = new FileInfo(path);
            return fileInfo.Length;
        }

        public string ReadAllText(string path) => File.ReadAllText(path);

        public void WriteAllText(string path, string content) => File.WriteAllText(path, content);

        public IEnumerable<string> FileReadAllLines(string path) => File.ReadLines(path);

        public void FileWriteAllLines(string path, IEnumerable<string> contents) => File.WriteAllLines(path, contents);

        public byte[] FileReadAllBytes(string path) => File.ReadAllBytes(path);

        public void FileWriteAllBytes(string path, byte[] bytes) => File.WriteAllBytes(path, bytes);

        public Stream CreateFile(string path) => File.Create(path);
        public Stream FileOpen(string path, FileMode mode) => File.Open(path, mode);

        public bool DirectoryExists(string path) => Directory.Exists(path);

        public FileAttributes GetFileAttributes(string path) => File.GetAttributes(path);

        public string ToLongPath(string path) => path;

        public string ToShortPath(string path) => path;

        public Version GetFileVersion(string path) {
            var fvi = FileVersionInfo.GetVersionInfo(path);
            return new Version(fvi.FileMajorPart, fvi.FileMinorPart, fvi.FileBuildPart, fvi.FilePrivatePart);
        }

        public void DeleteFile(string path) => File.Delete(path);

        public void DeleteDirectory(string path, bool recursive) => Directory.Delete(path, recursive);

        public string[] GetFileSystemEntries(string path, string searchPattern, SearchOption options) => Directory.GetFileSystemEntries(path, searchPattern, options);

        public void CreateDirectory(string path) => Directory.CreateDirectory(path);

        public string CompressFile(string path, string relativeTodir) {
            string zipFilePath = Path.GetTempFileName();
            using (FileStream zipStream = new FileStream(zipFilePath, FileMode.Create))
            using (ZipArchive archive = new ZipArchive(zipStream, ZipArchiveMode.Create)) {
                string entryName = Path.GetFileName(path);
                if (!string.IsNullOrWhiteSpace(relativeTodir)) {
                    entryName = path.MakeRelativePath(relativeTodir).Replace('\\', '/');
                }
                archive.CreateEntryFromFile(path, entryName);
            }
            return zipFilePath;
        }

        public string CompressFiles(IEnumerable<string> paths, string relativeTodir, IProgress<string> progress, CancellationToken ct) {
            string zipFilePath = Path.GetTempFileName();
            using (FileStream zipStream = new FileStream(zipFilePath, FileMode.Create))
            using (ZipArchive archive = new ZipArchive(zipStream, ZipArchiveMode.Create)) {
                foreach (string path in paths) {
                    if (ct.IsCancellationRequested) {
                        break;
                    }

                    string entryName = null;
                    if (!string.IsNullOrWhiteSpace(relativeTodir)) {
                        entryName = path.MakeRelativePath(relativeTodir).Replace('\\', '/');
                    } else {
                        entryName = path.MakeRelativePath(Path.GetDirectoryName(path)).Replace('\\', '/');
                    }
                    progress?.Report(path);
                    archive.CreateEntryFromFile(path, entryName);
                }
            }
            return zipFilePath;
        }

        public string CompressDirectory(string path) {
            Matcher matcher = new Matcher(StringComparison.OrdinalIgnoreCase);
            matcher.AddInclude("*.*");
            return CompressDirectory(path, matcher, new Progress<string>((p) => { }), CancellationToken.None);
        }

        public string CompressDirectory(string path, Matcher matcher, IProgress<string> progress, CancellationToken ct) {
            string zipFilePath = Path.GetTempFileName();
            using (FileStream zipStream = new FileStream(zipFilePath, FileMode.Create))
            using (ZipArchive archive = new ZipArchive(zipStream, ZipArchiveMode.Create)) {
                Queue<string> dirs = new Queue<string>();
                dirs.Enqueue(path);
                while (dirs.Count > 0) {
                    var dir = dirs.Dequeue();
                    var subdirs = Directory.GetDirectories(dir);
                    foreach (var subdir in subdirs) {
                        dirs.Enqueue(subdir);
                    }

                    var files = matcher.GetResultsInFullPath(dir);
                    foreach (var file in files) {
                        if (ct.IsCancellationRequested) {
                            return string.Empty;
                        }
                        progress?.Report(file);
                        string entryName = file.MakeRelativePath(dir).Replace('\\', '/');
                        archive.CreateEntryFromFile(file, entryName);
                    }
                }
            }
            return zipFilePath;
        }

        public string GetDownloadsPath(string fileName) {
            return Path.Combine("~/Downloads", fileName);
        }
    }
}
