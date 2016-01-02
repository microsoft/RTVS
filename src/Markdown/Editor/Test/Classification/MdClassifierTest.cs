using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.IO;
using Microsoft.Common.Core.Tests.Utility;
using Microsoft.Languages.Editor.Shell;
using Microsoft.Languages.Editor.Tests.Utility;
using Microsoft.Markdown.Editor.Classification.MD;
using Microsoft.Markdown.Editor.ContentTypes;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.Editor.Mocks;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Xunit;

namespace Microsoft.Markdown.Editor.Tests.Classification {
    public class MarkdownClassifierTest : xUnitFileTest {
        // change to true in debugger if you want all baseline tree files regenerated
        private static bool _regenerateBaselineFiles = false;

        public MarkdownClassifierTest(TestFilesSetupFixture fixture) : base(fixture) {
            Fixture.Initialize("Classification");
        }

        [Fact]
        [Trait("Category", "Md.Classifier")]
        public void ClassifyMarkdownFileTest01() {
            ClassifyFile("01.md");
        }

        private void ClassifyFile(string fileName) {
            try {
                string testFile = GetTestFilePath(fileName);
                string content = LoadFile(fileName);

                TextBufferMock textBuffer = new TextBufferMock(content, MdContentTypeDefinition.ContentType);

                MdClassifierProvider classifierProvider = new MdClassifierProvider();
                EditorShell.Current.CompositionService.SatisfyImportsOnce(classifierProvider);

                IClassifier cls = classifierProvider.GetClassifier(textBuffer);

                IList<ClassificationSpan> spans = cls.GetClassificationSpans(new SnapshotSpan(textBuffer.CurrentSnapshot, new Span(0, textBuffer.CurrentSnapshot.Length)));
                string actual = ClassificationWriter.WriteClassifications(spans);

                string baselineFile = testFile + ".colors";

                if (_regenerateBaselineFiles) {
                    // Update this to your actual enlistment if you need to update baseline
                    string enlistmentPath = @"F:\RTVS\src\Markdown\Editor\Test\Files\Classification";
                    baselineFile = Path.Combine(enlistmentPath, Path.GetFileName(testFile)) + ".colors";

                    TestFiles.UpdateBaseline(baselineFile, actual);
                } else {
                    TestFiles.CompareToBaseLine(baselineFile, actual);
                }
            } catch (Exception exception) {
                Assert.True(false, string.Format(CultureInfo.InvariantCulture, "Test {0} has thrown an exception: {1}", Path.GetFileName(fileName), exception.Message));
            }
        }
    }
}
