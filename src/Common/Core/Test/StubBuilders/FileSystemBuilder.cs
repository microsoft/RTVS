using Microsoft.Common.Core.IO;
using NSubstitute;

namespace Microsoft.Common.Core.Test.StubBuilders {
    public class FileSystemBuilder {
        public static IFileSystem CreateDefault() {
            return Substitute.For<IFileSystem>();
        }
    }
}
