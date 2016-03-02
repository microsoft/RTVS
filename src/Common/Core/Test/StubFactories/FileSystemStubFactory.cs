// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Common.Core.IO;
using NSubstitute;

namespace Microsoft.Common.Core.Test.StubBuilders {
    public class FileSystemStubFactory {
        public static IFileSystem CreateDefault() {
            return Substitute.For<IFileSystem>();
        }
    }
}
