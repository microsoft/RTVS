// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.R.Core.Test {
    [ExcludeFromCodeCoverage]
    [AssemblyFixture]
    public class CoreTestFilesFixture : DeployFilesFixture {
        public CoreTestFilesFixture() : base(@"R\Core\Test\Files", "Files") { }
    }
}
