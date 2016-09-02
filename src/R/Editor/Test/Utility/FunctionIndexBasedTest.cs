// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.Languages.Editor.Shell;
using Microsoft.R.Support.Help;
using Microsoft.R.Support.Test.Utility;
using Microsoft.UnitTests.Core.Mef;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;

namespace Microsoft.R.Editor.Test.Utility {
    [ExcludeFromCodeCoverage]
    public abstract class FunctionIndexBasedTest : IAsyncLifetime {
        protected readonly IExportProvider ExportProvider;
        protected readonly IEditorShell EditorShell;
        protected readonly IPackageIndex PackageIndex;
        protected readonly IFunctionIndex FunctionIndex;

        protected FunctionIndexBasedTest(AssemblyMefCatalogFixture catalog) {
            ExportProvider = catalog.CreateExportProvider();
            EditorShell = ExportProvider.GetExportedValue<IEditorShell>();
            PackageIndex = ExportProvider.GetExportedValue<IPackageIndex>();
            FunctionIndex = ExportProvider.GetExportedValue<IFunctionIndex>();
        }

        public Task InitializeAsync() {
            return PackageIndex.InitializeAsync(FunctionIndex);
        }

        public virtual async Task DisposeAsync() {
            await PackageIndex.DisposeAsync(ExportProvider);
            ExportProvider.Dispose();
        }
     }
}
