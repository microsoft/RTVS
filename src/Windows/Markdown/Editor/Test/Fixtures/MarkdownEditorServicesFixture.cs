// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Common.Core.Services;
using Microsoft.Markdown.Editor.Test.Fixtures;
using Microsoft.R.Editor.Test.Fixtures;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.Markdown.Editor.Test {
    [ExcludeFromCodeCoverage]
    public class MarkdownEditorServicesFixture : REditorServicesFixture {
        protected override IEnumerable<string> GetAssemblyNames() => base.GetAssemblyNames().Concat(new[] {
            "Microsoft.Markdown.Editor.Windows.dll",
            "Microsoft.Markdown.Editor.Test.dll"
        });

        protected override void SetupServices(IServiceManager serviceManager, ITestInput testInput) {
            serviceManager.AddService(new TestMarkdownEditorSettings());
            base.SetupServices(serviceManager, testInput);
        }
    }
}