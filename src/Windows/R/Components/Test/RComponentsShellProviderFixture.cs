// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition.Hosting;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.Common.Core.Extensions;
using Microsoft.Common.Core.Test.Fixtures;
using Microsoft.R.Components.Settings;
using Microsoft.R.Components.Test.StubFactories;
using Microsoft.UnitTests.Core.XUnit;
using Xunit.Sdk;

namespace Microsoft.R.Components.Test {
    [ExcludeFromCodeCoverage]
    public sealed class RComponentsShellProviderFixture : CoreShellProviderFixture {
        private ITestInput _testInput;

        protected override CompositionContainer CreateCompositionContainer() {
            var catalog = new RComponentsMefCatalog();
            return catalog.CreateContainer();
        }

        public override Task<Task<RunSummary>> InitializeAsync(ITestInput testInput, IMessageBus messageBus) {
            _testInput = testInput;
            return base.InitializeAsync(testInput, messageBus);
        }

        protected override void AddExports(CompositionBatch batch) {
            batch.AddValue<IRSettings>(RSettingsStubFactory.CreateForExistingRPath(_testInput.FileSytemSafeName));
        }
    }
}
