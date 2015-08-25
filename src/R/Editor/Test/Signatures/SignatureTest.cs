using Microsoft.Languages.Core.Test.Utility;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.Parser;
using Microsoft.R.Editor.Signatures;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.R.Editor.Test.Signatures
{
    [TestClass]
    public class SignatureTest : UnitTestBase
    {
        [TestMethod]
        public void SignatureTest1()
        {
            string content = @"x <- as.matrix(x); break;";
            AstRoot ast = RParser.Parse(content);

            int signatureEnd;
            string functionName = SignatureHelp.GetFunctionNameFromBuffer(ast, 7, out signatureEnd);

            Assert.AreEqual("as.matrix", functionName);
            Assert.AreEqual(17, signatureEnd);
        }

        [TestMethod]
        public void SignatureTest2()
        {
            string content = @"x <- as.matrix(x; break;";
            AstRoot ast = RParser.Parse(content);

            int signatureEnd;
            string functionName = SignatureHelp.GetFunctionNameFromBuffer(ast, 7, out signatureEnd);

            Assert.AreEqual("as.matrix", functionName);
            Assert.AreEqual(16, signatureEnd);
        }

        [TestMethod]
        public void SignatureTest3()
        {
            string content = @"x <- as.matrix(x  ; break;";
            AstRoot ast = RParser.Parse(content);

            int signatureEnd;
            string functionName = SignatureHelp.GetFunctionNameFromBuffer(ast, 7, out signatureEnd);

            Assert.AreEqual("as.matrix", functionName);
            Assert.AreEqual(16, signatureEnd);
        }
    }
}
