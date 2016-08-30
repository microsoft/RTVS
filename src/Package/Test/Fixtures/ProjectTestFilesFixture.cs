// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.VisualStudio.R.Package.Test {
    [ExcludeFromCodeCoverage]
    [AssemblyFixture]
    public class PackageTestFilesFixture : DeployFilesFixture {
        public PackageTestFilesFixture() : base(@"Package\Test\Files", "Files") { }
    }
}
