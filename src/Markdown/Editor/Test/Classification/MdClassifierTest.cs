// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using FluentAssertions;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.Test.Utility;
using Microsoft.Languages.Core.Classification;
using Microsoft.Languages.Editor.Composition;
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
    public class MarkdownClassifierTest {
        private readonly IExportProvider _exportProvider;
        private readonly MarkdownTestFilesFixture _files;
        // change to true in debugger if you want all baseline tree files regenerated
        private static bool _regenerateBaselineFiles = false;

        public MarkdownClassifierTest(IExportProvider exportProvider, MarkdownTestFilesFixture files) {
            _exportProvider = exportProvider;
            _files = files;
        }

        [Test]
        [Category.Md.Classifier]
        public void ClassifyMarkdownFileTest01() {
            Action a = () => ClassifyFile(_files, @"Classification\01.rmd");
            a.ShouldNotThrow();
        }

        private void ClassifyFile(MarkdownTestFilesFixture fixture, string fileName) {
            string testFile = fixture.GetDestinationPath(fileName);
            string content = fixture.LoadDestinationFile(fileName);

            TextBufferMock textBuffer = new TextBufferMock(content, MdContentTypeDefinition.ContentType);

            var crs = _exportProvider.GetExportedValue<IClassificationTypeRegistryService>();
            var ctrs = _exportProvider.GetExportedValue<IContentTypeRegistryService>();
            var cnp = _exportProvider.GetExports<IClassificationNameProvider, IComponentContentTypes>();

            MdClassifierProvider classifierProvider = new MdClassifierProvider(crs, ctrs, cnp, _exportProvider.GetExportedValue<ICoreShell>());
            _exportProvider.GetExportedValue<ICoreShell>().CompositionService.SatisfyImportsOnce(classifierProvider);

            IClassifier cls = classifierProvider.GetClassifier(textBuffer);

            IList<ClassificationSpan> spans = cls.GetClassificationSpans(new SnapshotSpan(textBuffer.CurrentSnapshot, new Span(0, textBuffer.CurrentSnapshot.Length)));
            string actual = ClassificationWriter.WriteClassifications(spans);

            string baselineFile = testFile + ".colors";

            if (_regenerateBaselineFiles) {
                baselineFile = Path.Combine(fixture.SourcePath, @"Classification\", Path.GetFileName(testFile)) + ".colors";
                TestFiles.UpdateBaseline(baselineFile, actual);
            } else {
                TestFiles.CompareToBaseLine(baselineFile, actual);
            }
        }
    }
}
