// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using FluentAssertions;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Services;
using Microsoft.R.Editor.Roxygen;
using Microsoft.R.Editor.Test.Completions;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.Language.Intellisense;
using Xunit;

namespace Microsoft.R.Editor.Test.Roxygen {
    [ExcludeFromCodeCoverage]
    [Category.Roxygen]
    public class RoxygenCompletionTest {
        private readonly IServiceContainer _services;

        public RoxygenCompletionTest(IServiceContainer services) {
            _services = services;
        }


        [Test]
        public void Keywords01() {
            var completionSets = new List<CompletionSet>();
            RCompletionTestUtilities.GetCompletions(_services, "   ", 1, completionSets);
            completionSets.Should().ContainSingle();

            var filtered = completionSets.First().Completions.Where(c => c.DisplayText.StartsWithOrdinal("@"));
            filtered.Should().BeEmpty();
        }

        [CompositeTest]
        [InlineData("#  f", 1)]
        [InlineData("#'  f", 2)]
        public void Keywords02(string content, int start) {
            for (var i = start; i < content.Length; i++) {
            var completionSets = new List<CompletionSet>();
                RCompletionTestUtilities.GetCompletions(_services, content, i, completionSets);
                completionSets.Should().ContainSingle();
                completionSets.First().Completions.Should().BeEmpty();
            }
        }


        [Test]
        public void Keywords03() {
            var completionSets = new List<CompletionSet>();
            RCompletionTestUtilities.GetCompletions(_services, "#' @", 4, completionSets);
            completionSets.Should().ContainSingle();
            completionSets.First().Completions.Should().HaveCount(RoxygenKeywords.Keywords.Length);
        }

        [CompositeTest]
        [InlineData("#' @alia", 7)]
        public void Filtering(string content, int start) {
            for (var i = start; i < content.Length; i++) {
                var completionSets = new List<CompletionSet>();
                RCompletionTestUtilities.GetCompletions(_services, content, i, completionSets);
                completionSets.Should().ContainSingle();
                completionSets[0].Filter();
                completionSets[0].Completions.Should().ContainSingle();
            }
        }
    }
}
