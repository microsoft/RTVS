// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.R.Support.Test.Utility {
    [ExcludeFromCodeCoverage]
    [AssemblyFixture]
    public class SupportTestFilesSetup : DeployFilesFixture {
        public SupportTestFilesSetup() : base(@"R\Support\Test\RD\Files", "Files") {}
    }
}
