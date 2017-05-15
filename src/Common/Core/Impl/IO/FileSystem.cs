// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Diagnostics;
using System.Threading;
using Microsoft.Extensions.FileSystemGlobbing;

namespace Microsoft.Common.Core.IO {
    public class FileSystem : IFileSystem {
        public virtual IFileSystemWatcher CreateFileSystemWatcher(string path, string filter) => new FileSystemWatcherProxy(path, filter);

        public virtual IDirectoryInfo GetDirectoryInfo(string directoryPath) => new DirectoryInfoProxy(directoryPath);

        public virtual bool FileExists(string path) => File.Exists(path);

        public virtual long FileSize(string path) {
            var fileInfo = new FileInfo(path);
            return fileInfo.Length;
        }

        public virtual string ReadAllText(string path) => File.ReadAllText(path);

        public virtual void WriteAllText(string path, string content) => File.WriteAllText(path, content);

        public virtual IEnumerable<string> FileReadAllLines(string path) => File.ReadLines(path);

        public virtual void FileWriteAllLines(string path, IEnumerable<string> contents) => File.WriteAllLines(path, contents);

        public virtual byte[] FileReadAllBytes(string path) => File.ReadAllBytes(path);

        public virtual void FileWriteAllBytes(string path, byte[] bytes) => File.WriteAllBytes(path, bytes);

        public virtual Stream CreateFile(string path) => File.Create(path);
        public virtual Stream FileOpen(string path, FileMode mode) => File.Open(path, mode);

        public virtual bool DirectoryExists(string path) => Directory.Exists(path);

        public virtual FileAttributes GetFileAttributes(string path) => File.GetAttributes(path);

        public virtual string ToLongPath(string path) => path;

        public virtual string ToShortPath(string path) => path;

        public virtual Version GetFileVersion(string path) {
            var fvi = FileVersionInfo.GetVersionInfo(path);
            return new Version(fvi.FileMajorPart, fvi.FileMinorPart, fvi.FileBuildPart, fvi.FilePrivatePart);
        }

        public virtual void DeleteFile(string path) => File.Delete(path);

        public virtual void DeleteDirectory(string path, bool recursive) => Directory.Delete(path, recursive);

        public virtual string[] GetFileSystemEntries(string path, string searchPattern, SearchOption options) => Directory.GetFileSystemEntries(path, searchPattern, options);

        public virtual void CreateDirectory(string path) => Directory.CreateDirectory(path);

        public virtual string[] GetFiles(string path) => Directory.GetFiles(path);
        public virtual string[] GetFiles(string path, string pattern) => Directory.GetFiles(path, pattern);
        public virtual string[] GetFiles(string path, string pattern, SearchOption option) => Directory.GetFiles(path, pattern, option);
        public virtual string[] GetDirectories(string path) => Directory.GetDirectories(path);

        public virtual string CompressFile(string path, string relativeTodir) {
            var zipFilePath = Path.GetTempFileName();
            using (var zipStream = new FileStream(zipFilePath, FileMode.Create))
            using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create)) {
                var entryName = Path.GetFileName(path);
                if (!string.IsNullOrWhiteSpace(relativeTodir)) {
                    entryName = path.MakeRelativePath(relativeTodir).Replace('\\', '/');
                }
                archive.CreateEntryFromFile(path, entryName);
            }
            return zipFilePath;
        }

        public virtual string CompressFiles(IEnumerable<string> paths, string relativeTodir, IProgress<string> progress, CancellationToken ct) {
            var zipFilePath = Path.GetTempFileName();
            using (var zipStream = new FileStream(zipFilePath, FileMode.Create))
            using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create)) {
                foreach (var path in paths) {
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

        public virtual string CompressDirectory(string path) {
            var matcher = new Matcher(StringComparison.OrdinalIgnoreCase);
            matcher.AddInclude("*.*");
            return CompressDirectory(path, matcher, new Progress<string>((p) => { }), CancellationToken.None);
        }

        public virtual string CompressDirectory(string path, Matcher matcher, IProgress<string> progress, CancellationToken ct) {
            var zipFilePath = Path.GetTempFileName();
            using (var zipStream = new FileStream(zipFilePath, FileMode.Create))
            using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create)) {
                var dirs = new Queue<string>();
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
                        var entryName = file.MakeRelativePath(dir).Replace('\\', '/');
                        archive.CreateEntryFromFile(file, entryName);
                    }
                }
            }
            return zipFilePath;
        }

        public virtual string GetDownloadsPath(string fileName) => throw new NotImplementedException();
    }
}
