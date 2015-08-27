using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Languages.Core.Test.Utility;
using Microsoft.Languages.Editor.Test.Mocks;
using Microsoft.Languages.Editor.Test.Utility;
using Microsoft.R.Support.RD.Classification;
using Microsoft.R.Support.RD.ContentTypes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;

namespace Microsoft.R.Support.Test.RD.Classification
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class RdClassifierTest: UnitTestBase
    {
        [TestMethod]
        public void ClassifyRContent()
        {
            string expected1 =
@"[0:9] keyword
[9:1] RD Braces
[16:2] number
[19:1] number
[32:5] string";

            string expected2 =
@"[0:9] keyword
[9:1] RD Braces
[16:2] number
[19:1] number
[32:6] string";

            string s1 = "\\examples{ x <- -9:9 plot(col = \"";
            string s2 = "red\")";
            string original = s1 + s2;

            TextBufferMock textBuffer = new TextBufferMock(original, RdContentTypeDefinition.ContentType);
            ClassificationTypeRegistryServiceMock ctrs = new ClassificationTypeRegistryServiceMock();
            RdClassifier cls = new RdClassifier(textBuffer, ctrs);

            IList<ClassificationSpan> spans = cls.GetClassificationSpans(new SnapshotSpan(textBuffer.CurrentSnapshot, new Span(0, textBuffer.CurrentSnapshot.Length)));
            string actual = ClassificationWriter.WriteClassifications(spans);
            BaselineCompare.CompareStringLines(expected1, actual);

            textBuffer.Insert(s1.Length, "%");
            spans = cls.GetClassificationSpans(new SnapshotSpan(textBuffer.CurrentSnapshot, new Span(0, textBuffer.CurrentSnapshot.Length)));
            actual = ClassificationWriter.WriteClassifications(spans);
            BaselineCompare.CompareStringLines(expected2, actual);

            textBuffer.Delete(new Span(s1.Length, 1));
            spans = cls.GetClassificationSpans(new SnapshotSpan(textBuffer.CurrentSnapshot, new Span(0, textBuffer.CurrentSnapshot.Length)));
            actual = ClassificationWriter.WriteClassifications(spans);
            BaselineCompare.CompareStringLines(expected1, actual);
        }
    }
}
