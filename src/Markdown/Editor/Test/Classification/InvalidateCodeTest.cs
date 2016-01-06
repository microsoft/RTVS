using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.Languages.Editor.Test.Text;
using Microsoft.Languages.Editor.Test.Utility;
using Microsoft.Markdown.Editor.Classification.MD;
using Microsoft.Markdown.Editor.ContentTypes;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.Editor.Mocks;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;

namespace Microsoft.Markdown.Editor.Test.Classification {
    [ExcludeFromCodeCoverage]
    public class InvalidateCodeTest {
        //[Test]
        //[Category.Md.Classifier]
        public void Markdown_InvalidateCodeTest() {
            string content = "```'{r}\n#R\n```";
            TextBufferMock textBuffer = new TextBufferMock(content, MdContentTypeDefinition.ContentType);

            MdClassifierProvider classifierProvider = new MdClassifierProvider();
            IClassifier cls = classifierProvider.GetClassifier(textBuffer);

            Typing.Type(textBuffer, 6, "\n");

            IList<ClassificationSpan> spans = cls.GetClassificationSpans(new SnapshotSpan(textBuffer.CurrentSnapshot, new Span(0, textBuffer.CurrentSnapshot.Length)));
            string actual = ClassificationWriter.WriteClassifications(spans);

            actual.TrimEnd().Should().Be("[0:15] Markdown Code");
        }
    }
}
