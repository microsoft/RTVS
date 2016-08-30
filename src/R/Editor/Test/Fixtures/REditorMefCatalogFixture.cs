// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.R.Support.Test;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.R.Editor.Test {
    [AssemblyFixture]
    [ExcludeFromCodeCoverage]
    public sealed class REditorMefCatalogFixture : REditorMefCatalogFixtureBase { }

    [ExcludeFromCodeCoverage]
    public class REditorMefCatalogFixtureBase : RSupportMefCatalogFixtureBase {
        protected override IEnumerable<string> GetBinDirectoryAssemblies() => base.GetBinDirectoryAssemblies().Concat(new[] {
            "Microsoft.R.Editor",
            "Microsoft.R.Editor.Test"
        });
    }
}
