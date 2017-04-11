// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
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

        [CompositeTest]
        [InlineData("\t\n", "\"\\t\\n\"")]
        [InlineData("\\t\n", "\"\\\\t\\n\"")]
        [InlineData("\n", "\"\\n\"")]
        [InlineData("\\n", "\"\\\\n\"")]
        public void EscapeCharacterTest(string source, string expectedLiteral) {
            string actualLiteral = source.ToRStringLiteral();
            actualLiteral.Should().Be(expectedLiteral);
            string actualSource = actualLiteral.FromRStringLiteral();
            actualSource.Should().Be(source);
        }

        [CompositeTest]
        [InlineData("\"hello\"", "hello")]
        [InlineData("\"z\\xa\"", "z\n")]
        [InlineData("\"z\\xA\"", "z\n")]
        [InlineData("\"z\\xAz\"", "z\nz")]
        [InlineData("\"z\\x0a\"", "z\n")]
        [InlineData("\"z\\x0A\"", "z\n")]
        [InlineData("\"z\\x20\"", "z ")]
        [InlineData("\"z\\x200\"", "z 0")]
        [InlineData("\"z\\40\"", "z ")]
        [InlineData("\"z\\40a\"", "z a")]
        [InlineData("\"z\\101\"", "zA")]
        [InlineData("\"z\\7\"", "z\x7")]
        public void FromRStringLiteral(string source, string expected) {
            string actual = source.FromRStringLiteral();
            actual.Should().Be(expected);
        }

        [CompositeTest]
        [InlineData("\"hello\\x\"")]
        [InlineData("\"hello\\xg\"")]
        public void FromRStringLiteralError(string source) {
            Assert.Throws<FormatException>(() => source.FromRStringLiteral());
        }
    }
}
