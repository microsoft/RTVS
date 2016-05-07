// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.Html.Core.Parser.Tokens;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;

namespace Microsoft.Html.Core.Test.Tokens {
    [ExcludeFromCodeCoverage]
    public class TokenTest {
        [Test]
        [Category.Html]
        public void TokenConstructorTest() {
            var target = new HtmlToken(-1, 0);
            Assert.Equal(0, target.Length);
        }

        [Test]
        [Category.Html]
        public void ShiftTest() {
            var target = new HtmlToken(17, 5);
            Assert.Equal(17, target.Start);
            Assert.Equal(22, target.End);

            target.Shift(8);
            Assert.Equal(25, target.Start);
            Assert.Equal(30, target.End);

            target.Shift(-5);
            Assert.Equal(20, target.Start);
            Assert.Equal(25, target.End);
        }

        [Test]
        [Category.Html]
        public void EndTest() {
            var target = new HtmlToken(17, 5);
            Assert.Equal(22, target.End);
        }

        [Test]
        [Category.Html]
        public void StartTest() {
            var target = new HtmlToken(17, 22);
            Assert.Equal(17, target.Start);
        }
    }
}
