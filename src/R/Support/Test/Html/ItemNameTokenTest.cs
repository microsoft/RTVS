// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.Html.Core.Parser.Tokens;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;

namespace Microsoft.Html.Core.Test.Tokens {
    [ExcludeFromCodeCoverage]
    public class NameTokenTest {
        [Test]
        [Category.Html]
        public void NameToken_HasNameTest() {
            var token = NameToken.Create(0, 5);
            Assert.True(token.HasName());

            token = NameToken.Create(0, 5, 5, -1, 0);
            Assert.False(token.HasName());
        }

        [Test]
        [Category.Html]
        public void NameToken_HasPrefixTest() {
            var token = NameToken.Create(0, 5);
            Assert.False(token.HasPrefix());

            token = NameToken.Create(0, 5, 5, -1, 0);
            Assert.True(token.HasPrefix());
        }

        [Test]
        [Category.Html]
        public void NameToken_HasQualifiedNameTest() {
            var token = NameToken.Create(0, 5);
            Assert.False(token.HasQualifiedName());

            token = NameToken.Create(0, 5, 5, -1, 0);
            Assert.False(token.HasQualifiedName());

            token = NameToken.Create(0, 5, 5, 6, 4);
            Assert.True(token.HasQualifiedName());
        }

        [Test]
        [Category.Html]
        public void NameToken_IsWellFormedTest() {
            var token = NameToken.Create(0, 5);
            Assert.True(token.IsNameWellFormed());

            token = NameToken.Create(0, 5, 5, -1, 0);
            Assert.False(token.IsNameWellFormed());

            token = NameToken.Create(-1, 0, 5, -1, 0);
            Assert.False(token.IsNameWellFormed());

            token = NameToken.Create(0, 5, 5, -1, 0);
            Assert.False(token.IsNameWellFormed());

            token = NameToken.Create(0, 5, 5, 6, 4);
            Assert.True(token.IsNameWellFormed());
        }

        [Test]
        [Category.Html]
        public void ShiftTest() {
            var token = NameToken.Create(0, 5, 5, 6, 4);

            Assert.Equal(0, token.Start);
            Assert.Equal(10, token.End);

            Assert.Equal(0, token.PrefixRange.Start);
            Assert.Equal(5, token.PrefixRange.End);

            Assert.Equal(5, token.ColonRange.Start);
            Assert.Equal(6, token.ColonRange.End);

            Assert.Equal(6, token.NameRange.Start);
            Assert.Equal(10, token.NameRange.End);

            token.Shift(2);

            Assert.Equal(2, token.Start);
            Assert.Equal(12, token.End);

            Assert.Equal(2, token.PrefixRange.Start);
            Assert.Equal(7, token.PrefixRange.End);

            Assert.Equal(7, token.ColonRange.Start);
            Assert.Equal(8, token.ColonRange.End);

            Assert.Equal(8, token.NameRange.Start);
            Assert.Equal(12, token.NameRange.End);


            token = NameToken.Create(0, 5, 5, -1, 0);

            Assert.Equal(0, token.Start);
            Assert.Equal(6, token.End);

            Assert.Equal(-1, token.NameRange.Start);
            Assert.Equal(-1, token.NameRange.End);

            token.Shift(3);

            Assert.Equal(3, token.Start);
            Assert.Equal(9, token.End);

            Assert.Equal(-1, token.NameRange.Start);
            Assert.Equal(-1, token.NameRange.End);
        }

        [Test]
        [Category.Html]
        public void NameToken_QualifiedNameTest() {
            var token = NameToken.Create(0, 5, 5, 6, 4);

            Assert.Equal(0, token.QualifiedName.Start);
            Assert.Equal(10, token.QualifiedName.End);
        }
    }
}
