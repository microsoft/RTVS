using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using FluentAssertions;
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
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.Editor.Mocks;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;

namespace Microsoft.R.Editor.Test.Signatures {
    [ExcludeFromCodeCoverage]
    [Category.R.Signatures]
    public class ParameterTest {
        [Test]
        public void ParameterTest01() {
            string content = @"x <- foo(a,b,c,d)";
            AstRoot ast = RParser.Parse(content);

            ITextBuffer textBuffer = new TextBufferMock(content, RContentTypeDefinition.ContentType);
            ParameterInfo parametersInfo = SignatureHelp.GetParametersInfoFromBuffer(ast, textBuffer.CurrentSnapshot, 10);

            parametersInfo.Should().NotBeNull()
                .And.HaveFunctionCall()
                .And.HaveFunctionName("foo")
                .And.HaveParameterIndex(0)
                .And.HaveSignatureEnd(17);

            parametersInfo = SignatureHelp.GetParametersInfoFromBuffer(ast, textBuffer.CurrentSnapshot, 11);
            parametersInfo.Should().HaveParameterIndex(1);
            parametersInfo = SignatureHelp.GetParametersInfoFromBuffer(ast, textBuffer.CurrentSnapshot, 12);
            parametersInfo.Should().HaveParameterIndex(1);

            parametersInfo = SignatureHelp.GetParametersInfoFromBuffer(ast, textBuffer.CurrentSnapshot, 13);
            parametersInfo.Should().HaveParameterIndex(2);
            parametersInfo = SignatureHelp.GetParametersInfoFromBuffer(ast, textBuffer.CurrentSnapshot, 14);
            parametersInfo.Should().HaveParameterIndex(2);

            parametersInfo = SignatureHelp.GetParametersInfoFromBuffer(ast, textBuffer.CurrentSnapshot, 15);
            parametersInfo.Should().HaveParameterIndex(3);
            parametersInfo = SignatureHelp.GetParametersInfoFromBuffer(ast, textBuffer.CurrentSnapshot, 16);
            parametersInfo.Should().HaveParameterIndex(3);
        }

        [Test]
        public void ParameterTest02() {
            string content = @"x <- foo(,,,)";
            AstRoot ast = RParser.Parse(content);

            ITextBuffer textBuffer = new TextBufferMock(content, RContentTypeDefinition.ContentType);
            ParameterInfo parametersInfo = SignatureHelp.GetParametersInfoFromBuffer(ast, textBuffer.CurrentSnapshot, 9);

            parametersInfo.Should().NotBeNull()
                .And.HaveFunctionCall()
                .And.HaveFunctionName("foo")
                .And.HaveParameterIndex(0);

            parametersInfo = SignatureHelp.GetParametersInfoFromBuffer(ast, textBuffer.CurrentSnapshot, 10);
            parametersInfo.Should().HaveParameterIndex(1);
            parametersInfo = SignatureHelp.GetParametersInfoFromBuffer(ast, textBuffer.CurrentSnapshot, 11);
            parametersInfo.Should().HaveParameterIndex(2);
            parametersInfo = SignatureHelp.GetParametersInfoFromBuffer(ast, textBuffer.CurrentSnapshot, 12);
            parametersInfo.Should().HaveParameterIndex(3);
        }

        [Test]
        public void ParameterTest03() {
            string content = @"x <- foo(,,";

            AstRoot ast = RParser.Parse(content);
            ITextBuffer textBuffer = new TextBufferMock(content, RContentTypeDefinition.ContentType);

            var parametersInfo = SignatureHelp.GetParametersInfoFromBuffer(ast, textBuffer.CurrentSnapshot, 11);
            parametersInfo.Should().HaveParameterIndex(2);
        }

        [Test]
        public void ParameterTest04() {
            string content =
@"x <- foo(,, 

if(x > 1) {";
            ParameterInfo parametersInfo;

            AstRoot ast = RParser.Parse(content);
            ITextBuffer textBuffer = new TextBufferMock(content, RContentTypeDefinition.ContentType);

            parametersInfo = SignatureHelp.GetParametersInfoFromBuffer(ast, textBuffer.CurrentSnapshot, 11);
            parametersInfo.Should().HaveParameterIndex(2);
            parametersInfo = SignatureHelp.GetParametersInfoFromBuffer(ast, textBuffer.CurrentSnapshot, 12);
            parametersInfo.Should().HaveParameterIndex(2);
        }

        [Test]
        public void ParameterTest05() {
            string content =
@"x <- abs(cos(


while";
            AstRoot ast = RParser.Parse(content);

            ITextBuffer textBuffer = new TextBufferMock(content, RContentTypeDefinition.ContentType);
            ParameterInfo parametersInfo = SignatureHelp.GetParametersInfoFromBuffer(ast, textBuffer.CurrentSnapshot, content.Length - 5);

            parametersInfo.Should().NotBeNull()
                .And.HaveFunctionCall()
                .And.HaveFunctionName("cos")
                .And.HaveParameterIndex(0)
                .And.HaveSignatureEnd(content.Length - 5);
        }

        [Test]
        public void ParameterTest06() {
            string content =
@"x <- abs(

function(a) {
";
            AstRoot ast = RParser.Parse(content);

            ITextBuffer textBuffer = new TextBufferMock(content, RContentTypeDefinition.ContentType);
            ParameterInfo parametersInfo = SignatureHelp.GetParametersInfoFromBuffer(ast, textBuffer.CurrentSnapshot, content.Length - 1);

            parametersInfo.Should().NotBeNull()
                .And.HaveFunctionCall()
                .And.HaveFunctionName("abs")
                .And.HaveParameterIndex(0)
                .And.HaveSignatureEnd(content.Length);
        }

        [Test(Skip = "Need to understand how test is working")]
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

            signatures.Should().ContainSingle();

            int index = GetCurrentParameterIndex(signatures[0] as SignatureHelp, signatures[0].CurrentParameter);
            index.Should().Be(0);

            textView.Caret = new TextCaretMock(textView, 5);
            TextBufferUtility.ApplyTextChange(textBuffer, 4, 0, 1, "a");
            index = GetCurrentParameterIndex(signatures[0] as SignatureHelp, signatures[0].CurrentParameter);
            index.Should().Be(0);

            textView.Caret = new TextCaretMock(textView, 6);
            TextBufferUtility.ApplyTextChange(textBuffer, 5, 0, 1, ",");
            index = GetCurrentParameterIndex(signatures[0] as SignatureHelp, signatures[0].CurrentParameter);
            index.Should().Be(1);

            textView.Caret = new TextCaretMock(textView, 7);
            TextBufferUtility.ApplyTextChange(textBuffer, 6, 0, 1, ",");
            index = GetCurrentParameterIndex(signatures[0] as SignatureHelp, signatures[0].CurrentParameter);
            index.Should().Be(2);

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

        [Test(Skip = "Need to understand how test is working")]
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

            signatures.Should().ContainSingle();

            textView.Caret = new TextCaretMock(textView, 8);
            SignatureHelp sh = signatures[0] as SignatureHelp;
            int index = sh.ComputeCurrentParameter(tree.TextSnapshot, tree.AstRoot, 8);
            index.Should().Be(11);

            textView.Caret = new TextCaretMock(textView, 15);
            index = sh.ComputeCurrentParameter(tree.TextSnapshot, tree.AstRoot, 15);
            index.Should().Be(6);

            completed.Set();
        }

        [Test(Skip = "Need to understand how test is working")]
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

            signatures.Should().ContainSingle();

            textView.Caret = new TextCaretMock(textView, 8);
            SignatureHelp sh = signatures[0] as SignatureHelp;
            int index = sh.ComputeCurrentParameter(tree.TextSnapshot, tree.AstRoot, 8);
            index.Should().Be(0);

            completed.Set();
        }

        [Test(Skip = "Need to understand how test is working")]
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

            signatures.Should().ContainSingle();

            textView.Caret = new TextCaretMock(textView, 8);
            SignatureHelp sh = signatures[0] as SignatureHelp;
            int index = sh.ComputeCurrentParameter(tree.TextSnapshot, tree.AstRoot, 8);
            index.Should().Be(9);

            completed.Set();
        }
    }
}
