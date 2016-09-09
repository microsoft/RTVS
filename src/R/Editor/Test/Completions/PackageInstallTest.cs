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
            using (var script = new RHostScript(Workflow.RSessions)) {
                try {
                    await script.Session.ExecuteAsync("remove.packages('abc')", REvaluationKind.Mutating);
                } catch (RException) { }

                await PackageIndex.BuildIndexAsync();

                var completionSets = new List<CompletionSet>();
                GetCompletions("abc::", 5, completionSets);

                completionSets.Should().ContainSingle();
                completionSets[0].Completions.Should().BeEmpty();

                try {
                    await script.Session.ExecuteAsync("install.packages('abc')", REvaluationKind.Mutating);
                } catch (RException) { }

                await PackageIndex.BuildIndexAsync();

                completionSets.Clear();
                GetCompletions("abc::", 5, completionSets);

                completionSets.Should().ContainSingle();
                completionSets[0].Completions.Should().NotBeEmpty();
            }
        }
    }
}
