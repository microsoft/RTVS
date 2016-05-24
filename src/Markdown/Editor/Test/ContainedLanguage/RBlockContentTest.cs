// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.Markdown.Editor.Utility;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;

namespace Microsoft.Markdown.Editor.Test.Classification {
    [ExcludeFromCodeCoverage]
    public class RBlockContentTest {
        [CompositeTest]
        [Category.Md.RCode]
        [InlineData("{r}", "")]
        [InlineData("{r}\n\n", "")]
        [InlineData("{r x=1}\nplot()", "plot()")]
        [InlineData("{r x=1,\ny=2}\nx <- 1\n", "x <- 1")]
        [InlineData("{r x=function() {\n}}\nx <- 1\n", "x <- 1")]
        [InlineData("{r}\nparams$a = 3\nx <- 1\n", "x <- 1")]
        [InlineData("{r}\nparams$a = 3\n{r}\n", "{r}")]
        [InlineData("{r}\n{r}\n", "{r}")]
        public void RCode(string markdown, string rCode) {
            MarkdownUtility.GetRContentFromMarkdownCodeBlock(markdown).TrimEnd().Should().Be(rCode);
        }
    }
}
