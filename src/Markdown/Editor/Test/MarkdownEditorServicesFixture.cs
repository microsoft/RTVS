// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.R.Editor.Test.Fixtures;

namespace Microsoft.Markdown.Editor.Test {
    [ExcludeFromCodeCoverage]
    public class MarkdownEditorServicesFixture : REditorServicesFixture {
        protected override IEnumerable<string> GetAssemblyNames() => base.GetAssemblyNames().Concat(new[] {
            "Microsoft.Markdown.Editor.dll",
            "Microsoft.Markdown.Editor.Test.dll"
        });
    }
}