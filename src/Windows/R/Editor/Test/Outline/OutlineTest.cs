// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.IO;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.Test.Utility;
using Microsoft.Languages.Core.Test.Utility;
using Microsoft.Languages.Editor.Outline;
using Microsoft.Languages.Editor.Text;
using Microsoft.R.Components.ContentTypes;
using Microsoft.R.Editor.Outline;
using Microsoft.R.Editor.Test.Mocks;
using Microsoft.R.Editor.Tree;
using Microsoft.VisualStudio.Editor.Mocks;

namespace Microsoft.R.Editor.Test.Outline {
    [ExcludeFromCodeCoverage]
    public class OutlineTest {
        public static OutlineRegionCollection BuildOutlineRegions(ICoreShell shell, string content) {
            var textBuffer = new TextBufferMock(content, RContentTypeDefinition.ContentType);
            var eb = textBuffer.ToEditorBuffer();
            using (var tree = new EditorTree(eb, shell)) {
                tree.Build();
                using (var editorDocument = new EditorDocumentMock(tree)) {
                    using (var ob = new ROutlineRegionBuilder(editorDocument, shell)) {
                        var rc = new OutlineRegionCollection(0);
                        ob.BuildRegions(rc);
                        return rc;
                    }
                }
            }
        }

        // change to true in debugger if you want all baseline tree files regenerated
        private static bool _regenerateBaselineFiles = false;

        public static void OutlineFile(ICoreShell shell, EditorTestFilesFixture fixture, string name) {
            var testFile = fixture.GetDestinationPath(name);
            var baselineFile = testFile + ".outline";
            var text = fixture.LoadDestinationFile(name);

            var rc = BuildOutlineRegions(shell, text);
            var actual = TextRangeCollectionWriter.WriteCollection(rc);

            if (_regenerateBaselineFiles) {
                baselineFile = Path.Combine(fixture.SourcePath, Path.GetFileName(testFile)) + ".outline";
                TestFiles.UpdateBaseline(baselineFile, actual);
            } else {
                TestFiles.CompareToBaseLine(baselineFile, actual);
            }
        }
    }
}
