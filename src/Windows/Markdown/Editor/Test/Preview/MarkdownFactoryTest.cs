// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.Markdown.Editor.Preview.Parser;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.Text;
using NSubstitute;
using Xunit;

namespace Microsoft.Markdown.Editor.Test.Preview {
    [ExcludeFromCodeCoverage]
    [Category.Md.Preview]
    public sealed class MarkdownFactoryTest {
        [CompositeTest]
        [InlineData("")]
        [InlineData("*text*")]
        [InlineData("```\ncode\n```")]
        [InlineData("```{r}\nx <- 1\n```")]
        public void ParseTest(string markdown) {
            var snapshot = Substitute.For<ITextSnapshot>();
            snapshot.GetText().Returns(markdown);
            var doc = snapshot.ParseToMarkdown();
            doc.Should().NotBeNull();

            // Verify caching
            snapshot.ClearReceivedCalls();
            doc = snapshot.ParseToMarkdown();
            doc.Should().NotBeNull();
            snapshot.DidNotReceive().GetText();
        }
    }
}
