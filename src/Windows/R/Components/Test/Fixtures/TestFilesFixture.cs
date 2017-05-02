// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.R.Components.Test {
    [ExcludeFromCodeCoverage]
    [AssemblyFixture]
    public class TestFilesFixture : DeployFilesFixture {
        public TestFilesFixture() : base(@"Windows\R\Components\Test\Files", "Files") { }

        public string ReposDestinationPath => GetDestinationPath("Repos");
        public string LibraryDestinationPath => GetDestinationPath("Library");
        public string Library2DestinationPath => GetDestinationPath("Library2");
    }
}
