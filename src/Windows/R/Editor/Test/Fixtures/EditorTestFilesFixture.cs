// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.R.Editor.Test {
    [ExcludeFromCodeCoverage]
    [AssemblyFixture]
    public class EditorTestFilesFixture : DeployFilesFixture {
        public EditorTestFilesFixture() : base(@"R\Editor\Test\Files", "Files") {}
    }
}