// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition.Hosting;
using System.Diagnostics.CodeAnalysis;
using Microsoft.R.Editor.Test;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.R.Editor.Application.Test {
    // Fixture doesn't import itself. Use AssemblyFixtureImportAttribute
    [AssemblyFixture]
    [ExcludeFromCodeCoverage]
    public class REditorApplicationShellProviderFixture : REditorShellProviderFixture {
        protected override CompositionContainer CreateCompositionContainer() {
            var catalog = new REditorApplicationMefCatalog();
            return catalog.CreateContainer();
        }
    }
}
