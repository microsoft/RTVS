// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.Shell;
using Microsoft.R.Components.ContentTypes;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.Parser;
using Microsoft.R.Editor.Completion;
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
x()
";

            GetPeekableItems(content, 3, 0, items);
            items.Should().ContainSingle().Which.DisplayName.Should().Be("x");
        }

        [Test]
        public void PeekVariable01() {
            List<IPeekableItem> items = new List<IPeekableItem>();
            string content =
@"
x <- function(a) { }
z <- 1
x()
z
";

            GetPeekableItems(content, 4, 0, items);
            items.Should().ContainSingle().Which.DisplayName.Should().Be("z");
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
}
";

            GetPeekableItems(content, 4, 4, items);
            items.Should().ContainSingle().Which.DisplayName.Should().Be("a");
        }

        private void GetPeekableItems(string content, int lineNumber, int column, IList<IPeekableItem> items) {
            var tb = new TextBufferMock(content, RContentTypeDefinition.ContentType);
            var line = tb.CurrentSnapshot.GetLineFromLineNumber(lineNumber);
            GetPeekableItems(content, line.Start + column, items);
        }

        private void GetPeekableItems(string content, int position, IList<IPeekableItem> items) {
            var document = new EditorDocumentMock(content, "file.r");

            TextViewMock textView = new TextViewMock(document.TextBuffer, position);
            textView.Caret.MoveTo(new SnapshotPoint(textView.TextBuffer.CurrentSnapshot, position));

            var peekSession = new PeekSessionMock(textView, position);
            var factory = PeekResultFactoryMock.Create();
            var peekSource = new PeekableItemSource(textView.TextBuffer, factory);

            peekSource.AugmentPeekSession(peekSession, items);
        }
    }
}
