// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.R.Editor.Application.Test {
    [ExcludeFromCodeCoverage]
    [AssemblyFixture]
    public class EditorAppTestFilesFixture : DeployFilesFixture {
        public EditorAppTestFilesFixture() : base(@"Windows\R\Editor\Application.Test\Files", "Files") { }
    }
}
