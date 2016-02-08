using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Microsoft.Common.Core.IO;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Microsoft.Common.Core.Test.IO.SubstituteFactories
{
    [ExcludeFromCodeCoverage]
    public static class FileInfoFactory
    {
        public static IFileInfo Create(IFileSystem fileSystem, string path)
        {
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

        public static IFileInfo Delete(IFileSystem fileSystem, string path)
        {
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