// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using FluentAssertions;
using Microsoft.Languages.Editor.Outline;
using Microsoft.R.Editor.ContentType;
using Microsoft.R.Editor.Outline;
using Microsoft.R.Editor.Test.Mocks;
using Microsoft.R.Editor.Tree;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.Editor.Mocks;
using Xunit;

namespace Microsoft.R.Editor.Test.Outline {
    [ExcludeFromCodeCoverage]
    [Category.R.Outlining]
    [Collection(CollectionNames.NonParallel)]
    public class OutlineBuilderTest {
        private readonly EditorTestFilesFixture _testFiles;

        public OutlineBuilderTest(EditorTestFilesFixture testFiles) {
            _testFiles = testFiles;
        }

        [Test]
        public void RRegionBuilder_ConstructionTest() {
            TextBufferMock textBuffer = new TextBufferMock(string.Empty, RContentTypeDefinition.ContentType);
            EditorTree tree = new EditorTree(textBuffer);
            EditorDocumentMock editorDocument = new EditorDocumentMock(tree);

            ROutlineRegionBuilder ob = new ROutlineRegionBuilder(editorDocument);

            ob.EditorDocument.Should().NotBeNull();
            ob.EditorTree.Should().NotBeNull();

            editorDocument.DocumentClosing.GetInvocationList().Should().ContainSingle();

            FieldInfo treeUpdateField = tree.GetType().GetField("UpdateCompleted", BindingFlags.Instance | BindingFlags.NonPublic);
            var d = (MulticastDelegate)treeUpdateField.GetValue(tree);
            d.GetInvocationList().Should().ContainSingle();

            ob.Dispose();

            editorDocument.DocumentClosing.Should().BeNull();
            treeUpdateField.GetValue(tree).Should().BeNull();
        }

        [Test(ThreadType.UI)]
        public void RRegionBuilder_Test01() {
            OutlineRegionCollection rc = OutlineTest.BuildOutlineRegions("");

            rc.Should().BeEmpty();
            rc.Start.Should().Be(0);
            rc.Length.Should().Be(0);
        }

        [Test(ThreadType.UI)]
        public void RRegionBuilder_Test02() {
            string content =
@"if (ncol(x) == 1L) {
    xnames < -1
} else {
    xnames < -paste0(1, 1L:ncol(x))
  }
  if (intercept) {
    x<- cbind(1, x)
    xnames<- c(0, xnames)
  }
";
            OutlineRegionCollection rc = OutlineTest.BuildOutlineRegions(content);

            rc.Should().HaveCount(3);

            rc[0].Start.Should().Be(0);
            rc[0].Length.Should().Be(89);

            rc[1].Start.Should().Be(41);
            rc[1].End.Should().Be(89);
            rc[1].DisplayText.Should().Be("else...");

            rc[2].Start.Should().Be(93);
            rc[2].End.Should().Be(162);
            rc[2].DisplayText.Should().Be("if...");
        }

        [Test]
        public void RRegionBuilder_OutlineFile01() {
            Action a = () => OutlineTest.OutlineFile(_testFiles, "01.r");
            a.ShouldNotThrow();
        }
    }
}
