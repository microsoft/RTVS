using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using Microsoft.Languages.Core.Test.Utility;
using Microsoft.Languages.Editor.Shell;
using Microsoft.Languages.Editor.Test.Mocks;
using Microsoft.Languages.Editor.Test.Utility;
using Microsoft.Languages.Editor.Tests.Shell;
using Microsoft.R.Support.Markdown.Classification;
using Microsoft.R.Support.Markdown.ContentTypes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;

namespace Microsoft.R.Support.Test.Markdown.Classification
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class MArkdownClassifierTest : UnitTestBase
    {
        // change to true in debugger if you want all baseline tree files regenerated
        private static bool _regenerateBaselineFiles = false;

        [TestMethod]
        public void ClassifyMarkdownFileTest01()
        {
            EditorShell.SetShell(TestEditorShell.Create());
            ClassifyFile(TestContext, @"Classification\01.md");
        }

        private static void ClassifyFile(TestContext context, string fileName)
        {
            try
            {
                string testFile = TestFiles.GetTestFilePath(context, fileName);
                string content = TestFiles.LoadFile(context, fileName);

                TextBufferMock textBuffer = new TextBufferMock(content, MdContentTypeDefinition.ContentType);
                ClassificationTypeRegistryServiceMock ctrs = new ClassificationTypeRegistryServiceMock();
                MdClassifier cls = new MdClassifier(textBuffer, ctrs);

                IList<ClassificationSpan> spans = cls.GetClassificationSpans(new SnapshotSpan(textBuffer.CurrentSnapshot, new Span(0, textBuffer.CurrentSnapshot.Length)));
                string actual = ClassificationWriter.WriteClassifications(spans);

                string baselineFile = testFile + ".colors";

                if (_regenerateBaselineFiles)
                {
                    // Update this to your actual enlistment if you need to update baseline
                    string enlistmentPath = @"F:\RTVS\src\R\Support\Test\Markdown\Files\Classification";
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
