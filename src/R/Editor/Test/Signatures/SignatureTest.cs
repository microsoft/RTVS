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
        [InlineData(@"x <- as.matrix(x); break;", "as.matrix", 17)]
        [InlineData(@"x <- as.matrix(x; break;", "as.matrix", 16)]
        [InlineData(@"x <- as.matrix(x  ; break;", "as.matrix", 18)]
        public void Signature(string content, string expectedFunctionName, int expectedSignatureEnd) {
            AstRoot ast = RParser.Parse(content);

            int signatureEnd;
            int position = 7;
            string functionName = SignatureHelp.GetFunctionNameFromBuffer(ast, ref position, out signatureEnd);

            functionName.Should().Be(expectedFunctionName);
            signatureEnd.Should().Be(expectedSignatureEnd);
        }
    }
}
