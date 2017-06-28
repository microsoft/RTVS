// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Shell;
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
    [Category.Md.RCode]
    public sealed class MarkdownCodeTest {
        private readonly ITextBufferFactoryService _tbfs;
        private readonly IClassificationTypeRegistryService _crs;
        private readonly IContentTypeRegistryService _ctrs;

        public MarkdownCodeTest(IServiceContainer serviceProvider) {
            var coreShell = serviceProvider.GetService<ICoreShell>();
            _crs = coreShell.GetService<IClassificationTypeRegistryService>();
            _ctrs = coreShell.GetService<IContentTypeRegistryService>();
            _tbfs = coreShell.GetService<ITextBufferFactoryService>();
        }

        [Test]
        public void EditRCode01() {
            const string content = "```{r}\n#R\n```";
            var cls = GetClassifier(content, out ITextBuffer textBuffer);

            var actual = GetSpans(cls, textBuffer);
            actual.TrimEnd().Should().Be(string.Empty);

            Typing.Type(textBuffer, 6, "\n");
            actual = GetSpans(cls, textBuffer);
            actual.TrimEnd().Should().Be(string.Empty);

            Typing.Delete(textBuffer, 4, 1);
            actual = GetSpans(cls, textBuffer);
            actual.TrimEnd().Should().Be("[0:13] R Markdown Monospace");

            Typing.Type(textBuffer, 4, "R");
            actual = GetSpans(cls, textBuffer);
            actual.TrimEnd().Should().Be(string.Empty);
        }

        [Test]
        public void EditRCode02() {
            string content = "```{r}\nx <- 1\n```";
            var cls = GetClassifier(content, out ITextBuffer textBuffer);

            Typing.Delete(textBuffer, 0, 1);
            var actual = GetSpans(cls, textBuffer);
            actual.TrimEnd().Should().Be("[0:2] R Markdown Monospace");

            Typing.Type(textBuffer, 1, "`");
            actual = GetSpans(cls, textBuffer);
            actual.TrimEnd().Should().Be(string.Empty);
        }

        private IClassifier GetClassifier(string content, out ITextBuffer textBuffer) {
            textBuffer = _tbfs.CreateTextBuffer(new ContentTypeMock(MdContentTypeDefinition.ContentType));
            textBuffer.Insert(0, content);

            var classifierProvider = new MdClassifierProvider(_crs, _ctrs);
            return classifierProvider.GetClassifier(textBuffer);
        }

        private string GetSpans(IClassifier cls, ITextBuffer textBuffer) {
            var spans = cls.GetClassificationSpans(new SnapshotSpan(textBuffer.CurrentSnapshot, new Span(0, textBuffer.CurrentSnapshot.Length)));
            return ClassificationWriter.WriteClassifications(spans);
        }
    }
}
