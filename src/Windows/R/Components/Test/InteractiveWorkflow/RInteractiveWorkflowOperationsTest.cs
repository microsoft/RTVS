// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.Common.Core.Services;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.UnitTests.Core.Threading;
using Xunit;

namespace Microsoft.R.Components.Test.InteractiveWorkflow {
    [ExcludeFromCodeCoverage]
    public class RInteractiveWorkflowOperationsTest : IAsyncLifetime  {
        private readonly IRInteractiveWorkflowVisual _workflow;
        private IInteractiveWindowVisualComponent _workflowComponent;

        public RInteractiveWorkflowOperationsTest(IServiceContainer services) {
            _workflow = services.GetService<IRInteractiveWorkflowVisualProvider>().GetOrCreate();
        }

        public async Task InitializeAsync() {
            _workflowComponent = await UIThreadHelper.Instance.Invoke(() => _workflow.GetOrCreateVisualComponentAsync());
        }

        public Task DisposeAsync() {
            _workflowComponent.Dispose();
            return Task.CompletedTask;
        }
    }
}
