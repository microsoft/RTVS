// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition.Hosting;
using System.Diagnostics.CodeAnalysis;
using Microsoft.R.Support.Test;

namespace Microsoft.R.Editor.Test {
    [ExcludeFromCodeCoverage]
    public class REditorShellProviderFixture : RSupportShellProviderFixture {
        protected override CompositionContainer CreateCompositionContainer() {
            var catalog = new REditorAssemblyMefCatalog();
            return catalog.CreateContainer();
        }
    }
}
