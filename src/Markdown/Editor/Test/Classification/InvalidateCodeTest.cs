// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.Languages.Core.Classification;
using Microsoft.Languages.Editor.Composition;
using Microsoft.Languages.Editor.Shell;
using Microsoft.Languages.Editor.Test.Text;
using Microsoft.Languages.Editor.Test.Utility;
using Microsoft.Markdown.Editor.Classification.MD;
using Microsoft.Markdown.Editor.ContentTypes;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.Editor.Mocks;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.Markdown.Editor.Test.Classification {
    [ExcludeFromCodeCoverage]
    public class InvalidateCodeTest {
        [Test]
        [Category.Md.Classifier]
        public void Markdown_InvalidateCodeTest() {
            string content = "```{r}#R\n```";
            var factory = EditorShell.Current.ExportProvider.GetExportedValue<ITextBufferFactoryService>();
            ITextBuffer textBuffer = factory.CreateTextBuffer(new ContentTypeMock(MdContentTypeDefinition.ContentType));
            textBuffer.Insert(0, content);

            var crs = EditorShell.Current.ExportProvider.GetExportedValue<IClassificationTypeRegistryService>();
            var ctrs = EditorShell.Current.ExportProvider.GetExportedValue<IContentTypeRegistryService>();
            var cnp = EditorShell.Current.ExportProvider.GetExports<IClassificationNameProvider, IComponentContentTypes>();

            MdClassifierProvider classifierProvider = new MdClassifierProvider(crs, ctrs, cnp);
            IClassifier cls = classifierProvider.GetClassifier(textBuffer);

            string actual = GetSpans(cls, textBuffer);
            actual.TrimEnd().Should().Be(
@"[0:3] Markdown Code
[6:2] comment
[9:3] Markdown Code");

            Typing.Type(textBuffer, 6, "\n");
            actual = GetSpans(cls, textBuffer);
            actual.TrimEnd().Should().Be(
@"[0:3] Markdown Code
[7:2] comment
[10:3] Markdown Code");

            Typing.Delete(textBuffer, 4, 1);
            actual = GetSpans(cls, textBuffer);
            actual.TrimEnd().Should().Be(
@"[0:3] Markdown Code
[4:5] Markdown Code
[9:3] Markdown Code");

            Typing.Type(textBuffer, 4, "R");
            actual = GetSpans(cls, textBuffer);
            actual.TrimEnd().Should().Be(
@"[0:3] Markdown Code
[7:2] comment
[10:3] Markdown Code");
        }

        private string GetSpans(IClassifier cls, ITextBuffer textBuffer) {
            IList<ClassificationSpan> spans = cls.GetClassificationSpans(new SnapshotSpan(textBuffer.CurrentSnapshot, new Span(0, textBuffer.CurrentSnapshot.Length)));
            return ClassificationWriter.WriteClassifications(spans);
        }
    }
}
