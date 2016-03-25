// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.Languages.Core.Text;
using Microsoft.R.Components.ContentTypes;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.Parser;
using Microsoft.R.Editor.Completion;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.Editor.Mocks;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;

namespace Microsoft.R.Editor.Test.Completions {
    [ExcludeFromCodeCoverage]
    [Category.R.Completion]
    public class RCompletionSourceTest {
        [Test]
        public void BaseFunctions01() {
            List<CompletionSet> completionSets = new List<CompletionSet>();
            GetCompletions("", 0, completionSets);

            completionSets.Should().ContainSingle()
                .Which.Completions.Should().Contain(c => c.DisplayText == "abbreviate")
                    .And.Contain(c => c.DisplayText == "abs");
        }

        [Test]
        public void BaseFunctions02() {
            List<CompletionSet> completionSets = new List<CompletionSet>();
            GetCompletions("f", 1, completionSets, new TextRange(0, 1));

            completionSets.Should().ContainSingle();
            completionSets[0].Filter();

            completionSets[0].Completions[0].DisplayText.Should().Be("factanal");
            completionSets[0].Completions[1].Description.Should().Be("Factors");
        }

        [Test]
        public void Keywords01() {
            List<CompletionSet> completionSets = new List<CompletionSet>();
            GetCompletions("f", 1, completionSets, new TextRange(0, 1));

            completionSets.Should().ContainSingle();
            completionSets[0].Filter();

            completionSets[0].Completions.Should().Contain(c => c.DisplayText == "for");
        }

        [Test]
        public void Packages01() {
            List<CompletionSet> completionSets = new List<CompletionSet>();
            GetCompletions("library(", 8, completionSets);

            completionSets.Should().ContainSingle();

            completionSets[0].Completions.Should().Contain(c => c.DisplayText == "base")
                .Which.Description.Should().Be("Base R functions.");
        }

        [Test]
        public void SpecificPackage01() {
            List<CompletionSet> completionSets = new List<CompletionSet>();
            GetCompletions("utils::", 7, completionSets);

            completionSets.Should().ContainSingle();

            completionSets[0].Completions.Should().Contain(c => c.DisplayText == "adist")
                .Which.Description.Should().Be("Approximate String Distances");
        }

        [Test]
        public void Comments01() {
            List<CompletionSet> completionSets = new List<CompletionSet>();
            GetCompletions("#No", 3, completionSets);

            completionSets.Should().ContainSingle()
                .Which.Completions.Should().BeEmpty();
        }

        [Test]
        public void Comments02() {
            List<CompletionSet> completionSets = new List<CompletionSet>();
            GetCompletions("#No", 0, completionSets);

            completionSets.Should().ContainSingle()
                .Which.Completions.Should().NotBeEmpty();
        }

        [Test]
        public void FunctionDefinition01() {
            List<CompletionSet> completionSets = new List<CompletionSet>();
            GetCompletions("x <- function()", 14, completionSets);

            completionSets.Should().ContainSingle()
                .Which.Completions.Should().BeEmpty();
        }

        [Test]
        public void FunctionDefinition02() {
            for (int i = 14; i <= 18; i++) {
                List<CompletionSet> completionSets = new List<CompletionSet>();
                GetCompletions("x <- function(a, b)", i, completionSets);

                completionSets.Should().ContainSingle()
                    .Which.Completions.Should().BeEmpty();
            }
        }

        [Test]
        public void FunctionDefinition03() {
            for (int i = 14; i <= 19; i++) {
                List<CompletionSet> completionSets = new List<CompletionSet>();
                GetCompletions("x <- function(a, b = x+y)", i, completionSets);

                completionSets.Should().ContainSingle()
                    .Which.Completions.Should().BeEmpty();
            }

            for (int i = 20; i <= 24; i++) {
                List<CompletionSet> completionSets = new List<CompletionSet>();
                GetCompletions("x <- function(a, b = x+y)", i, completionSets);

                completionSets.Should().NotBeEmpty();
                completionSets[0].Completions.Should().NotBeEmpty();
            }
        }

        [Test]
        public void CaseSentivity() {
            List<CompletionSet> completionSets = new List<CompletionSet>();
            GetCompletions("x <- T", 6, completionSets);

            completionSets.Should().ContainSingle();
            completionSets[0].Filter();

            completionSets[0].Completions.Should().NotBeEmpty()
                .And.OnlyContain(c => c.DisplayText[0] == 'T');
        }

        [Test]
        public void UserVariables01() {
            List<CompletionSet> completionSets = new List<CompletionSet>();
            var content =
@"
aaa123 <- 1

bbb123 <- 1

";
            GetCompletions(content, 0, completionSets);

            completionSets.Should().ContainSingle();
            completionSets[0].Filter();

            completionSets[0].Completions.Should().NotBeEmpty()
                .And.NotContain(c => c.DisplayText == "aaa123")
                .And.NotContain(c => c.DisplayText == "bbb123");

            completionSets.Clear();
            GetCompletions(content, 15, completionSets);

            completionSets.Should().ContainSingle();
            completionSets[0].Filter();

            completionSets[0].Completions.Should().NotBeEmpty()
                .And.Contain(c => c.DisplayText == "aaa123")
                .And.NotContain(c => c.DisplayText == "bbb123");

            completionSets.Clear();
            GetCompletions(content, 30, completionSets);

            completionSets.Should().ContainSingle();
            completionSets[0].Filter();

            completionSets[0].Completions.Should().NotBeEmpty()
                .And.Contain(c => c.DisplayText == "aaa123")
                .And.Contain(c => c.DisplayText == "bbb123");
        }

        [Test]
        public void UserFunctions01() {
            List<CompletionSet> completionSets = new List<CompletionSet>();
            GetCompletions("aaaa <- function(a,b,c)\r\na", 25, completionSets);

            completionSets.Should().ContainSingle();
            completionSets[0].Filter();

            completionSets[0].Completions.Should().NotBeEmpty()
                .And.Contain(c => c.DisplayText == "aaaa");
        }

        [Test]
        public void UserFunctions02() {
            List<CompletionSet> completionSets = new List<CompletionSet>();
            var content =
@"
aaa123 <- function(a,b,c) { }
while(TRUE) {
aaa456 <- function() { }
#
aa
}";
            GetCompletions(content, content.IndexOf('#') + 4, completionSets);

            completionSets.Should().ContainSingle();
            completionSets[0].Filter();

            var completions = completionSets[0].Completions;
            completions.Should().NotBeEmpty();
            completions.Should().Contain(c => c.DisplayText == "aaa123");
            completions.Should().Contain(c => c.DisplayText == "aaa456");
        }

        [Test]
        public void UserFunctions03() {
            List<CompletionSet> completionSets = new List<CompletionSet>();
            var content =
@"
aaa123 <- function(a,b,c) { }
while(TRUE) {
$
aa
aaa456 <- function() { }
#
aa
}
aaa789 <- function(a,b,c) { }
";
            GetCompletions(content, content.IndexOf('$') + 4, completionSets);

            completionSets.Should().ContainSingle();
            completionSets[0].Filter();

            var completions = completionSets[0].Completions;
            completions.Should().NotBeEmpty();
            completions.Should().Contain(c => c.DisplayText == "aaa123");
            completions.Should().NotContain(c => c.DisplayText == "aaa456");
            completions.Should().Contain(c => c.DisplayText == "aaa789");

            completionSets.Clear();
            GetCompletions(content, content.IndexOf('#') + 4, completionSets);

            completionSets.Should().ContainSingle();
            completionSets[0].Filter();

            completions = completionSets[0].Completions;
            completions.Should().NotBeEmpty();
            completions.Should().Contain(c => c.DisplayText == "aaa123");
            completions.Should().Contain(c => c.DisplayText == "aaa456");
            completions.Should().Contain(c => c.DisplayText == "aaa789");
        }

        [Test]
        public void UserFunctionArguments01() {
            List<CompletionSet> completionSets = new List<CompletionSet>();
            GetCompletions("aaa <- function(a,b,c)\r\naaa(a ", 29, completionSets);

            completionSets.Should().ContainSingle();
            completionSets[0].Filter();

            completionSets[0].Completions.Should().NotBeEmpty()
                .And.Contain(c => c.DisplayText == "a =");
        }

        private void GetCompletions(string content, int position, IList<CompletionSet> completionSets, ITextRange selectedRange = null) {
            AstRoot ast = RParser.Parse(content);

            TextBufferMock textBuffer = new TextBufferMock(content, RContentTypeDefinition.ContentType);
            TextViewMock textView = new TextViewMock(textBuffer, position);

            if (selectedRange != null) {
                textView.Selection.Select(new SnapshotSpan(textBuffer.CurrentSnapshot, selectedRange.Start, selectedRange.Length), false);
            }

            CompletionSessionMock completionSession = new CompletionSessionMock(textView, completionSets, position);
            RCompletionSource completionSource = new RCompletionSource(textBuffer);

            completionSource.PopulateCompletionList(position, completionSession, completionSets, ast);
        }
    }
}
