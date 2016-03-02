// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Text;
using FluentAssertions;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.Common.Core.Test
{
    [ExcludeFromCodeCoverage]
    public class StringBuilderExtensionsTest
    {
        [Test]
        public void AppendIf_True()
        {
            var sb = new StringBuilder();
            sb.AppendIf(true, "ab");
            sb.ToString().Should().Be("ab");
        }

        [Test]
        public void AppendIf_False()
        {
            var sb = new StringBuilder();
            sb.AppendIf(false, "ab");
            sb.ToString().Should().BeEmpty();
        }
    }
}