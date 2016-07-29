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
        protected readonly IExportProvider _exportProvider;
        protected readonly IEditorShell _editorShell;
        protected readonly IPackageIndex _packageIndex;
        protected readonly IFunctionIndex _functionIndex;

        protected FunctionIndexBasedTest(AssemblyMefCatalogFixture catalog) {
            _exportProvider = catalog.CreateExportProvider();
            _editorShell = _exportProvider.GetExportedValue<IEditorShell>();
            _packageIndex = _exportProvider.GetExportedValue<IPackageIndex>();
            _functionIndex = _exportProvider.GetExportedValue<IFunctionIndex>();
        }

        public Task InitializeAsync() {
            return _packageIndex.InitializeAsync(_functionIndex);
        }

        public virtual async Task DisposeAsync() {
            await _packageIndex.DisposeAsync(_exportProvider);
            _exportProvider.Dispose();
        }
     }
}
