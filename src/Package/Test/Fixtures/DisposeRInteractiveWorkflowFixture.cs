// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.VisualStudio.R.Package.Shell;
using Xunit;

namespace Microsoft.VisualStudio.R.Package.Test.Fixtures {
    [ExcludeFromCodeCoverage]
    // Fixture doesn't import itself. Use AssemblyFixtureImportAttribute
    public sealed class DisposeRInteractiveWorkflowFixture : IAsyncLifetime {
        private readonly IRInteractiveWorkflow _workflow;

        public DisposeRInteractiveWorkflowFixture() {
            _workflow = VsAppShell.Current.GetService<IRInteractiveWorkflowProvider>().GetOrCreate();
        }
        
        public async Task InitializeAsync() {
            await _workflow.RSessions.TrySwitchBrokerAsync(GetType().Name);
        }

        public Task DisposeAsync() {
            _workflow.Dispose();
            return Task.CompletedTask;
        }
    }
}
