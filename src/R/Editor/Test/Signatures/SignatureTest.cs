using System.Diagnostics.CodeAnalysis;
using Microsoft.Languages.Core.Test.Utility;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.Parser;
using Microsoft.R.Editor.Signatures;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.R.Editor.Test.Signatures {
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class SignatureTest : UnitTestBase {
        [TestMethod]
        [TestCategory("R.Signatures")]
        public void SignatureTest1() {
            string content = @"x <- as.matrix(x); break;";
            AstRoot ast = RParser.Parse(content);

            int signatureEnd;
            int position = 7;
            string functionName = SignatureHelp.GetFunctionNameFromBuffer(ast, ref position, out signatureEnd);

            Assert.AreEqual("as.matrix", functionName);
            Assert.AreEqual(17, signatureEnd);
        }

        [TestMethod]
        [TestCategory("R.Signatures")]
        public void SignatureTest2() {
            string content = @"x <- as.matrix(x; break;";
            AstRoot ast = RParser.Parse(content);

            int signatureEnd;
            int position = 7;
            string functionName = SignatureHelp.GetFunctionNameFromBuffer(ast, ref position, out signatureEnd);

            Assert.AreEqual("as.matrix", functionName);
            Assert.AreEqual(16, signatureEnd);
        }

        [TestMethod]
        [TestCategory("R.Signatures")]
        public void SignatureTest3() {
            string content = @"x <- as.matrix(x  ; break;";
            AstRoot ast = RParser.Parse(content);

            int signatureEnd;
            int position = 7;
            string functionName = SignatureHelp.GetFunctionNameFromBuffer(ast, ref position, out signatureEnd);

            Assert.AreEqual("as.matrix", functionName);
            Assert.AreEqual(18, signatureEnd);
        }
    }
}
