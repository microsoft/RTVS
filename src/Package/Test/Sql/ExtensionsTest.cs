// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.IO;
using FluentAssertions;
using Microsoft.Common.Core.IO;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.R.Package.Sql;
using NSubstitute;
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

        [CompositeTest]
        [InlineData("", "[]", SqlQuoteType.Bracket)]
        [InlineData("abc", "[abc]", SqlQuoteType.Bracket)]
        [InlineData("abc", "\"abc\"", SqlQuoteType.Quote)]
        [InlineData("a bc", "[a bc]", SqlQuoteType.Bracket)]
        [InlineData("a bc", "[a bc]", SqlQuoteType.None)]
        [InlineData("abc", "abc", SqlQuoteType.None)]
        [InlineData("[[a", "[[[a]", SqlQuoteType.Bracket)]
        [InlineData("[[a", "\"[[a\"", SqlQuoteType.Quote)]
        [InlineData("[[a", "[[a", SqlQuoteType.None)]
        public void ToSqlName(string name, string expected, SqlQuoteType quoteType) {
            var actual = name.ToSqlName(quoteType);
            actual.Should().Be(expected);
        }

        [CompositeTest]
        [InlineData("", "")]
        [InlineData("abc", "")]
        [InlineData("create proceDure a", "a")]
        [InlineData("CREATE PROCEDURE [b]", "b")]
        [InlineData("CREATE   PROCEDURE   [a b]", "a b")]
        [InlineData("CREATE\tPROCEDURE\t\"a b\"", "a b")]
        [InlineData("CREATE PROCEDURE [b\n", "b")]
        public void GetSProcNameFromTemplate(string content, string expected) {
            var fs = Substitute.For<IFileSystem>();
            fs.FileExists(null).ReturnsForAnyArgs(true);
            fs.ReadAllText(null).ReturnsForAnyArgs(content);

            var actual = fs.GetSProcNameFromTemplate(@"C:\foo.r");
            actual.Should().Be(expected);
        }
    }
}
