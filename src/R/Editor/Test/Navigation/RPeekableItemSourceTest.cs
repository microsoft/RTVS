// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using FluentAssertions;
using Microsoft.R.Components.ContentTypes;
using Microsoft.R.Editor.Navigation.Peek;
using Microsoft.R.Editor.Test.Mocks;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.Editor.Mocks;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using NSubstitute;

namespace Microsoft.R.Editor.Test.Navigation {
    [ExcludeFromCodeCoverage]
    [Category.R.Completion]
    public class RPeekableItemSourceTest {
        [Test]
        public void PeekFunction01() {
            List<IPeekableItem> items = new List<IPeekableItem>();
            string content =
@"
x <- function(a) { }
z <- 1
x()";
            RunPeekTest(content, 3, 0, "x");
        }

        [Test]
        public void PeekVariable01() {
            List<IPeekableItem> items = new List<IPeekableItem>();
            string content =
@"
x <- function(a) { }
z <- 1
x()
z";
            RunPeekTest(content, 4, 0, "z");
        }

        [Test]
        public void PeekArgument01() {
            List<IPeekableItem> items = new List<IPeekableItem>();
            string content =
@"
x <- function(a) {
    z <- 1
    x()
    a
}";
            RunPeekTest(content, 4, 4, "a");
        }

        private void RunPeekTest(string content, int line, int column, string name) {
            List<IPeekableItem> items = new List<IPeekableItem>();

            GetPeekableItems(content, line, column, items);
            items.Should().ContainSingle();
            var item = items[0];

            item.DisplayName.Should().Be(name);
            var source = item.GetOrCreateResultSource(PredefinedPeekRelationships.Definitions.Name);
            source.Should().NotBeNull();

            var coll = new PeekResultCollectionMock();
            int count = 0;
            var cb = Substitute.For<IFindPeekResultsCallback>();
            cb.When(x => x.ReportProgress(Arg.Any<int>())).Do(x => count += (int)x[0]);
            source.FindResults(PredefinedPeekRelationships.Definitions.Name, coll, default(CancellationToken), cb);

            count.Should().Be(1);
            coll.Should().HaveCount(1);

            coll[0].DisplayInfo.Title.Should().Be("file.r");
            coll[0].DisplayInfo.Label.Should().Be(name);
            coll[0].DisplayInfo.TitleTooltip.Should().Be(@"C:\file.r");
        }

        private void GetPeekableItems(string content, int lineNumber, int column, IList<IPeekableItem> items) {
            var tb = new TextBufferMock(content, RContentTypeDefinition.ContentType);
            var line = tb.CurrentSnapshot.GetLineFromLineNumber(lineNumber);
            GetPeekableItems(content, line.Start + column, items);
        }

        private void GetPeekableItems(string content, int position, IList<IPeekableItem> items) {
            var document = new EditorDocumentMock(content, @"C:\file.r");

            TextViewMock textView = new TextViewMock(document.TextBuffer, position);
            textView.Caret.MoveTo(new SnapshotPoint(textView.TextBuffer.CurrentSnapshot, position));

            var peekSession = new PeekSessionMock(textView, position);
            var factory = PeekResultFactoryMock.Create();
            var peekSource = new PeekableItemSource(textView.TextBuffer, factory);

            peekSource.AugmentPeekSession(peekSession, items);
        }
    }
}
