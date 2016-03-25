// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.Common.Core.Test.Utility;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.Parser;
using Microsoft.R.Editor.Signatures;
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
            AstRoot ast = RParser.Parse(content);

            int signatureEnd;
            string functionName = SignatureHelp.GetFunctionNameFromBuffer(ast, ref position, out signatureEnd);

            functionName.Should().Be(expectedFunctionName);
            signatureEnd.Should().Be(expectedSignatureEnd);
        }
    }
}
