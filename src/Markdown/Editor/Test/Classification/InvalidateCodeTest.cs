using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Languages.Core.Test.Utility;
using Microsoft.Languages.Editor.Shell;
using Microsoft.Languages.Editor.Test.Text;
using Microsoft.Languages.Editor.Test.Utility;
using Microsoft.Languages.Editor.Tests.Shell;
using Microsoft.Markdown.Editor.Classification;
using Microsoft.Markdown.Editor.Classification.MD;
using Microsoft.Markdown.Editor.ContentTypes;
using Microsoft.Markdown.Editor.Test.Utility;
using Microsoft.VisualStudio.Editor.Mocks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;

namespace Microsoft.Markdown.Editor.Test.Classification
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class InvalidateCodeTest : UnitTestBase
    {
        [TestMethod]
        public void Markdown_InvalidateCodeTest()
        {
            string content = "```'{r}\n#R\n```";
            TextBufferMock textBuffer = new TextBufferMock(content, MdContentTypeDefinition.ContentType);

            EditorShell.SetShell(TestEditorShell.Create(MarkdownTestCompositionCatalog.Current));
            MdClassifierProvider classifierProvider = new MdClassifierProvider();
            IClassifier cls = classifierProvider.GetClassifier(textBuffer);

            Typing.Type(textBuffer, 6, "\n");

            IList<ClassificationSpan> spans = cls.GetClassificationSpans(new SnapshotSpan(textBuffer.CurrentSnapshot, new Span(0, textBuffer.CurrentSnapshot.Length)));
            string actual = ClassificationWriter.WriteClassifications(spans);

            Assert.AreEqual("[0:15] Markdown Code", actual.TrimEnd());
        }
    }
}
