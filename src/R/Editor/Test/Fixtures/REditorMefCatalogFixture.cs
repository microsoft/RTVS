// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.R.Support.Test;

namespace Microsoft.R.Editor.Test {
    // Fixture doesn't import itself. Use AssemblyFixtureImportAttribute
    [ExcludeFromCodeCoverage]
    public class REditorMefCatalogFixture : RSupportMefCatalogFixture {
        protected override IEnumerable<string> GetAssemblies() => base.GetAssemblies().Concat(new[] {
            "Microsoft.R.Editor",
            "Microsoft.R.Editor.Test"
        });
    }
}
