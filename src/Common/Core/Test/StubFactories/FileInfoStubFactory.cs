// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.IO;
using Microsoft.Common.Core.IO;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Microsoft.Common.Core.Test.StubFactories {
    [ExcludeFromCodeCoverage]
    public static class FileInfoStubFactory {
        public static IFileInfo Create(IFileSystem fileSystem, string path) {
            var fi = Substitute.For<IFileInfo>();
            fi.FullName.Returns(path);
            fi.Exists.Returns(true);
            fi.Directory.Returns((IDirectoryInfo)null);
            fileSystem.FileExists(path).Returns(true);

            try {
                fileSystem.GetFileAttributes(path);
            } catch (IOException) {
                default(FileAttributes).Returns(FileAttributes.Normal);
            }

            return fi;
        }

        public static IFileInfo Delete(IFileSystem fileSystem, string path) {
            var fi = Substitute.For<IFileInfo>();
            fi.FullName.Returns(path);
            fi.Exists.Returns(false);
            fi.Directory.Returns((IDirectoryInfo)null);
            fileSystem.FileExists(path).Returns(false);
            try {
                fileSystem.GetFileAttributes(path).Throws<FileNotFoundException>();
            } catch (IOException) { }
            return fi;
        }
    }
}