// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.Common.Core.Shell;
using Microsoft.Languages.Core.Classification;
using Microsoft.Languages.Editor.Composition;
using Microsoft.Languages.Editor.Test.Text;
using Microsoft.Languages.Editor.Test.Utility;
using Microsoft.Markdown.Editor.Classification.MD;
using Microsoft.Markdown.Editor.ContentTypes;
using Microsoft.UnitTests.Core.Mef;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.Editor.Mocks;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.Markdown.Editor.Test.Classification {
    [ExcludeFromCodeCoverage]
    public class MarkdownCodeTest {
        private readonly IExportProvider _exportProvider;
        private readonly ITextBufferFactoryService _tbfs;
        private readonly IClassificationTypeRegistryService _crs;
        private readonly IContentTypeRegistryService _ctrs;
        private readonly IEnumerable<Lazy<IClassificationNameProvider, IComponentContentTypes>> _cnp;

        public MarkdownCodeTest(IExportProvider exportProvider) {
            _exportProvider = exportProvider;
            _crs = _exportProvider.GetExportedValue<IClassificationTypeRegistryService>();
            _ctrs = _exportProvider.GetExportedValue<IContentTypeRegistryService>();
            _cnp = _exportProvider.GetExports<IClassificationNameProvider, IComponentContentTypes>();
            _tbfs = _exportProvider.GetExportedValue<ITextBufferFactoryService>();
        }

        [Test]
        [Category.Md.RCode]
        public void EditRCode01() {
            string content = "```{r}\n#R\n```";
            ITextBuffer textBuffer;
            IClassifier cls = GetClassifier(content, out textBuffer);

            string actual = GetSpans(cls, textBuffer);
            actual.TrimEnd().Should().Be(string.Empty);

            Typing.Type(textBuffer, 6, "\n");
            actual = GetSpans(cls, textBuffer);
            actual.TrimEnd().Should().Be(string.Empty);

            Typing.Delete(textBuffer, 4, 1);
            actual = GetSpans(cls, textBuffer);
            actual.TrimEnd().Should().Be("[0:13] Markdown Monospace");

            Typing.Type(textBuffer, 4, "R");
            actual = GetSpans(cls, textBuffer);
            actual.TrimEnd().Should().Be(string.Empty);
        }

        [Test]
        [Category.Md.RCode]
        public void EditRCode02() {
            string content = "```{r}\nx <- 1\n```";
            ITextBuffer textBuffer;
            IClassifier cls = GetClassifier(content, out textBuffer);

            Typing.Delete(textBuffer, 0, 1);
            var actual = GetSpans(cls, textBuffer);
            actual.TrimEnd().Should().Be("[0:2] Markdown Monospace");

            Typing.Type(textBuffer, 1, "`");
            actual = GetSpans(cls, textBuffer);
            actual.TrimEnd().Should().Be(string.Empty);
        }

        private IClassifier GetClassifier(string content, out ITextBuffer textBuffer) {
            textBuffer = _tbfs.CreateTextBuffer(new ContentTypeMock(MdContentTypeDefinition.ContentType));
            textBuffer.Insert(0, content);

            MdClassifierProvider classifierProvider = new MdClassifierProvider(_crs, _ctrs, _cnp, _exportProvider.GetExportedValue<ICoreShell>());
           return classifierProvider.GetClassifier(textBuffer);
        }

        private string GetSpans(IClassifier cls, ITextBuffer textBuffer) {
            IList<ClassificationSpan> spans = cls.GetClassificationSpans(new SnapshotSpan(textBuffer.CurrentSnapshot, new Span(0, textBuffer.CurrentSnapshot.Length)));
            return ClassificationWriter.WriteClassifications(spans);
        }
    }
}
