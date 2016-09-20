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
using Xunit;

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
            GetCompletions("FAC", 3, completionSets, new TextRange(0, 3));

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
            GetCompletions("librAry(", 8, completionSets);

            completionSets.Should().ContainSingle();

            completionSets[0].Completions.Should().Contain(c => c.DisplayText == "base")
                .Which.Description.Should().Be("Base R functions.");
        }

        [CompositeTest]
        [InlineData("utils::", 7, "adist", "Approximate String Distances")]
        [InlineData("lm(utils::)", 10, "adist", "Approximate String Distances")]
        public void SpecificPackage(string content, int position, string expectedEntry, string expectedDescription) {
            List<CompletionSet> completionSets = new List<CompletionSet>();
            GetCompletions(content, position, completionSets);

            completionSets.Should().ContainSingle();

            completionSets[0].Completions.Should().Contain(c => c.DisplayText == expectedEntry)
                .Which.Description.Should().Be(expectedDescription);
        }

        [CompositeTest]
        [InlineData("#No", 3)]
        [InlineData("\"i \"", 2)]
        [InlineData("'i '", 2)]
        [InlineData("iii ", 2)]
        [InlineData("`i `", 2)]
        [InlineData("2. ", 2)]
        [InlineData("' ", 2)]
        [InlineData("\"a", 2)]
        [InlineData("\"a'", 2)]
        [InlineData("\"", 1)]
        public void SuppressedCompletion(string content, int position) {
            List<CompletionSet> completionSets = new List<CompletionSet>();
            GetCompletions(content, position, completionSets);

            completionSets.Should().ContainSingle()
                .Which.Completions.Should().BeEmpty();
        }

        [Test]
        public void BeforeComment() {
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
        public void UserVariables01() {
            List<CompletionSet> completionSets = new List<CompletionSet>();
            var content =
@"
aaa123 <- 1

bbb123 = 1

";
            GetCompletions(content, 0, completionSets);

            completionSets.Should().ContainSingle();
            completionSets[0].Filter();

            completionSets[0].Completions.Should().NotBeEmpty()
                .And.Contain(c => c.DisplayText == "aaa123")
                .And.Contain(c => c.DisplayText == "bbb123");

            completionSets.Clear();
            GetCompletions(content, 2, 0, completionSets);

            completionSets.Should().ContainSingle();
            completionSets[0].Filter();

            completionSets[0].Completions.Should().NotBeEmpty()
                .And.Contain(c => c.DisplayText == "aaa123")
                .And.Contain(c => c.DisplayText == "bbb123");

            completionSets.Clear();
            GetCompletions(content, 4, 0, completionSets);

            completionSets.Should().ContainSingle();
            completionSets[0].Filter();

            completionSets[0].Completions.Should().NotBeEmpty()
                .And.Contain(c => c.DisplayText == "aaa123")
                .And.Contain(c => c.DisplayText == "bbb123");
        }

        [Test]
        public void UserVariables02() {
            List<CompletionSet> completionSets = new List<CompletionSet>();
            var content =
@"
{

    aaa123 <- 1

    1 -> bbb123

}
";
            GetCompletions(content, 2, 0, completionSets);

            completionSets.Should().ContainSingle();
            completionSets[0].Filter();

            completionSets[0].Completions.Should().NotBeEmpty()
                .And.NotContain(c => c.DisplayText == "aaa123")
                .And.NotContain(c => c.DisplayText == "bbb123");

            completionSets.Clear();
            GetCompletions(content, 4, 0, completionSets);

            completionSets.Should().ContainSingle();
            completionSets[0].Filter();

            completionSets[0].Completions.Should().NotBeEmpty()
                .And.Contain(c => c.DisplayText == "aaa123")
                .And.NotContain(c => c.DisplayText == "bbb123");

            completionSets.Clear();
            GetCompletions(content, 6, 0, completionSets);

            completionSets.Should().ContainSingle();
            completionSets[0].Filter();

            completionSets[0].Completions.Should().NotBeEmpty()
                .And.Contain(c => c.DisplayText == "aaa123")
                .And.Contain(c => c.DisplayText == "bbb123");
        }

        [Test]
        public void UserVariables03() {
            List<CompletionSet> completionSets = new List<CompletionSet>();
            var content = 
@"x123 <- 1
for(x456 in 1:10) x";

            GetCompletions(content, 0, content.Length, completionSets);

            completionSets.Should().ContainSingle();
            completionSets[0].Filter();

            completionSets[0].Completions.Should().NotBeEmpty()
                .And.Contain(c => c.DisplayText == "x123")
                .And.Contain(c => c.DisplayText == "x456");
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
            completionSets[0].Completions.Should().BeEmpty();

            completionSets.Clear();
            GetCompletions(content, content.IndexOf('#') + 5, completionSets);
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

aa
aaa456 <- function() { }

aa
}
aaa789 = function(a,b,c) { }
";
            GetCompletions(content, 4, 0, completionSets);

            completionSets.Should().ContainSingle();
            completionSets[0].Filter();

            var completions = completionSets[0].Completions;
            completions.Should().NotBeEmpty();
            completions.Should().Contain(c => c.DisplayText == "aaa123");
            completions.Should().NotContain(c => c.DisplayText == "aaa456");
            completions.Should().Contain(c => c.DisplayText == "aaa789");

            completionSets.Clear();
            GetCompletions(content, 7, 0, completionSets);

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
            string content =
@"
aaa <- function(a, b, c) { }
aaa(a
";
            GetCompletions(content, 2, 5, completionSets);

            completionSets.Should().ContainSingle();
            completionSets[0].Filter();

            completionSets[0].Completions.Should().NotBeEmpty()
                .And.Contain(c => c.DisplayText == "a =");
        }

        private void GetCompletions(string content, int lineNumber, int column, IList<CompletionSet> completionSets, ITextRange selectedRange = null) {
            TextBufferMock textBuffer = new TextBufferMock(content, RContentTypeDefinition.ContentType);
            var line = textBuffer.CurrentSnapshot.GetLineFromLineNumber(lineNumber);
            GetCompletions(content, line.Start + column, completionSets, selectedRange);
        }

        private void GetCompletions(string content, int caretPosition, IList<CompletionSet> completionSets, ITextRange selectedRange = null) {
            AstRoot ast = RParser.Parse(content);

            TextBufferMock textBuffer = new TextBufferMock(content, RContentTypeDefinition.ContentType);
            TextViewMock textView = new TextViewMock(textBuffer, caretPosition);

            if (selectedRange != null) {
                textView.Selection.Select(new SnapshotSpan(textBuffer.CurrentSnapshot, selectedRange.Start, selectedRange.Length), false);
            }

            CompletionSessionMock completionSession = new CompletionSessionMock(textView, completionSets, caretPosition);
            RCompletionSource completionSource = new RCompletionSource(textBuffer);

            completionSource.PopulateCompletionList(caretPosition, completionSession, completionSets, ast);
        }
    }
}
