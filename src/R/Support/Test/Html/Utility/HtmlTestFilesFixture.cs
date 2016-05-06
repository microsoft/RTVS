// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.Html.Core.Test.Utility {
    [ExcludeFromCodeCoverage]
    [AssemblyFixture]
    public class HtmlTestFilesFixture : DeployFilesFixture {
        public HtmlTestFilesFixture() : base(@"R\Support\Test\Html\Files", "Files") {}
    }
}
