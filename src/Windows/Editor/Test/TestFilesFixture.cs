// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.Languages.Editor.Test {
    [ExcludeFromCodeCoverage]
    [AssemblyFixture]
    public class TestFilesFixture : DeployFilesFixture {
        public TestFilesFixture() : base(@"Common\Editor\Test\Files", "Files") { }
    }
}
