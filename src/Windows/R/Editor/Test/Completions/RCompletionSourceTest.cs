// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Services;
using Microsoft.Languages.Core.Text;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.Language.Intellisense;
using Xunit;

namespace Microsoft.R.Editor.Test.Completions {
    [ExcludeFromCodeCoverage]
    [Category.R.Completion]
    public class RCompletionSourceTest {
        private readonly IServiceContainer _services;

        public RCompletionSourceTest(IServiceContainer services) {
            _services = services;
        }


        [Test]
        public void Keywords01() {
            var completionSets = new List<CompletionSet>();
            RCompletionTestUtilities.GetCompletions(_services, "f", 1, completionSets, new TextRange(0, 1));

            completionSets.Should().ContainSingle();
            completionSets[0].Filter();

            completionSets[0].Completions.Should().Contain(c => c.DisplayText == "for");
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
            var completionSets = new List<CompletionSet>();
            RCompletionTestUtilities.GetCompletions(_services, content, position, completionSets);

            completionSets.Should().ContainSingle()
                .Which.Completions.Should().BeEmpty();
        }

        [Test]
        public void BeforeComment() {
            var completionSets = new List<CompletionSet>();
            RCompletionTestUtilities.GetCompletions(_services, "#No", 0, completionSets);

            completionSets.Should().ContainSingle()
                .Which.Completions.Should().NotBeEmpty();
        }

        [Test]
        public void FunctionDefinition01() {
            var completionSets = new List<CompletionSet>();
            RCompletionTestUtilities.GetCompletions(_services, "x <- function()", 14, completionSets);

            completionSets.Should().ContainSingle()
                .Which.Completions.Should().BeEmpty();
        }

        [Test]
        public void FunctionDefinition02() {
            for (int i = 14; i <= 18; i++) {
                var completionSets = new List<CompletionSet>();
                RCompletionTestUtilities.GetCompletions(_services, "x <- function(a, b)", i, completionSets);

                completionSets.Should().ContainSingle()
                    .Which.Completions.Should().BeEmpty();
            }
        }

        [Test]
        public void FunctionDefinition03() {
            for (int i = 14; i <= 19; i++) {
                var completionSets = new List<CompletionSet>();
                RCompletionTestUtilities.GetCompletions(_services, "x <- function(a, b = x+y)", i, completionSets);

                completionSets.Should().ContainSingle()
                    .Which.Completions.Should().BeEmpty();
            }

            for (int i = 20; i <= 24; i++) {
                var completionSets = new List<CompletionSet>();
                RCompletionTestUtilities.GetCompletions(_services, "x <- function(a, b = x+y)", i, completionSets);

                completionSets.Should().NotBeEmpty();
                completionSets[0].Completions.Should().NotBeEmpty();
            }
        }

        [Test]
        public void UserVariables01() {
            var completionSets = new List<CompletionSet>();
            var content =
@"
aaa123 <- 1

bbb123 = 1

";
            RCompletionTestUtilities.GetCompletions(_services, content, 0, completionSets);

            completionSets.Should().ContainSingle();
            completionSets[0].Filter();

            completionSets[0].Completions.Should().NotBeEmpty()
                .And.Contain(c => c.DisplayText == "aaa123")
                .And.Contain(c => c.DisplayText == "bbb123");

            completionSets.Clear();
            RCompletionTestUtilities.GetCompletions(_services, content, 2, 0, completionSets);

            completionSets.Should().ContainSingle();
            completionSets[0].Filter();

            completionSets[0].Completions.Should().NotBeEmpty()
                .And.Contain(c => c.DisplayText == "aaa123")
                .And.Contain(c => c.DisplayText == "bbb123");

            completionSets.Clear();
            RCompletionTestUtilities.GetCompletions(_services, content, 4, 0, completionSets);

            completionSets.Should().ContainSingle();
            completionSets[0].Filter();

            completionSets[0].Completions.Should().NotBeEmpty()
                .And.Contain(c => c.DisplayText == "aaa123")
                .And.Contain(c => c.DisplayText == "bbb123");
        }

        [Test]
        public void UserVariables02() {
            var completionSets = new List<CompletionSet>();
            var content =
@"
{

    aaa123 <- 1

    1 -> bbb123

}
";
            RCompletionTestUtilities.GetCompletions(_services, content, 2, 0, completionSets);

            completionSets.Should().ContainSingle();
            completionSets[0].Filter();

            completionSets[0].Completions.Should().NotBeEmpty()
                .And.NotContain(c => c.DisplayText == "aaa123")
                .And.NotContain(c => c.DisplayText == "bbb123");

            completionSets.Clear();
            RCompletionTestUtilities.GetCompletions(_services, content, 4, 0, completionSets);

            completionSets.Should().ContainSingle();
            completionSets[0].Filter();

            completionSets[0].Completions.Should().NotBeEmpty()
                .And.Contain(c => c.DisplayText == "aaa123")
                .And.NotContain(c => c.DisplayText == "bbb123");

            completionSets.Clear();
            RCompletionTestUtilities.GetCompletions(_services, content, 6, 0, completionSets);

            completionSets.Should().ContainSingle();
            completionSets[0].Filter();

            completionSets[0].Completions.Should().NotBeEmpty()
                .And.Contain(c => c.DisplayText == "aaa123")
                .And.Contain(c => c.DisplayText == "bbb123");
        }

        [Test]
        public void UserVariables03() {
            var completionSets = new List<CompletionSet>();
            var content =
@"x123 <- 1
for(x456 in 1:10) x";

            RCompletionTestUtilities.GetCompletions(_services, content, 0, content.Length, completionSets);

            completionSets.Should().ContainSingle();
            completionSets[0].Filter();

            completionSets[0].Completions.Should().NotBeEmpty()
                .And.Contain(c => c.DisplayText == "x123")
                .And.Contain(c => c.DisplayText == "x456");
        }

        [Test]
        public void UserFunctions01() {
            var completionSets = new List<CompletionSet>();
            RCompletionTestUtilities.GetCompletions(_services, "aaaa <- function(a,b,c)\r\na", 25, completionSets);

            completionSets.Should().ContainSingle();
            completionSets[0].Filter();

            completionSets[0].Completions.Should().NotBeEmpty()
                .And.Contain(c => c.DisplayText == "aaaa");
        }

        [Test]
        public void UserFunctions02() {
            var completionSets = new List<CompletionSet>();
            var content =
@"
aaa123 <- function(a,b,c) { }
while(TRUE) {
aaa456 <- function() { }
#
aa
}";
            RCompletionTestUtilities.GetCompletions(_services, content, content.IndexOf('#') + 4, completionSets);
            completionSets.Should().ContainSingle();
            completionSets[0].Completions.Should().BeEmpty();

            completionSets.Clear();
            RCompletionTestUtilities.GetCompletions(_services, content, content.IndexOf('#') + 5, completionSets);
            completionSets.Should().ContainSingle();
            completionSets[0].Filter();

            var completions = completionSets[0].Completions;
            completions.Should().NotBeEmpty();
            completions.Should().Contain(c => c.DisplayText == "aaa123");
            completions.Should().Contain(c => c.DisplayText == "aaa456");
        }

        [Test]
        public void UserFunctions03() {
            var completionSets = new List<CompletionSet>();
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
            RCompletionTestUtilities.GetCompletions(_services, content, 4, 0, completionSets);

            completionSets.Should().ContainSingle();
            completionSets[0].Filter();

            var completions = completionSets[0].Completions;
            completions.Should().NotBeEmpty();
            completions.Should().Contain(c => c.DisplayText == "aaa123");
            completions.Should().NotContain(c => c.DisplayText == "aaa456");
            completions.Should().Contain(c => c.DisplayText == "aaa789");

            completionSets.Clear();
            RCompletionTestUtilities.GetCompletions(_services, content, 7, 0, completionSets);

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
            var completionSets = new List<CompletionSet>();
            string content =
@"
aaa <- function(a, b, c) { }
aaa(a
";
            RCompletionTestUtilities.GetCompletions(_services, content, 2, 5, completionSets);

            completionSets.Should().ContainSingle();
            completionSets[0].Filter();

            completionSets[0].Completions.Should().NotBeEmpty()
                .And.Contain(c => c.DisplayText == "a =");
        }

        [CompositeTest]
        [InlineData(
            @"
aaa <- function(a, b, c) { }
aaa(x, ", "a =")]
        [InlineData(
            @"
aaa <- function(a, b, c) { }
aaa(x, aaa(y,", "a =")]
        [InlineData(
            @"
aaa <- function(a, b, c) { }
aaa(x, (1+2,", "a =")]
        [InlineData(
            @"
aaa <- function(a, b, c) { } bbb <- function(x, y, z) { }
aaa(x, bbb(y,", "x =")]
        public void UserFunctionArgumentsNoBrace(string content, string expectedEntry) {
            var completionSets = new List<CompletionSet>();
            RCompletionTestUtilities.GetCompletions(_services, content, 2, content.Length - content.IndexOfOrdinal("aaa("), completionSets);

            completionSets.Should().ContainSingle();
            completionSets[0].Filter();

            completionSets[0].Completions.Should().NotBeEmpty()
                .And.Contain(c => c.DisplayText == expectedEntry);
        }
    }
}
