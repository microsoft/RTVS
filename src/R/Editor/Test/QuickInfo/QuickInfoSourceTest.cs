using System.Collections.Generic;
using Microsoft.Languages.Core.Test.Utility;
using Microsoft.Languages.Editor.Test.Mocks;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.Parser;
using Microsoft.R.Editor.ContentType;
using Microsoft.R.Editor.QuickInfo;
using Microsoft.R.Editor.Signatures;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.Text;

namespace Microsoft.R.Editor.Test.Signatures
{
    [TestClass]
    public class QuickInfoSourceTest : UnitTestBase
    {
        //[TestMethod]
        public void QuickInfoSourceTest1()
        {
            string content = @"x <- as.matrix(x)";
            AstRoot ast = RParser.Parse(content);

            ITextBuffer textBuffer = new TextBufferMock(content, RContentTypeDefinition.ContentType);
            QuickInfoSource quickInfoSource = new QuickInfoSource(textBuffer);
            QuickInfoSessionMock quickInfoSession = new QuickInfoSessionMock();
            List<object> quickInfoContent = new List<object>();
            ITrackingSpan applicableSpan;

            quickInfoSession.TriggerPoint = new SnapshotPoint(textBuffer.CurrentSnapshot, 6);
            quickInfoSource.AugmentQuickInfoSession(quickInfoSession, quickInfoContent, out applicableSpan);
            ParametersInfo parametersInfo = SignatureHelp.GetParametersInfoFromBuffer(ast, textBuffer.CurrentSnapshot, 10);

            Assert.IsNotNull(applicableSpan);
            Assert.AreEqual(1, quickInfoContent.Count);
        }
    }
}
