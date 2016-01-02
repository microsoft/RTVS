using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Microsoft.Common.Core.Test.Utility;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.Parser;
using Microsoft.R.Editor.ContentType;
using Microsoft.R.Editor.Settings;
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

namespace Microsoft.R.Editor.Test.Signatures {
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class ParameterTest : UnitTestBase {
        [TestMethod]
        [TestCategory("R.Signatures")]
        public void ParameterTest01() {
            string content = @"x <- foo(a,b,c,d)";
            AstRoot ast = RParser.Parse(content);

            ITextBuffer textBuffer = new TextBufferMock(content, RContentTypeDefinition.ContentType);
            ParameterInfo parametersInfo = SignatureHelp.GetParametersInfoFromBuffer(ast, textBuffer.CurrentSnapshot, 10);

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
        [TestCategory("R.Signatures")]
        public void ParameterTest02() {
            string content = @"x <- foo(,,,)";
            AstRoot ast = RParser.Parse(content);

            ITextBuffer textBuffer = new TextBufferMock(content, RContentTypeDefinition.ContentType);
            ParameterInfo parametersInfo = SignatureHelp.GetParametersInfoFromBuffer(ast, textBuffer.CurrentSnapshot, 9);

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
        [TestCategory("R.Signatures")]
        public void ParameterTest03() {
            string content = @"x <- foo(,,";
            ParameterInfo parametersInfo;

            AstRoot ast = RParser.Parse(content);
            ITextBuffer textBuffer = new TextBufferMock(content, RContentTypeDefinition.ContentType);

            parametersInfo = SignatureHelp.GetParametersInfoFromBuffer(ast, textBuffer.CurrentSnapshot, 11);
            Assert.AreEqual(2, parametersInfo.ParameterIndex);
        }

        [TestMethod]
        [TestCategory("R.Signatures")]
        public void ParameterTest04() {
            string content =
@"x <- foo(,, 

if(x > 1) {";
            ParameterInfo parametersInfo;

            AstRoot ast = RParser.Parse(content);
            ITextBuffer textBuffer = new TextBufferMock(content, RContentTypeDefinition.ContentType);

            parametersInfo = SignatureHelp.GetParametersInfoFromBuffer(ast, textBuffer.CurrentSnapshot, 11);
            Assert.AreEqual(2, parametersInfo.ParameterIndex);
            parametersInfo = SignatureHelp.GetParametersInfoFromBuffer(ast, textBuffer.CurrentSnapshot, 12);
            Assert.AreEqual(2, parametersInfo.ParameterIndex);
        }

        [TestMethod]
        [TestCategory("R.Signatures")]
        public void ParameterTest05() {
            string content =
@"x <- abs(cos(


while";
            AstRoot ast = RParser.Parse(content);

            ITextBuffer textBuffer = new TextBufferMock(content, RContentTypeDefinition.ContentType);
            ParameterInfo parametersInfo = SignatureHelp.GetParametersInfoFromBuffer(ast, textBuffer.CurrentSnapshot, content.Length - 5);

            Assert.IsNotNull(parametersInfo);
            Assert.IsNotNull(parametersInfo.FunctionCall);
            Assert.AreEqual("cos", parametersInfo.FunctionName);
            Assert.AreEqual(0, parametersInfo.ParameterIndex);
            Assert.AreEqual(content.Length - 5, parametersInfo.SignatureEnd);
        }

        [TestMethod]
        [TestCategory("R.Signatures")]
        public void ParameterTest06() {
            string content =
@"x <- abs(

function(a) {
";
            AstRoot ast = RParser.Parse(content);

            ITextBuffer textBuffer = new TextBufferMock(content, RContentTypeDefinition.ContentType);
            ParameterInfo parametersInfo = SignatureHelp.GetParametersInfoFromBuffer(ast, textBuffer.CurrentSnapshot, content.Length - 1);

            Assert.IsNotNull(parametersInfo);
            Assert.IsNotNull(parametersInfo.FunctionCall);
            Assert.AreEqual("abs", parametersInfo.FunctionName);
            Assert.AreEqual(0, parametersInfo.ParameterIndex);
            Assert.AreEqual(content.Length, parametersInfo.SignatureEnd);
        }

        [TestMethod]
        [TestCategory("R.Signatures")]
        public void ParameterTest_ComputeCurrentParameter01() {
            ITextBuffer textBuffer = new TextBufferMock("aov(", RContentTypeDefinition.ContentType);
            SignatureHelpSource source = new SignatureHelpSource(textBuffer);
            SignatureHelpSessionMock session = new SignatureHelpSessionMock(textBuffer, 0);
            TextViewMock textView = session.TextView as TextViewMock;
            List<ISignature> signatures = new List<ISignature>();

            EditorTree tree = new EditorTree(textBuffer);
            tree.Build();
            var document = new EditorDocumentMock(tree);

            session.TrackingPoint = new TrackingPointMock(textBuffer, 4, PointTrackingMode.Positive, TrackingFidelityMode.Forward);
            FunctionIndexTestExecutor.ExecuteTest((ManualResetEventSlim completed) => {
                object result = FunctionIndex.GetFunctionInfo("aov", (object o) => {
                    ComputeCurrentParameter01_Body(completed);
                });

                if (result != null && !completed.IsSet) {
                    ComputeCurrentParameter01_Body(completed);
                }

            });
        }

        private void ComputeCurrentParameter01_Body(ManualResetEventSlim completed) {
            ITextBuffer textBuffer = new TextBufferMock("aov(", RContentTypeDefinition.ContentType);
            SignatureHelpSource source = new SignatureHelpSource(textBuffer);
            SignatureHelpSessionMock session = new SignatureHelpSessionMock(textBuffer, 0);
            TextViewMock textView = session.TextView as TextViewMock;
            List<ISignature> signatures = new List<ISignature>();

            EditorTree tree = new EditorTree(textBuffer);
            tree.Build();
            var document = new EditorDocumentMock(tree);

            session.TrackingPoint = new TrackingPointMock(textBuffer, 4, PointTrackingMode.Positive, TrackingFidelityMode.Forward);

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
        }

        private int GetCurrentParameterIndex(SignatureHelp sh, IParameter parameter) {
            for (int i = 0; i < sh.Parameters.Count; i++) {
                if (sh.Parameters[i] == parameter) {
                    return i;
                }
            }

            return -1;
        }

        [TestMethod]
        [TestCategory("R.Signatures")]
        public void ParameterTest_ComputeCurrentParameter02() {
            FunctionIndexTestExecutor.ExecuteTest((ManualResetEventSlim completed) => {
                object result = FunctionIndex.GetFunctionInfo("legend", (object o) => {
                    ComputeCurrentParameter02_Body(completed);
                });

                if (result != null && !completed.IsSet) {
                    ComputeCurrentParameter02_Body(completed);
                }
            });
        }

        private void ComputeCurrentParameter02_Body(ManualResetEventSlim completed) {
            REditorSettings.PartialArgumentNameMatch = true;

            ITextBuffer textBuffer = new TextBufferMock("legend(bty=1, lt=3)", RContentTypeDefinition.ContentType);
            SignatureHelpSource source = new SignatureHelpSource(textBuffer);
            SignatureHelpSessionMock session = new SignatureHelpSessionMock(textBuffer, 0);
            TextViewMock textView = session.TextView as TextViewMock;
            List<ISignature> signatures = new List<ISignature>();

            EditorTree tree = new EditorTree(textBuffer);
            tree.Build();
            var document = new EditorDocumentMock(tree);

            session.TrackingPoint = new TrackingPointMock(textBuffer, 7, PointTrackingMode.Positive, TrackingFidelityMode.Forward);

            tree.TakeThreadOwnerShip();
            source.AugmentSignatureHelpSession(session, signatures, tree.AstRoot, (x) => { });

            Assert.AreEqual(1, signatures.Count);

            textView.Caret = new TextCaretMock(textView, 8);
            SignatureHelp sh = signatures[0] as SignatureHelp;
            int index = sh.ComputeCurrentParameter(tree.TextSnapshot, tree.AstRoot, 8);
            Assert.AreEqual(11, index);

            textView.Caret = new TextCaretMock(textView, 15);
            index = sh.ComputeCurrentParameter(tree.TextSnapshot, tree.AstRoot, 15);
            Assert.AreEqual(6, index);

            completed.Set();
        }

        [TestMethod]
        [TestCategory("R.Signatures")]
        public void ParameterTest_ComputeCurrentParameter03() {
            FunctionIndexTestExecutor.ExecuteTest((ManualResetEventSlim completed) => {
                object result = FunctionIndex.GetFunctionInfo("legend", (object o) => {
                    ComputeCurrentParameter03_Body(completed);
                });

                if (result != null && !completed.IsSet) {
                    ComputeCurrentParameter03_Body(completed);
                }
            });
        }

        private void ComputeCurrentParameter03_Body(ManualResetEventSlim completed) {
            REditorSettings.PartialArgumentNameMatch = false;

            ITextBuffer textBuffer = new TextBufferMock("legend(an=1)", RContentTypeDefinition.ContentType);
            SignatureHelpSource source = new SignatureHelpSource(textBuffer);
            SignatureHelpSessionMock session = new SignatureHelpSessionMock(textBuffer, 0);
            TextViewMock textView = session.TextView as TextViewMock;
            List<ISignature> signatures = new List<ISignature>();

            EditorTree tree = new EditorTree(textBuffer);
            tree.Build();
            var document = new EditorDocumentMock(tree);

            session.TrackingPoint = new TrackingPointMock(textBuffer, 7, PointTrackingMode.Positive, TrackingFidelityMode.Forward);

            tree.TakeThreadOwnerShip();
            source.AugmentSignatureHelpSession(session, signatures, tree.AstRoot, (x) => { });

            Assert.AreEqual(1, signatures.Count);

            textView.Caret = new TextCaretMock(textView, 8);
            SignatureHelp sh = signatures[0] as SignatureHelp;
            int index = sh.ComputeCurrentParameter(tree.TextSnapshot, tree.AstRoot, 8);
            Assert.AreEqual(0, index);

            completed.Set();
        }

        [TestMethod]
        [TestCategory("R.Signatures")]
        public void ParameterTest_ComputeCurrentParameter04() {
            FunctionIndexTestExecutor.ExecuteTest((ManualResetEventSlim completed) => {
                object result = FunctionIndex.GetFunctionInfo("legend", (object o) => {
                    ComputeCurrentParameter04_Body(completed);
                });

                if (result != null && !completed.IsSet) {
                    ComputeCurrentParameter04_Body(completed);
                }
            });
        }

        private void ComputeCurrentParameter04_Body(ManualResetEventSlim completed) {
            REditorSettings.PartialArgumentNameMatch = true;

            ITextBuffer textBuffer = new TextBufferMock("legend(an=1)", RContentTypeDefinition.ContentType);
            SignatureHelpSource source = new SignatureHelpSource(textBuffer);
            SignatureHelpSessionMock session = new SignatureHelpSessionMock(textBuffer, 0);
            TextViewMock textView = session.TextView as TextViewMock;
            List<ISignature> signatures = new List<ISignature>();

            EditorTree tree = new EditorTree(textBuffer);
            tree.Build();
            var document = new EditorDocumentMock(tree);

            session.TrackingPoint = new TrackingPointMock(textBuffer, 7, PointTrackingMode.Positive, TrackingFidelityMode.Forward);

            tree.TakeThreadOwnerShip();
            source.AugmentSignatureHelpSession(session, signatures, tree.AstRoot, (x) => { });

            Assert.AreEqual(1, signatures.Count);

            textView.Caret = new TextCaretMock(textView, 8);
            SignatureHelp sh = signatures[0] as SignatureHelp;
            int index = sh.ComputeCurrentParameter(tree.TextSnapshot, tree.AstRoot, 8);
            Assert.AreEqual(9, index);

            completed.Set();
        }
    }
}
