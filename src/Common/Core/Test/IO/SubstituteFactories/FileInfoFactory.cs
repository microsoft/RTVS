using Microsoft.Common.Core.IO;
using NSubstitute;

namespace Microsoft.Common.Core.Tests.IO.SubstituteFactories
{
    public static class FileInfoFactory
    {
        public static IFileInfo Create(IFileSystem fileSystem, string path)
        {
            var fi = Substitute.For<IFileInfo>();
            fi.FullName.Returns(path);
            fi.Exists.Returns(true);
            fi.Directory.Returns((IDirectoryInfo)null);
            fileSystem.FileExists(path).Returns(true);
            return fi;
        }

        public static IFileInfo Delete(IFileSystem fileSystem, string path)
        {
            var fi = Substitute.For<IFileInfo>();
            fi.FullName.Returns(path);
            fi.Exists.Returns(false);
            fi.Directory.Returns((IDirectoryInfo)null);
            fileSystem.FileExists(path).Returns(false);
            return fi;
        }
    }
}