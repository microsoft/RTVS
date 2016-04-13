// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;

namespace Microsoft.R.Host.Client.Test {
    [ExcludeFromCodeCoverage]
    public class RStringExtensionsTest {
        [Test]
        [Category.Variable.Explorer]
        public void ConvertCharacterCodesTest() {
            string target = "<U+4E2D><U+570B><U+8A9E>";
            target.ConvertCharacterCodes().Should().Be("中國語");
        }
    }
}
