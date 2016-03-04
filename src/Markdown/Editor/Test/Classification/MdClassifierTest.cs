// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using FluentAssertions;
using Microsoft.Common.Core.Test.Utility;
using Microsoft.Languages.Editor.Shell;
using Microsoft.Languages.Editor.Test.Utility;
using Microsoft.Markdown.Editor.Classification.MD;
using Microsoft.Markdown.Editor.ContentTypes;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.Editor.Mocks;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;

namespace Microsoft.Markdown.Editor.Test.Classification {
    [ExcludeFromCodeCoverage]
    public class MarkdownClassifierTest {
        private readonly MarkdownTestFilesFixture _files;
        // change to true in debugger if you want all baseline tree files regenerated
        private static bool _regenerateBaselineFiles = false;

        public MarkdownClassifierTest(MarkdownTestFilesFixture files) {
            _files = files;
        }

        [Test]
        [Category.Md.Classifier]
        public void ClassifyMarkdownFileTest01() {
            Action a = () => ClassifyFile(_files, @"Classification\01.md");
            a.ShouldNotThrow();
        }

        private static void ClassifyFile(MarkdownTestFilesFixture fixture, string fileName) {
            string testFile = fixture.GetDestinationPath(fileName);
            string content = fixture.LoadDestinationFile(fileName);

            TextBufferMock textBuffer = new TextBufferMock(content, MdContentTypeDefinition.ContentType);

            MdClassifierProvider classifierProvider = new MdClassifierProvider();
            EditorShell.Current.CompositionService.SatisfyImportsOnce(classifierProvider);

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
