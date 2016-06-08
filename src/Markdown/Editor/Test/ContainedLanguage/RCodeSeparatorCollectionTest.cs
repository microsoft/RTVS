// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Linq;
using FluentAssertions;
using Microsoft.Languages.Core.Text;
using Microsoft.Markdown.Editor.ContainedLanguage;
using Microsoft.Markdown.Editor.Tokens;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;

namespace Microsoft.Markdown.Editor.Test.Classification {
    [ExcludeFromCodeCoverage]
    public class RCodeSeparatorCollectionTest {
        [CompositeTest]
        [Category.Md.RCode]
        [InlineData("", 0)]
        [InlineData("```", 0)]
        [InlineData("a```", 0)]
        [InlineData("```{r}", 1)]
        [InlineData("a\n```{r}\n", 1)]
        [InlineData("a\n```{r}\n", 1)]
        [InlineData("a\n```{r}\n```\n```{r}\n", 2)]
        public void BuildTest(string content, int expectedTokens) {
            var tokenizer = new MdTokenizer();
            var tokens = tokenizer.Tokenize(content);
            var rCodeTokens = tokens.Where(t => t.TokenType == MarkdownTokenType.Code);
            rCodeTokens.Should().HaveCount(expectedTokens);
        }

        [CompositeTest]
        [Category.Md.RCode]
        [InlineData(0, 0, "a", true)]
        [InlineData(0, 0, "`", true)]
        [InlineData(1, 1, "a", true)]
        [InlineData(0, 1, "", true)]
        [InlineData(1, 2, "", true)]
        [InlineData(3, 1, "", true)]
        [InlineData(4, 1, "", true)]
        [InlineData(5, 1, "", false)]
        [InlineData(6, 0, "a", false)]
        [InlineData(6, 1, "", false)]
        [InlineData(11, 0, "a", false)]
        [InlineData(12, 0, " ", true)]
        [InlineData(12, 1, "a", true)]
        [InlineData(15, 0, "a", true)]
        [InlineData(15, 1, "", true)]
        [InlineData(16, 0, "a", false)]
        [InlineData(16, 1, "", false)]
        public void DestructiveTest01(int start, int oldLength, string newText, bool expected) {
            var markdown = "```{r}\nx<-1\n```\na";
            var coll = BuildCollection(markdown);
            var newCode = markdown.Remove(start, oldLength).Insert(start, newText);
            var result = coll.IsDestructiveChange(start, oldLength, newText.Length, new TextStream(markdown), new TextStream(newCode));
            result.Should().Be(expected);
        }

        [CompositeTest]
        [Category.Md.RCode]
        [InlineData(13, 4, "a", true)]
        [InlineData(15, 0, "`", true)]
        [InlineData(15, 0, "``", true)]
        [InlineData(16, 0, "`", true)]
        [InlineData(16, 0, "`'", true)]
        [InlineData(16, 0, "```", true)]
        [InlineData(16, 1, "```{r}", true)]
        public void DestructiveTest02(int start, int oldLength, string newText, bool expected) {
            var markdown = "```{r}\nx<-1\n```\n\n```{r}\ny<-2\n```";
            var coll = BuildCollection(markdown);
            var newCode = markdown.Remove(start, oldLength).Insert(start, newText);
            var result = coll.IsDestructiveChange(start, oldLength, newText.Length, new TextStream(markdown), new TextStream(newCode));
            result.Should().Be(expected);
        }

        [CompositeTest]
        [Category.Md.RCode]
        [InlineData("abc```{r}\n\n```", 0, 3, "", true)]
        [InlineData("```{r}\n\n```", 0, 0, "a", true)]
        [InlineData("```{r}\n\n```", 8, 0, "a", true)]
        [InlineData("```{r}\n\na```", 8, 1, "", true)]
        [InlineData("```", 2, 1, "", true)]
        public void DestructiveTest03(string content, int start, int oldLength, string newText, bool expected) {
            var coll = BuildCollection(content);
            var newCode = content.Remove(start, oldLength).Insert(start, newText);
            var result = coll.IsDestructiveChange(start, oldLength, newText.Length, new TextStream(content), new TextStream(newCode));
            result.Should().Be(expected);
        }

        private RCodeSeparatorCollection BuildCollection(string markdown) {
            var tokenizer = new MdTokenizer();
            var tokens = tokenizer.Tokenize(markdown);
            var rCodeTokens = tokens.Where(t => t.TokenType == MarkdownTokenType.Code);

            var coll = new RCodeSeparatorCollection();
            foreach (var t in rCodeTokens) {
                coll.Add(new TextRange(t.Start, 5));
                coll.Add(new TextRange(t.End - 3, 3));
            }
            return coll;
        }
    }
}
