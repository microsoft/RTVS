// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;

namespace Microsoft.Common.Core.Extensions {
    /// <summary>Provides extension methods for the <see cref="T:System.IO.Compression.ZipArchive" /> and <see cref="T:System.IO.Compression.ZipArchiveEntry" /> classes.</summary>
    /// Copied from System.ComponentModel.Composition.FileSystem.dll
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class ZipFileExtensions {
        /// <summary>Archives a file by compressing it and adding it to the zip archive.</summary>
        /// <returns>A wrapper for the new entry in the zip archive.</returns>
        /// <param name="destination">The zip archive to add the file to.</param>
        /// <param name="sourceFileName">The path to the file to be archived. You can specify either a relative or an absolute path. A relative path is interpreted as relative to the current working directory.</param>
        /// <param name="entryName">The name of the entry to create in the zip archive.</param>
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="sourceFileName" /> is <see cref="F:System.String.Empty" />, contains only white space, or contains at least one invalid character.-or-<paramref name="entryName" /> is <see cref="F:System.String.Empty" />.</exception>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="sourceFileName" /> or <paramref name="entryName" /> is null.</exception>
        /// <exception cref="T:System.IO.PathTooLongException">In <paramref name="sourceFileName" />, the specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must not exceed 248 characters, and file names must not exceed 260 characters.</exception>
        /// <exception cref="T:System.IO.DirectoryNotFoundException">
        /// <paramref name="sourceFileName" /> is invalid (for example, it is on an unmapped drive).</exception>
        /// <exception cref="T:System.IO.IOException">The file specified by <paramref name="sourceFileName" /> cannot be opened.</exception>
        /// <exception cref="T:System.UnauthorizedAccessException">
        /// <paramref name="sourceFileName" /> specifies a directory.-or-The caller does not have the required permission to access the file specified by <paramref name="sourceFileName" />.</exception>
        /// <exception cref="T:System.IO.FileNotFoundException">The file specified by <paramref name="sourceFileName" /> is not found.</exception>
        /// <exception cref="T:System.NotSupportedException">The <paramref name="sourceFileName" /> parameter is in an invalid format.-or-The zip archive does not support writing.</exception>
        /// <exception cref="T:System.ObjectDisposedException">The zip archive has been disposed.</exception>
        public static ZipArchiveEntry CreateEntryFromFile(this ZipArchive destination, string sourceFileName, string entryName) {
            return DoCreateEntryFromFile(destination, sourceFileName, entryName, new CompressionLevel?());
        }

        /// <summary>Archives a file by compressing it using the specified compression level and adding it to the zip archive.</summary>
        /// <returns>A wrapper for the new entry in the zip archive.</returns>
        /// <param name="destination">The zip archive to add the file to.</param>
        /// <param name="sourceFileName">The path to the file to be archived. You can specify either a relative or an absolute path. A relative path is interpreted as relative to the current working directory.</param>
        /// <param name="entryName">The name of the entry to create in the zip archive.</param>
        /// <param name="compressionLevel">One of the enumeration values that indicates whether to emphasize speed or compression effectiveness when creating the entry.</param>
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="sourceFileName" /> is <see cref="F:System.String.Empty" />, contains only white space, or contains at least one invalid character.-or-<paramref name="entryName" /> is <see cref="F:System.String.Empty" />.</exception>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="sourceFileName" /> or <paramref name="entryName" /> is null.</exception>
        /// <exception cref="T:System.IO.DirectoryNotFoundException">
        /// <paramref name="sourceFileName" /> is invalid (for example, it is on an unmapped drive).</exception>
        /// <exception cref="T:System.IO.PathTooLongException">In <paramref name="sourceFileName" />, the specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must not exceed 248 characters, and file names must not exceed 260 characters.</exception>
        /// <exception cref="T:System.IO.IOException">The file specified by <paramref name="sourceFileName" /> cannot be opened.</exception>
        /// <exception cref="T:System.UnauthorizedAccessException">
        /// <paramref name="sourceFileName" /> specifies a directory.-or-The caller does not have the required permission to access the file specified by <paramref name="sourceFileName" />.</exception>
        /// <exception cref="T:System.IO.FileNotFoundException">The file specified by <paramref name="sourceFileName" /> is not found.</exception>
        /// <exception cref="T:System.NotSupportedException">The <paramref name="sourceFileName" /> parameter is in an invalid format.-or-The zip archive does not support writing.</exception>
        /// <exception cref="T:System.ObjectDisposedException">The zip archive has been disposed.</exception>
        public static ZipArchiveEntry CreateEntryFromFile(this ZipArchive destination, string sourceFileName, string entryName, CompressionLevel compressionLevel) {
            return DoCreateEntryFromFile(destination, sourceFileName, entryName, new CompressionLevel?(compressionLevel));
        }

        internal static ZipArchiveEntry DoCreateEntryFromFile(ZipArchive destination, string sourceFileName, string entryName, CompressionLevel? compressionLevel) {
            if (destination == null)
                throw new ArgumentNullException("destination");
            if (sourceFileName == null)
                throw new ArgumentNullException("sourceFileName");
            if (entryName == null)
                throw new ArgumentNullException("entryName");
            using (Stream stream = (Stream)File.Open(sourceFileName, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                ZipArchiveEntry zipArchiveEntry = compressionLevel.HasValue ? destination.CreateEntry(entryName, compressionLevel.Value) : destination.CreateEntry(entryName);
                DateTime dateTime = File.GetLastWriteTime(sourceFileName);
                if (dateTime.Year < 1980 || dateTime.Year > 2107)
                    dateTime = new DateTime(1980, 1, 1, 0, 0, 0);
                zipArchiveEntry.LastWriteTime = (DateTimeOffset)dateTime;
                using (Stream destination1 = zipArchiveEntry.Open())
                    stream.CopyTo(destination1);
                return zipArchiveEntry;
            }
        }

        /// <summary>Extracts an entry in the zip archive to a file.</summary>
        /// <param name="source">The zip archive entry to extract a file from.</param>
        /// <param name="destinationFileName">The path of the file to create from the contents of the entry. You can  specify either a relative or an absolute path. A relative path is interpreted as relative to the current working directory.</param>
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="destinationFileName" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />.-or-<paramref name="destinationFileName" /> specifies a directory.</exception>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="destinationFileName" /> is null. </exception>
        /// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must not exceed 248 characters, and file names must not exceed 260 characters. </exception>
        /// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid (for example, it is on an unmapped drive). </exception>
        /// <exception cref="T:System.IO.IOException">
        /// <paramref name="destinationFileName" /> already exists.-or- An I/O error occurred.-or-The entry is currently open for writing.-or-The entry has been deleted from the archive.</exception>
        /// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission to create the new file.</exception>
        /// <exception cref="T:System.IO.InvalidDataException">The entry is missing from the archive, or is corrupt and cannot be read.-or-The entry has been compressed by using a compression method that is not supported.</exception>
        /// <exception cref="T:System.ObjectDisposedException">The zip archive that this entry belongs to has been disposed.</exception>
        /// <exception cref="T:System.NotSupportedException">
        /// <paramref name="destinationFileName" /> is in an invalid format. -or-The zip archive for this entry was opened in <see cref="F:System.IO.Compression.ZipArchiveMode.Create" /> mode, which does not permit the retrieval of entries.</exception>
        public static void ExtractToFile(this ZipArchiveEntry source, string destinationFileName) {
            source.ExtractToFile(destinationFileName, false);
        }

        /// <summary>Extracts an entry in the zip archive to a file, and optionally overwrites an existing file that has the same name.</summary>
        /// <param name="source">The zip archive entry to extract a file from.</param>
        /// <param name="destinationFileName">The path of the file to create from the contents of the entry. You can specify either a relative or an absolute path. A relative path is interpreted as relative to the current working directory.</param>
        /// <param name="overwrite">true to overwrite an existing file that has the same name as the destination file; otherwise, false.</param>
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="destinationFileName" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />.-or-<paramref name="destinationFileName" /> specifies a directory.</exception>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="destinationFileName" /> is null. </exception>
        /// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must not exceed 248 characters, and file names must not exceed 260 characters. </exception>
        /// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid (for example, it is on an unmapped drive). </exception>
        /// <exception cref="T:System.IO.IOException">
        /// <paramref name="destinationFileName" /> already exists and <paramref name="overwrite" /> is false.-or- An I/O error occurred.-or-The entry is currently open for writing.-or-The entry has been deleted from the archive.</exception>
        /// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission to create the new file.</exception>
        /// <exception cref="T:System.IO.InvalidDataException">The entry is missing from the archive or is corrupt and cannot be read.-or-The entry has been compressed by using a compression method that is not supported.</exception>
        /// <exception cref="T:System.ObjectDisposedException">The zip archive that this entry belongs to has been disposed.</exception>
        /// <exception cref="T:System.NotSupportedException">
        /// <paramref name="destinationFileName" /> is in an invalid format. -or-The zip archive for this entry was opened in <see cref="F:System.IO.Compression.ZipArchiveMode.Create" /> mode, which does not permit the retrieval of entries.</exception>
        public static void ExtractToFile(this ZipArchiveEntry source, string destinationFileName, bool overwrite) {
            if (source == null)
                throw new ArgumentNullException("source");
            if (destinationFileName == null)
                throw new ArgumentNullException("destinationFileName");
            FileMode mode = overwrite ? FileMode.Create : FileMode.CreateNew;
            using (Stream destination = (Stream)File.Open(destinationFileName, mode, FileAccess.Write, FileShare.None)) {
                using (Stream stream = source.Open())
                    stream.CopyTo(destination);
            }
            File.SetLastWriteTime(destinationFileName, source.LastWriteTime.DateTime);
        }
    }
}
