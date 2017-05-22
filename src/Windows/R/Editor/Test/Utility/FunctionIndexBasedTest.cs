// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.Common.Core.Services;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Editor.Functions;
using Microsoft.UnitTests.Core.Threading;
using Xunit;

namespace Microsoft.R.Editor.Test.Utility {
    [ExcludeFromCodeCoverage]
    public abstract class FunctionIndexBasedTest : IAsyncLifetime {
        protected IServiceContainer Services { get; }
        protected IPackageIndex PackageIndex { get; }
        protected IFunctionIndex FunctionIndex { get; }
        protected IRInteractiveWorkflow Workflow { get; }

        protected FunctionIndexBasedTest(IServiceContainer services) {
            Services = services;
            Workflow = UIThreadHelper.Instance.Invoke(() => Services.GetService<IRInteractiveWorkflowProvider>().GetOrCreate());
            FunctionIndex = Services.GetService<IFunctionIndex>();
            PackageIndex = Services.GetService<IPackageIndex>();
        }

        public async Task InitializeAsync() {
            await Workflow.RSessions.TrySwitchBrokerAsync(GetType().Name);
            await PackageIndex.BuildIndexAsync();
            await FunctionIndex.BuildIndexAsync();
        }

        public Task DisposeAsync() => Task.CompletedTask;
     }
}
