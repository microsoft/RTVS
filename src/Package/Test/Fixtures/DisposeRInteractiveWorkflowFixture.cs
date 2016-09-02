// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.VisualStudio.R.Package.Shell;

namespace Microsoft.VisualStudio.R.Package.Test.Fixtures {
    [ExcludeFromCodeCoverage]
    // Fixture doesn't import itself. Use AssemblyFixtureImportAttribute
    public sealed class DisposeRInteractiveWorkflowFixture : IDisposable {
        private readonly IRInteractiveWorkflow _workflow;

        public DisposeRInteractiveWorkflowFixture() {
            _workflow = VsAppShell.Current.ExportProvider.GetExportedValue<IRInteractiveWorkflowProvider>().GetOrCreate();
            _workflow.BrokerConnector.SwitchToLocalBroker(GetType().Name);
        }

        public void Dispose() {
            _workflow.Dispose();
        }
    }
}
