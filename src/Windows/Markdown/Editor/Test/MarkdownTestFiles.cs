// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.Markdown.Editor.Test {
    [ExcludeFromCodeCoverage]
    [AssemblyFixture]
    public class MarkdownTestFilesFixture : DeployFilesFixture {
        public MarkdownTestFilesFixture() : base(@"Markdown\Editor\Test\Files", "Files") { }
    }
}
