using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Microsoft.Common.Core.Test.Utility;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.Parser;
using Microsoft.R.Editor.ContentType;
using Microsoft.R.Editor.Signatures;
using Microsoft.R.Support.Test.Utility;
using Microsoft.VisualStudio.Editor.Mocks;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.Text;

namespace Microsoft.R.Editor.Test.Signatures {
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class SignatureHelpSourceTest : UnitTestBase {
        [TestMethod]
        [TestCategory("R.Signatures")]
        public void SignatureHelpSourceTest01() {
            string content = @"x <- as.matrix(x)";
            AstRoot ast = RParser.Parse(content);

            FunctionIndexTestExecutor.ExecuteTest((ManualResetEventSlim evt) => {
                int caretPosition = 15;
                ITextBuffer textBuffer = new TextBufferMock(content, RContentTypeDefinition.ContentType);
                SignatureHelpSource signatureHelpSource = new SignatureHelpSource(textBuffer);
                SignatureHelpSessionMock signatureHelpSession = new SignatureHelpSessionMock(textBuffer, caretPosition);
                List<ISignature> signatures = new List<ISignature>();

                signatureHelpSession.TrackingPoint = new TrackingPointMock(textBuffer, caretPosition, PointTrackingMode.Positive, TrackingFidelityMode.Forward);
                bool ready = signatureHelpSource.AugmentSignatureHelpSession(signatureHelpSession, signatures, ast, (object o) => {
                    signatureHelpSource.AugmentSignatureHelpSession(signatureHelpSession, signatures, ast, null);
                    SignatureHelpSourceTest01_TestBody(signatures, evt);
                });

                if (ready && !evt.IsSet) {
                    SignatureHelpSourceTest01_TestBody(signatures, evt);
                }
            });
        }

        private void SignatureHelpSourceTest01_TestBody(List<ISignature> signatures, ManualResetEventSlim completedEvent) {
            Assert.AreEqual(1, signatures.Count);
            Assert.AreEqual(2, signatures[0].Parameters.Count);

            Assert.AreEqual("x", signatures[0].CurrentParameter.Name);
            Assert.AreEqual("as.matrix(x, ...)", signatures[0].Content);
            Assert.IsFalse(string.IsNullOrEmpty(signatures[0].Documentation));

            completedEvent.Set();
        }
    }
}
