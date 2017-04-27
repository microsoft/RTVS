// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.R.Core.Parser;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;

namespace Microsoft.R.Editor.Test.Signatures {
    [ExcludeFromCodeCoverage]
    [Category.R.Signatures]
    public class SignatureTest {
        [CompositeTest]
        [InlineData(@"x <- as.matrix(x); break;", 7, "as.matrix", 17)]
        [InlineData(@"x <- as.matrix(x; break;", 7, "as.matrix", 16)]
        [InlineData(@"x <- as.matrix(x  ; break;", 7, "as.matrix", 18)]
        [InlineData(@"x <- as.matrix(func(x))", 16, "as.matrix", 23)]
        [InlineData(@"x <- as.matrix(func(x))", 20, "func", 22)]
        public void Signature(string content, int position, string expectedFunctionName, int expectedSignatureEnd) {
            var ast = RParser.Parse(content);
            var functionName = ast.GetFunctionNameFromBuffer(ref position, out int signatureEnd);

            functionName.Should().Be(expectedFunctionName);
            signatureEnd.Should().Be(expectedSignatureEnd);
        }
    }
}
