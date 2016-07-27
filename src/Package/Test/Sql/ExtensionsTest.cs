// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.IO;
using FluentAssertions;
using Microsoft.Common.Core.IO;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.R.Package.Sql;
using Microsoft.VisualStudio.R.Package.Sql.Publish;
using Xunit;

namespace Microsoft.VisualStudio.R.Package.Test.Sql {
    [ExcludeFromCodeCoverage]
    [Category.Sql]
    public class ExtensionsTest {
        [CompositeTest]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("a.r")]
        [InlineData("a")]
        [InlineData(@"C:\a.r")]
        public void ToSProcPath(string path) {
            var expected = !string.IsNullOrEmpty(path)
                    ? Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path) + SProcFileExtensions.SProcFileExtension)
                    : path;
            path.ToSProcFilePath().Should().Be(expected);
        }

        [CompositeTest]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("a.r")]
        [InlineData("a")]
        [InlineData(@"C:\a.r")]
        public void ToQueryPath(string path) {
            var expected = !string.IsNullOrEmpty(path)
                    ? Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path) + SProcFileExtensions.QueryFileExtension)
                    : path;
            path.ToQueryFilePath().Should().Be(expected);
        }
    }
}
