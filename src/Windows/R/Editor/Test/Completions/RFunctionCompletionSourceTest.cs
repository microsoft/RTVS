// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Common.Core.Services;
using Microsoft.Languages.Core.Text;
using Microsoft.R.Editor.Test.Utility;
using Microsoft.R.Host.Client.Test.Script;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.Language.Intellisense;
using Xunit;

namespace Microsoft.R.Editor.Test.Completions {
    [ExcludeFromCodeCoverage]
    [Category.R.Completion]
    [Collection(CollectionNames.NonParallel)]
    public class RFunctionCompletionSourceTest : FunctionIndexBasedTest {
        public RFunctionCompletionSourceTest(IServiceContainer services) : base(services) { }

        [Test]
        public void BaseFunctions01() {
            var completionSets = new List<CompletionSet>();
            RCompletionTestUtilities.GetCompletions(Services, "", 0, completionSets);

            completionSets.Should().ContainSingle()
                .Which.Completions.Should().Contain(c => c.DisplayText == "abbreviate")
                    .And.Contain(c => c.DisplayText == "abs");
        }

        [Test]
        public void BaseFunctions02() {
            var completionSets = new List<CompletionSet>();
            RCompletionTestUtilities.GetCompletions(Services, "FAC", 3, completionSets, new TextRange(0, 3));

            completionSets.Should().ContainSingle();
            completionSets[0].Filter();

            completionSets[0].Completions[0].DisplayText.Should().Be("factanal");
            completionSets[0].Completions[1].DisplayText.Should().Be("factor");
        }


        [Test]
        public void Packages01() {
            var completionSets = new List<CompletionSet>();
            RCompletionTestUtilities.GetCompletions(Services, "lIbrAry(", 8, completionSets);

            completionSets.Should().ContainSingle();

            completionSets[0].Completions.Should().Contain(c => c.DisplayText == "base")
                .Which.Description.Should().Be("Base R functions.");
        }

        [Test]
        public void RtvsPackage() {
            var completionSets = new List<CompletionSet>();
            RCompletionTestUtilities.GetCompletions(Services, "rtv", 3, completionSets, new TextRange(0, 3));

            completionSets.Should().ContainSingle();
            completionSets[0].Filter();
            completionSets[0].Completions[0].DisplayText.Should().Be("rtvs");
        }

        [CompositeTest]
        [InlineData("utils::", 7, "adist", "approximate string distance", false)]
        [InlineData("lm(utils::)", 10, "adist", "approximate string distance", false)]
        [InlineData("rtvs::", 6, "fetch_file", "used to download", true)]
        public async Task SpecificPackage(string content, int position, string expectedEntry, string expectedDescription, bool realHost) {
            var hostScript = realHost ? new RHostScript(Services) : null;
            try {
                var packageName = await FunctionIndex.GetPackageNameAsync(expectedEntry);
                packageName.Should().NotBeNull();

                var completionSets = new List<CompletionSet>();
                RCompletionTestUtilities.GetCompletions(Services, content, position, completionSets);

                completionSets.Should().ContainSingle();

                var entry = completionSets[0].Completions.FirstOrDefault(c => c.DisplayText == expectedEntry);
                entry.Should().NotBeNull();

                var description = entry.Description;
                description.Should().Contain(expectedDescription);
            } finally {
                hostScript?.Dispose();
            }
        }

        [Test]
        public void CaseSensitiveEntries() {
            var completionSets = new List<CompletionSet>();
            RCompletionTestUtilities.GetCompletions(Services, "ma", 2, completionSets);

            completionSets.Should().ContainSingle()
                .Which.Completions.Should()
                    .Contain(c => c.DisplayText == "matrix").And
                    .Contain(c => c.DisplayText == "Matrix");
        }

        [Test]
        public void NoDuplicatesEntries() {
            var completionSets = new List<CompletionSet>();
            RCompletionTestUtilities.GetCompletions(Services, "r", 1, completionSets);

            completionSets.Should().ContainSingle()
                .Which.Completions
                    .Should().ContainSingle(c => c.DisplayText == "require");
        }

        [Test]
        public void Datasets() {
            var completionSets = new List<CompletionSet>();
            RCompletionTestUtilities.GetCompletions(Services, "m", 1, completionSets);

            completionSets.Should().ContainSingle()
                .Which.Completions
                    .Should().Contain(c => c.DisplayText == "mtcars");
        }
    }
}
