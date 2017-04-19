// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Common.Core.Test.Utility;
using Microsoft.Languages.Editor.Test.Utility;
using Microsoft.R.Editor.RData.Classification;
using Microsoft.R.Editor.RData.ContentTypes;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.Editor.Mocks;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;

namespace Microsoft.R.Editor.RData.Test.Classification {
    [ExcludeFromCodeCoverage]
    public class RdClassifierTest {
        [Test]
        [Category.R.Classifier]
        public void ClassifyRContent() {
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
