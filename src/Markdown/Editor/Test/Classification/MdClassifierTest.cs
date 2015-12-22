using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using Microsoft.Common.Core.Tests.Utility;
using Microsoft.Languages.Editor.Shell;
using Microsoft.Languages.Editor.Tests.Utility;
using Microsoft.Markdown.Editor.Classification.MD;
using Microsoft.Markdown.Editor.ContentTypes;
using Microsoft.VisualStudio.Editor.Mocks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;

namespace Microsoft.Markdown.Editor.Test.Classification {
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class MarkdownClassifierTest : UnitTestBase
    {
        // change to true in debugger if you want all baseline tree files regenerated
        private static bool _regenerateBaselineFiles = false;

        [TestMethod]
        [TestCategory("Md.Classifier")]
        public void ClassifyMarkdownFileTest01()
        {
            ClassifyFile(TestContext, @"Classification\01.md");
        }

        private static void ClassifyFile(TestContext context, string fileName)
        {
            try
            {
                string testFile = TestFiles.GetTestFilePath(context, fileName);
                string content = TestFiles.LoadFile(context, fileName);

                TextBufferMock textBuffer = new TextBufferMock(content, MdContentTypeDefinition.ContentType);

                 MdClassifierProvider classifierProvider = new MdClassifierProvider();
                EditorShell.Current.CompositionService.SatisfyImportsOnce(classifierProvider);

                IClassifier cls = classifierProvider.GetClassifier(textBuffer);

                IList<ClassificationSpan> spans = cls.GetClassificationSpans(new SnapshotSpan(textBuffer.CurrentSnapshot, new Span(0, textBuffer.CurrentSnapshot.Length)));
                string actual = ClassificationWriter.WriteClassifications(spans);

                string baselineFile = testFile + ".colors";

                if (_regenerateBaselineFiles)
                {
                    // Update this to your actual enlistment if you need to update baseline
                    string enlistmentPath = @"F:\RTVS\src\Markdown\Editor\Test\Files\Classification";
                    baselineFile = Path.Combine(enlistmentPath, Path.GetFileName(testFile)) + ".colors";

                    TestFiles.UpdateBaseline(baselineFile, actual);
                }
                else
                {
                    TestFiles.CompareToBaseLine(baselineFile, actual);
                }
            }
            catch (Exception exception)
            {
                Assert.Fail(string.Format(CultureInfo.InvariantCulture, "Test {0} has thrown an exception: {1}", Path.GetFileName(fileName), exception.Message));
            }
        }
    }
}
