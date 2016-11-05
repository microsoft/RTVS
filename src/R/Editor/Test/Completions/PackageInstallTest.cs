// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Test.Script;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.Language.Intellisense;
using Xunit;

namespace Microsoft.R.Editor.Test.Completions {
    [ExcludeFromCodeCoverage]
    [Category.R.Completion]
    [Collection(CollectionNames.NonParallel)]
    public class PackageInstallTest : RCompletionSourceTestBase {
        public PackageInstallTest(REditorMefCatalogFixture catalog) : base(catalog) {}

        [Test]
        public async Task InstallPackageTest() {
            await Workflow.RSession.EnsureHostStartedAsync(new RHostStartupInfo {Name = nameof(InstallPackageTest)}, null, 50000);

            var completionSets = new List<CompletionSet>();
            for (int i = 0; i < 2; i++) {
                try {
                    await Workflow.Packages.UninstallPackageAsync("abc", null);
                    //await Workflow.RSession.ExecuteAsync("remove.packages('abc')", REvaluationKind.Mutating);
                } catch (RException) { }

                await PackageIndex.BuildIndexAsync();

                completionSets.Clear();
                GetCompletions("abc::", 5, completionSets);

                completionSets.Should().ContainSingle();
                // Try again one more time
                if (completionSets[0].Completions.Count == 0) {
                    break;
                }
            }
            completionSets[0].Completions.Should().BeEmpty();

            try {
                await Workflow.RSession.ExecuteAsync("install.packages('abc')", REvaluationKind.Mutating);
            } catch (RException) { }

            await PackageIndex.BuildIndexAsync();

            completionSets.Clear();
            GetCompletions("abc::", 5, completionSets);

            completionSets.Should().ContainSingle();
            completionSets[0].Completions.Should().NotBeEmpty();
        }
    }
}
