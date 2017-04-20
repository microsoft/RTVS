// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
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
        [InlineData("", "", "")]
        public void MakeRelativePath(string path, string basePath, string expected) {
            path.MakeRelativePath(basePath).Should().Be(expected);
        }

        [CompositeTest]
        [InlineData(@"C:\foo", @"C:\", "foo")]
        [InlineData(@"C:\foo", @"C:\foo", ".")]
        [InlineData(@"C:\foo", @"C:\foo\", ".")]
        [InlineData(@"C:\foo\", @"C:\foo", ".")]
        [InlineData(@"C:\", @"c:\foo", "..")]
        [InlineData(@"C:\foo", @"c:\bar", @"..\foo")]
        [InlineData(@"C:\abc\def.x", @"c:\abc\", @"def.x")]
        [InlineData(@"C:\abc\def.x\", @"c:\abc\", @"def.x")]
        [InlineData(@"c:\abc\def", @"c:\a\b", @"..\..\abc\def")]
        public void MakeCompleteRelativePath01(string path, string basePath, string expected) {
            path.MakeCompleteRelativePath(basePath).Should().Be(expected);
        }
        [CompositeTest]
        [InlineData("", "")]
        [InlineData(".", "")]
        [InlineData(".", ".")]
        [InlineData(@".\", ".")]
        [InlineData(".", @".\")]
        [InlineData(@".\", @".\")]
        [InlineData("", @"C:\\")]
        [InlineData(@"C:\", "")]
        public void MakeCompleteRelativePath02(string path, string basePath) {
            Action a = () => path.MakeCompleteRelativePath(basePath);
            a.ShouldThrow<ArgumentException>();
        }
    }
}
