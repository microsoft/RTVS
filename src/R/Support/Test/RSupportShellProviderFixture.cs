// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition.Hosting;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.Common.Core.Extensions;
using Microsoft.Languages.Editor.Test;
using Microsoft.R.Components.Settings;
using Microsoft.R.Support.Test.Utility;
using Microsoft.UnitTests.Core.XUnit;
using Xunit.Sdk;

namespace Microsoft.R.Support.Test {
    // Fixture doesn't import itself. Use AssemblyFixtureImportAttribute
    [AssemblyFixture]
    [ExcludeFromCodeCoverage]
    public class RSupportShellProviderFixture : EditorShellProviderFixture {
        private ITestInput _testInput;

        protected override CompositionContainer CreateCompositionContainer() {
            var catalog = new RSupportAssemblyMefCatalog();
            return catalog.CreateContainer();
        }

        public override async Task<Task<RunSummary>> InitializeAsync(ITestInput testInput, IMessageBus messageBus) {
            _testInput = testInput;
            await base.InitializeAsync(testInput, messageBus);
            ServiceManager.AddService(new TestRToolsSettings());
            return DefaultInitializeResult;
        }

        protected override void AddExports(CompositionBatch batch) {
            batch.AddValue<IRSettings>(new TestRToolsSettings(_testInput.FileSytemSafeName));
        }
    }
}
