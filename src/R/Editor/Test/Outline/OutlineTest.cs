using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using Microsoft.Common.Core.Test.Utility;
using Microsoft.Languages.Core.Test.Utility;
using Microsoft.Languages.Editor.Outline;
using Microsoft.R.Editor.ContentType;
using Microsoft.R.Editor.Outline;
using Microsoft.R.Editor.Test.Mocks;
using Microsoft.R.Editor.Tree;
using Microsoft.VisualStudio.Editor.Mocks;

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

        public static void OutlineFile(EditorTestFilesFixture fixture, string name)
        {
            string testFile = fixture.GetDestinationPath(name);
            string baselineFile = testFile + ".outline";
            string text = fixture.LoadDestinationFile(name);

            OutlineRegionCollection rc = BuildOutlineRegions(text);
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
    }
}
