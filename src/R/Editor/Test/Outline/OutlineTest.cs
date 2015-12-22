using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using Microsoft.Common.Core.Tests.Utility;
using Microsoft.Languages.Core.Tests.Utility;
using Microsoft.Languages.Editor.Outline;
using Microsoft.R.Editor.ContentType;
using Microsoft.R.Editor.Outline;
using Microsoft.R.Editor.Test.Mocks;
using Microsoft.R.Editor.Tree;
using Microsoft.VisualStudio.Editor.Mocks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.R.Editor.Test.Outline
{
    [ExcludeFromCodeCoverage]
    public class OutlineTest
    {
        public static OutlineRegionCollection BuildOutlineRegions(string content)
        {
            TextBufferMock textBuffer = new TextBufferMock(content, RContentTypeDefinition.ContentType);
            EditorTree tree = new EditorTree(textBuffer);
            tree.Build();

            EditorDocumentMock editorDocument = new EditorDocumentMock(tree);
            ROutlineRegionBuilder ob = new ROutlineRegionBuilder(editorDocument);
            OutlineRegionCollection rc = new OutlineRegionCollection(0);
            ob.BuildRegions(rc);

            return rc;
        }

        // change to true in debugger if you want all baseline tree files regenerated
        private static bool _regenerateBaselineFiles = false;

        public static void OutlineFile(TestContext context, string name)
        {
            try
            {
                string testFile = TestFiles.GetTestFilePath(context.TestRunDirectory, name);
                string baselineFile = testFile + ".outline";

                string text = TestFiles.LoadFile(context.TestRunDirectory, testFile);

                OutlineRegionCollection rc = OutlineTest.BuildOutlineRegions(text);
                string actual = TextRangeCollectionWriter.WriteCollection(rc);

                if (_regenerateBaselineFiles)
                {
                    // Update this to your actual enlistment if you need to update baseline
                    string enlistmentPath = @"F:\RTVS\src\R\Editor\Test\Files";
                    baselineFile = Path.Combine(enlistmentPath, Path.GetFileName(testFile)) + ".outline";

                    TestFiles.UpdateBaseline(baselineFile, actual);
                }
                else
                {
                    TestFiles.CompareToBaseLine(baselineFile, actual);
                }
            }
            catch (Exception exception)
            {
                Assert.Fail(string.Format(CultureInfo.InvariantCulture, "Test {0} has thrown an exception: {1}", Path.GetFileName(name), exception.Message));
            }
        }
    }
}
