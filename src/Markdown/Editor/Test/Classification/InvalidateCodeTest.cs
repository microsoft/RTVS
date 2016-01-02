using System.Collections.Generic;
using Microsoft.Common.Core.Tests.Utility;
using Microsoft.Languages.Editor.Tests.Text;
using Microsoft.Languages.Editor.Tests.Utility;
using Microsoft.Markdown.Editor.Classification.MD;
using Microsoft.Markdown.Editor.ContentTypes;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.Editor.Mocks;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Xunit;

namespace Microsoft.Markdown.Editor.Tests.Classification {
    public class InvalidateCodeTest : UnitTestBase {
        [Fact]
        [Trait("Category", "Md.Classifier")]
        public void Markdown_InvalidateCodeTest() {
            string content = "```'{r}\n#R\n```";
            TextBufferMock textBuffer = new TextBufferMock(content, MdContentTypeDefinition.ContentType);

            MdClassifierProvider classifierProvider = new MdClassifierProvider();
            IClassifier cls = classifierProvider.GetClassifier(textBuffer);

            Typing.Type(textBuffer, 6, "\n");

            IList<ClassificationSpan> spans = cls.GetClassificationSpans(new SnapshotSpan(textBuffer.CurrentSnapshot, new Span(0, textBuffer.CurrentSnapshot.Length)));
            string actual = ClassificationWriter.WriteClassifications(spans);

            Assert.Equal("[0:15] Markdown Code", actual.TrimEnd());
        }
    }
}
