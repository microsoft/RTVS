// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;

namespace Microsoft.Common.Core.Test.Extensions {
    [ExcludeFromCodeCoverage]
    [Category.CoreExtensions]
    public class IOExtensionsTest {
        [CompositeTest]
        [InlineData("", "", "")]
        [InlineData("", @"C:\\", "")]
        [InlineData(@"C:\", "", @"C:\")]
        [InlineData(@"C:\foo", @"C:\", "foo")]
        [InlineData(@"C:\foo", @"C:\foo", "")]
        [InlineData(@"C:\foo", @"C:\foo\", "")]
        [InlineData(@"C:\foo\", @"C:\foo", "")]
        [InlineData(@"C:\abc\def.x", @"c:\abc", "def.x")]
        [InlineData(@"C:\abc\def.x", @"c:\abc\", "def.x")]
        [InlineData(@"C:\abc\def.x\", @"c:\abc\", @"def.x\")]
        [InlineData(@"c:\abc", @"c:\a", @"c:\abc")]
        public void MakeRelativePath(string path, string basePath, string expected) {
            path.MakeRelativePath(basePath).Should().Be(expected);
        }
    }
}
