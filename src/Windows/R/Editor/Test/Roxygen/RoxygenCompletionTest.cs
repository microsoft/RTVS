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

        [Test]
        public void Keywords02() {
            var completionSets = new List<CompletionSet>();
            RCompletionTestUtilities.GetCompletions(_services, "#  ", 1, completionSets);
            completionSets.Should().ContainSingle();
            completionSets.First().Completions.Should().BeEmpty();
        }

        [Test]
        public void Keywords03() {
            var completionSets = new List<CompletionSet>();
            RCompletionTestUtilities.GetCompletions(_services, "#'  ", 1, completionSets);
            completionSets.Should().ContainSingle();
            completionSets.First().Completions.Should().BeEmpty();
        }

        [Test]
        public void Keywords04() {
            var completionSets = new List<CompletionSet>();
            RCompletionTestUtilities.GetCompletions(_services, "#'  ", 2, completionSets);
            completionSets.Should().ContainSingle();
            completionSets.First().Completions.Should().HaveCount(RoxygenKeywords.Keywords.Length);
        }
    }
}
