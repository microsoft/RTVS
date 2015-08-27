using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Microsoft.Languages.Core.Test.Utility;
using Microsoft.Languages.Editor.Test.Mocks;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.Parser;
using Microsoft.R.Editor.ContentType;
using Microsoft.R.Editor.QuickInfo;
using Microsoft.R.Editor.Signatures;
using Microsoft.R.Support.Test.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.Text;

namespace Microsoft.R.Editor.Test.Signatures
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class QuickInfoSourceTest : UnitTestBase
    {
        [TestMethod]
        public void QuickInfoSourceTest01()
        {
            string content = @"x <- as.matrix(x)";
            AstRoot ast = RParser.Parse(content);

            FunctionIndexTestExecutor.ExecuteTest((ManualResetEventSlim evt) =>
            {
                int caretPosition = 6;
                ITextBuffer textBuffer = new TextBufferMock(content, RContentTypeDefinition.ContentType);
                QuickInfoSource quickInfoSource = new QuickInfoSource(textBuffer);
                QuickInfoSessionMock quickInfoSession = new QuickInfoSessionMock(textBuffer, caretPosition);
                List<object> quickInfoContent = new List<object>();
                ITrackingSpan applicableSpan = null;

                quickInfoSession.TriggerPoint = new SnapshotPoint(textBuffer.CurrentSnapshot, caretPosition);
                bool ready = quickInfoSource.AugmentQuickInfoSession(ast, caretPosition, quickInfoSession, quickInfoContent, out applicableSpan,
                    (object o) =>
                    {
                        quickInfoSource.AugmentQuickInfoSession(ast, caretPosition, quickInfoSession,
                                                                quickInfoContent, out applicableSpan, null);

                        QuickInfoSourceTest01_TestBody(applicableSpan, quickInfoContent, ast, textBuffer, evt);
                    });

                if (ready && !evt.IsSet)
                {
                    QuickInfoSourceTest01_TestBody(applicableSpan, quickInfoContent, ast, textBuffer, evt);
                }
            });
        }

        private void QuickInfoSourceTest01_TestBody(ITrackingSpan applicableSpan, List<object> quickInfoContent, AstRoot ast, ITextBuffer textBuffer, ManualResetEventSlim completedEvent)
        {
            ParametersInfo parametersInfo = SignatureHelp.GetParametersInfoFromBuffer(ast, textBuffer.CurrentSnapshot, 10);

            Assert.IsNotNull(applicableSpan);
            Assert.AreEqual(1, quickInfoContent.Count);
            Assert.IsTrue(quickInfoContent[0].ToString().StartsWith("as.matrix(data = NA, nrow = 1, ncol = 1", StringComparison.Ordinal));

            completedEvent.Set();
        }
    }
}
