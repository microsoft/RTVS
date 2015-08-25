using Microsoft.Languages.Core.Test.Utility;
using Microsoft.Languages.Editor.Test.Mocks;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.Parser;
using Microsoft.R.Editor.ContentType;
using Microsoft.R.Editor.Signatures;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.Text;

namespace Microsoft.R.Editor.Test.Signatures
{
    [TestClass]
    public class ParameterTest : UnitTestBase
    {
        [TestMethod]
        public void ParameterTest1()
        {
            string content = @"x <- foo(a,b,c,d)";
            AstRoot ast = RParser.Parse(content);

            ITextBuffer textBuffer = new TextBufferMock(content, RContentTypeDefinition.ContentType);
            ParametersInfo parametersInfo = SignatureHelp.GetParametersInfoFromBuffer(ast, textBuffer.CurrentSnapshot, 10);

            Assert.IsNotNull(parametersInfo);
            Assert.IsNotNull(parametersInfo.FunctionCall);
            Assert.AreEqual("foo", parametersInfo.FunctionName);
            Assert.AreEqual(0, parametersInfo.ParameterIndex);
            Assert.AreEqual(17, parametersInfo.SignatureEnd);

            parametersInfo = SignatureHelp.GetParametersInfoFromBuffer(ast, textBuffer.CurrentSnapshot, 11);
            Assert.AreEqual(1, parametersInfo.ParameterIndex);
            parametersInfo = SignatureHelp.GetParametersInfoFromBuffer(ast, textBuffer.CurrentSnapshot, 12);
            Assert.AreEqual(1, parametersInfo.ParameterIndex);

            parametersInfo = SignatureHelp.GetParametersInfoFromBuffer(ast, textBuffer.CurrentSnapshot, 13);
            Assert.AreEqual(2, parametersInfo.ParameterIndex);
            parametersInfo = SignatureHelp.GetParametersInfoFromBuffer(ast, textBuffer.CurrentSnapshot, 14);
            Assert.AreEqual(2, parametersInfo.ParameterIndex);

            parametersInfo = SignatureHelp.GetParametersInfoFromBuffer(ast, textBuffer.CurrentSnapshot, 15);
            Assert.AreEqual(3, parametersInfo.ParameterIndex);
            parametersInfo = SignatureHelp.GetParametersInfoFromBuffer(ast, textBuffer.CurrentSnapshot, 16);
            Assert.AreEqual(3, parametersInfo.ParameterIndex);
        }

        [TestMethod]
        public void ParameterTest2()
        {
            string content = @"x <- foo(,,,)";
            AstRoot ast = RParser.Parse(content);

            ITextBuffer textBuffer = new TextBufferMock(content, RContentTypeDefinition.ContentType);
            ParametersInfo parametersInfo = SignatureHelp.GetParametersInfoFromBuffer(ast, textBuffer.CurrentSnapshot, 9);

            Assert.IsNotNull(parametersInfo);
            Assert.IsNotNull(parametersInfo.FunctionCall);
            Assert.AreEqual("foo", parametersInfo.FunctionName);
            Assert.AreEqual(0, parametersInfo.ParameterIndex);

            parametersInfo = SignatureHelp.GetParametersInfoFromBuffer(ast, textBuffer.CurrentSnapshot, 10);
            Assert.AreEqual(1, parametersInfo.ParameterIndex);
            parametersInfo = SignatureHelp.GetParametersInfoFromBuffer(ast, textBuffer.CurrentSnapshot, 11);
            Assert.AreEqual(2, parametersInfo.ParameterIndex);
            parametersInfo = SignatureHelp.GetParametersInfoFromBuffer(ast, textBuffer.CurrentSnapshot, 12);
            Assert.AreEqual(3, parametersInfo.ParameterIndex);
        }

        [TestMethod]
        public void ParameterTest3()
        {
            string content = @"x <- foo(,, ";
            ParametersInfo parametersInfo;

            AstRoot ast = RParser.Parse(content);
            ITextBuffer textBuffer = new TextBufferMock(content, RContentTypeDefinition.ContentType);

            parametersInfo = SignatureHelp.GetParametersInfoFromBuffer(ast, textBuffer.CurrentSnapshot, 11);
            Assert.AreEqual(2, parametersInfo.ParameterIndex);
            parametersInfo = SignatureHelp.GetParametersInfoFromBuffer(ast, textBuffer.CurrentSnapshot, 12);
            Assert.AreEqual(2, parametersInfo.ParameterIndex);
        }
    }
}
