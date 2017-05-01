// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using FluentAssertions;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.Test.Utility;
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
    public class MarkdownClassifierTest {
        private readonly ICoreShell _coreShell;
        private readonly MarkdownTestFilesFixture _files;
        // change to true in debugger if you want all baseline tree files regenerated
        private static bool _regenerateBaselineFiles = false;

        public MarkdownClassifierTest(IServiceContainer serviceProvider, MarkdownTestFilesFixture files) {
            _coreShell = serviceProvider.GetService<ICoreShell>();
            _files = files;
        }

        [Test]
        [Category.Md.Classifier]
        public void ClassifyMarkdownFileTest01() {
            Action a = () => ClassifyFile(_files, @"Classification\01.rmd");
            a.ShouldNotThrow();
        }

        private void ClassifyFile(MarkdownTestFilesFixture fixture, string fileName) {
            var testFile = fixture.GetDestinationPath(fileName);
            var content = fixture.LoadDestinationFile(fileName);

            var textBuffer = new TextBufferMock(content, MdContentTypeDefinition.ContentType);

            var crs = _coreShell.GetService<IClassificationTypeRegistryService>();
            var ctrs = _coreShell.GetService<IContentTypeRegistryService>();
            var classifierProvider = new MdClassifierProvider(crs, ctrs);
            var cls = classifierProvider.GetClassifier(textBuffer);

            var spans = cls.GetClassificationSpans(new SnapshotSpan(textBuffer.CurrentSnapshot, new Span(0, textBuffer.CurrentSnapshot.Length)));
            var actual = ClassificationWriter.WriteClassifications(spans);
            var baselineFile = testFile + ".colors";

            if (_regenerateBaselineFiles) {
                baselineFile = Path.Combine(fixture.SourcePath, @"Classification\", Path.GetFileName(testFile)) + ".colors";
                TestFiles.UpdateBaseline(baselineFile, actual);
            } else {
                TestFiles.CompareToBaseLine(baselineFile, actual);
            }
        }
    }
}
