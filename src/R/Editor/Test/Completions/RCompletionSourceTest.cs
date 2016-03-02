// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.Languages.Core.Text;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.Parser;
using Microsoft.R.Editor.Completion;
using Microsoft.R.Editor.ContentType;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.Editor.Mocks;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;

namespace Microsoft.R.Editor.Test.Completions {
    [ExcludeFromCodeCoverage]
    [Category.R.Completion]
    public class RCompletionSourceTest {
        [Test]
        public void RCompletionSource_BaseFunctionsTest01() {
            List<CompletionSet> completionSets = new List<CompletionSet>();
            GetCompletions("", 0, completionSets);

            completionSets.Should().ContainSingle()
                .Which.Completions.Should().Contain(c => c.DisplayText == "abbreviate")
                    .And.Contain(c => c.DisplayText == "abs");
        }

        [Test]
        public void RCompletionSource_BaseFunctionsTest02() {
            List<CompletionSet> completionSets = new List<CompletionSet>();
            GetCompletions("f", 1, completionSets, new TextRange(0, 1));

            completionSets.Should().ContainSingle();

            completionSets[0].Filter();

            completionSets[0].Completions[0].DisplayText.Should().Be("factanal");
            completionSets[0].Completions[1].Description.Should().Be("Factors");
        }

        [Test]
        public void RCompletionSource_KeywordsTest01() {
            List<CompletionSet> completionSets = new List<CompletionSet>();
            GetCompletions("f", 1, completionSets, new TextRange(0, 1));

            completionSets.Should().ContainSingle();

            completionSets[0].Filter();

            completionSets[0].Completions.Should().Contain(c => c.DisplayText == "for");
        }

        [Test]
        public void RCompletionSource_PackagesTest01() {
            List<CompletionSet> completionSets = new List<CompletionSet>();
            GetCompletions("library(", 8, completionSets);

            completionSets.Should().ContainSingle();

            completionSets[0].Completions.Should().Contain(c => c.DisplayText == "base")
                .Which.Description.Should().Be("Base R functions.");
        }

        [Test]
        public void RCompletionSource_SpecificPackageTest01() {
            List<CompletionSet> completionSets = new List<CompletionSet>();
            GetCompletions("utils::", 7, completionSets);

            completionSets.Should().ContainSingle();

            completionSets[0].Completions.Should().Contain(c => c.DisplayText == "adist")
                .Which.Description.Should().Be("Approximate String Distances");
        }

        [Test]
        public void RCompletionSource_CommentsTest01() {
            List<CompletionSet> completionSets = new List<CompletionSet>();
            GetCompletions("#No", 3, completionSets);

            completionSets.Should().ContainSingle()
                .Which.Completions.Should().BeEmpty();
        }

        [Test]
        public void RCompletionSource_CommentsTest02() {
            List<CompletionSet> completionSets = new List<CompletionSet>();
            GetCompletions("#No", 0, completionSets);

            completionSets.Should().ContainSingle()
                .Which.Completions.Should().NotBeEmpty();
        }

        [Test]
        public void RCompletionSource_FunctionDefinitionTest01() {
            List<CompletionSet> completionSets = new List<CompletionSet>();
            GetCompletions("x <- function()", 14, completionSets);

            completionSets.Should().ContainSingle()
                .Which.Completions.Should().BeEmpty();
        }

        [Test]
        public void RCompletionSource_FunctionDefinitionTest02() {
            for (int i = 14; i <= 18; i++) {
                List<CompletionSet> completionSets = new List<CompletionSet>();
                GetCompletions("x <- function(a, b)", i, completionSets);

                completionSets.Should().ContainSingle()
                    .Which.Completions.Should().BeEmpty();
            }
        }

        [Test]
        public void RCompletionSource_FunctionDefinitionTest03() {
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
        public void RCompletionSource_CaseSentivityTest() {
            List<CompletionSet> completionSets = new List<CompletionSet>();
            GetCompletions("x <- T", 6, completionSets);

            completionSets.Should().ContainSingle();

            completionSets[0].Filter();

            completionSets[0].Completions.Should().NotBeEmpty()
                .And.OnlyContain(c => c.DisplayText[0] == 'T');
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
