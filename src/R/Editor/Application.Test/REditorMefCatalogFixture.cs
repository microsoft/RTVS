// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.R.Editor.Test;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.R.Editor.Application.Test {
    [AssemblyFixture]
    [ExcludeFromCodeCoverage]
    public sealed class REditorApplicationMefCatalogFixture : REditorApplicationMefCatalogFixtureBase { }

    [ExcludeFromCodeCoverage]
    public class REditorApplicationMefCatalogFixtureBase : REditorMefCatalogFixtureBase {
        protected override IEnumerable<string> GetBinDirectoryAssemblies() => base.GetBinDirectoryAssemblies().Concat(new[] {
            "Microsoft.Markdown.Editor",
            "Microsoft.Markdown.Editor.Test",
            "Microsoft.Languages.Editor.Application",
            "Microsoft.R.Editor.Application.Test"
        });
    }
}
