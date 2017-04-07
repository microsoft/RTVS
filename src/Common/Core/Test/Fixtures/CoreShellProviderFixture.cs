// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition.Hosting;
using System.Threading.Tasks;
using Microsoft.Common.Core.Extensions;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.Test.Fakes.Shell;
using Microsoft.UnitTests.Core.Shell;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.UnitTests.Core.XUnit.MethodFixtures;
using Xunit.Sdk;

namespace Microsoft.Common.Core.Test.Fixtures {
    public abstract class CoreShellProviderFixture : MethodFixtureBase, ICoreShellProvider {
        private CompositionContainer _compositionContainer;
        public ICoreShell CoreShell { get; private set; }

        public override Task<Task<RunSummary>> InitializeAsync(ITestInput testInput, IMessageBus messageBus) {
            _compositionContainer = CreateCompositionContainer();
            var tcs = new TestCoreShell(new TestCompositionCatalog(_compositionContainer));
            ServiceManager = tcs.ServiceManager;
            CoreShell = tcs;

            var batch = new CompositionBatch().AddValue(CoreShell);
            AddExports(batch);
            _compositionContainer.Compose(batch);

            return base.InitializeAsync(testInput, messageBus);
        }

        public override Task DisposeAsync(RunSummary result, IMessageBus messageBus) {
            _compositionContainer?.Dispose();
            return base.DisposeAsync(result, messageBus);
        }

        protected IServiceManager ServiceManager { get; private set; }
        protected abstract CompositionContainer CreateCompositionContainer();
        protected virtual void AddExports(CompositionBatch batch) { }
    }
}
