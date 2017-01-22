// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.Languages.Editor.Shell;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Support.Help;
using Microsoft.R.Support.Test.Utility;
using Microsoft.UnitTests.Core.Mef;
using Microsoft.UnitTests.Core.Threading;
using Xunit;

namespace Microsoft.R.Editor.Test.Utility {
    [ExcludeFromCodeCoverage]
    public abstract class FunctionIndexBasedTest : IAsyncLifetime {
        protected IExportProvider ExportProvider { get; }
        protected IEditorShell EditorShell { get; }
        protected IPackageIndex PackageIndex { get; }
        protected IFunctionIndex FunctionIndex { get; }
        protected IRInteractiveWorkflow Workflow { get; }

        protected FunctionIndexBasedTest(IExportProvider exportProvider) {
            ExportProvider = exportProvider;
            Workflow = UIThreadHelper.Instance.Invoke(() => ExportProvider.GetExportedValue<IRInteractiveWorkflowProvider>().GetOrCreate());
            EditorShell = ExportProvider.GetExportedValue<IEditorShell>();
            FunctionIndex = ExportProvider.GetExportedValue<IFunctionIndex>();
            PackageIndex = ExportProvider.GetExportedValue<IPackageIndex>();
        }

        public async Task InitializeAsync() {
            await Workflow.RSessions.TrySwitchBrokerAsync(GetType().Name);
            await PackageIndex.BuildIndexAsync();
            await FunctionIndex.BuildIndexAsync();
        }

        public async Task DisposeAsync() {
            await PackageIndex.DisposeAsync(ExportProvider);
        }
     }
}
