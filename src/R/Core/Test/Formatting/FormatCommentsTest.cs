// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.R.Core.Formatting;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.R.Core.Test.Formatting {
    [ExcludeFromCodeCoverage]
    public class FormatCommentsTest {
        [Test]
        [Category.R.Formatting]
        public void InlineComment() {
            RFormatter f = new RFormatter();
            string actual = f.Format("if(1 && # comment\n   2) x");
            actual.Should().Be("if (1 && # comment\n   2)\n  x");
        }
    }
}
