// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.R.Editor.Test;

namespace Microsoft.R.Editor.Application.Test {
    // Fixture doesn't import itself. Use AssemblyFixtureImportAttribute
    [ExcludeFromCodeCoverage]
    public class REditorApplicationMefCatalog : REditorAssemblyMefCatalog {
        protected override IEnumerable<string> GetAssemblies() => base.GetAssemblies().Concat(new[] {
            "Microsoft.Markdown.Editor",
            "Microsoft.Markdown.Editor.Test",
            "Microsoft.Languages.Editor.Application",
            "Microsoft.R.Editor.Application.Test"
        });
    }
}
