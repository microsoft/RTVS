using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Microsoft.Languages.Core.Test.Utility;
using Microsoft.Languages.Editor.Shell;
using Microsoft.Languages.Editor.Tests.Shell;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.Parser;
using Microsoft.R.Editor.ContentType;
using Microsoft.R.Editor.Signatures;
using Microsoft.R.Editor.Test.Mocks;
using Microsoft.R.Editor.Test.Utility;
using Microsoft.R.Editor.Tree;
using Microsoft.R.Support.Help.Functions;
using Microsoft.R.Support.Test.Utility;
using Microsoft.VisualStudio.Editor.Mocks;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.Text;

namespace Microsoft.R.Editor.Test.Signatures
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class ParameterTest : UnitTestBase
    {
        [TestMethod]
        public void ParameterTest01()
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
        public void ParameterTest02()
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
        public void ParameterTest03()
        {
            string content = @"x <- foo(,,";
            ParametersInfo parametersInfo;

            AstRoot ast = RParser.Parse(content);
            ITextBuffer textBuffer = new TextBufferMock(content, RContentTypeDefinition.ContentType);

            parametersInfo = SignatureHelp.GetParametersInfoFromBuffer(ast, textBuffer.CurrentSnapshot, 11);
            Assert.AreEqual(2, parametersInfo.ParameterIndex);
        }

        [TestMethod]
        public void ParameterTest04()
        {
            string content =
@"x <- foo(,, 

if(x > 1) {";
            ParametersInfo parametersInfo;

            AstRoot ast = RParser.Parse(content);
            ITextBuffer textBuffer = new TextBufferMock(content, RContentTypeDefinition.ContentType);

            parametersInfo = SignatureHelp.GetParametersInfoFromBuffer(ast, textBuffer.CurrentSnapshot, 11);
            Assert.AreEqual(2, parametersInfo.ParameterIndex);
            parametersInfo = SignatureHelp.GetParametersInfoFromBuffer(ast, textBuffer.CurrentSnapshot, 12);
            Assert.AreEqual(2, parametersInfo.ParameterIndex);
        }

        [TestMethod]
        public void ParameterTest05()
        {
            string content =
@"x <- abs(cos(


while";
            AstRoot ast = RParser.Parse(content);

            ITextBuffer textBuffer = new TextBufferMock(content, RContentTypeDefinition.ContentType);
            ParametersInfo parametersInfo = SignatureHelp.GetParametersInfoFromBuffer(ast, textBuffer.CurrentSnapshot, content.Length - 5);

            Assert.IsNotNull(parametersInfo);
            Assert.IsNotNull(parametersInfo.FunctionCall);
            Assert.AreEqual("cos", parametersInfo.FunctionName);
            Assert.AreEqual(0, parametersInfo.ParameterIndex);
            Assert.AreEqual(content.Length - 5, parametersInfo.SignatureEnd);
        }

        [TestMethod]
        public void ParameterTest06()
        {
            string content =
@"x <- abs(

function(a) {
";
            AstRoot ast = RParser.Parse(content);

            ITextBuffer textBuffer = new TextBufferMock(content, RContentTypeDefinition.ContentType);
            ParametersInfo parametersInfo = SignatureHelp.GetParametersInfoFromBuffer(ast, textBuffer.CurrentSnapshot, content.Length - 1);

            Assert.IsNotNull(parametersInfo);
            Assert.IsNotNull(parametersInfo.FunctionCall);
            Assert.AreEqual("abs", parametersInfo.FunctionName);
            Assert.AreEqual(0, parametersInfo.ParameterIndex);
            Assert.AreEqual(content.Length, parametersInfo.SignatureEnd);
        }

        [TestMethod]
        public void ParameterTest_ComputeCurrentParameter01()
        {
            ITextBuffer textBuffer = new TextBufferMock("aov(", RContentTypeDefinition.ContentType);
            SignatureHelpSource source = new SignatureHelpSource(textBuffer);
            SignatureHelpSessionMock session = new SignatureHelpSessionMock(textBuffer, 0);
            TextViewMock textView = session.TextView as TextViewMock;
            List<ISignature> signatures = new List<ISignature>();

            EditorTree tree = new EditorTree(textBuffer);
            tree.Build();
            var document = new EditorDocumentMock(tree);

            session.TrackingPoint = new TrackingPointMock(textBuffer, 4, PointTrackingMode.Positive, TrackingFidelityMode.Forward);
            FunctionIndexTestExecutor.ExecuteTest((ManualResetEventSlim completed) =>
            {
                object result = FunctionIndex.GetFunctionInfo("aov", (object o) =>
                {
                    tree.TakeThreadOwnerShip();
                    source.AugmentSignatureHelpSession(session, signatures, tree.AstRoot, (x) => { });

                    Assert.AreEqual(1, signatures.Count);

                    int index = GetCurrentParameterIndex(signatures[0] as SignatureHelp, signatures[0].CurrentParameter);
                    Assert.AreEqual(0, index);

                    textView.Caret = new TextCaretMock(textView, 5);
                    TextBufferUtility.ApplyTextChange(textBuffer, 4, 0, 1, "a");
                    index = GetCurrentParameterIndex(signatures[0] as SignatureHelp, signatures[0].CurrentParameter);
                    Assert.AreEqual(0, index);

                    textView.Caret = new TextCaretMock(textView, 6);
                    TextBufferUtility.ApplyTextChange(textBuffer, 5, 0, 1, ",");
                    index = GetCurrentParameterIndex(signatures[0] as SignatureHelp, signatures[0].CurrentParameter);
                    Assert.AreEqual(1, index);

                    textView.Caret = new TextCaretMock(textView, 7);
                    TextBufferUtility.ApplyTextChange(textBuffer, 6, 0, 1, ",");
                    index = GetCurrentParameterIndex(signatures[0] as SignatureHelp, signatures[0].CurrentParameter);
                    Assert.AreEqual(2, index);

                    completed.Set();
                 });
            });
        }

        private int GetCurrentParameterIndex(SignatureHelp sh, IParameter parameter)
        {
            for (int i = 0; i < sh.Parameters.Count; i++)
            {
                if(sh.Parameters[i] == parameter)
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
