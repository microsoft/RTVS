// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.Languages.Editor.Controller.Constants;
using Microsoft.R.Components.ContentTypes;
using Microsoft.R.Components.Controller;
using Microsoft.R.Editor.Navigation.Commands;
using Microsoft.R.Editor.Test.Mocks;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.Editor.Mocks;
using Microsoft.VisualStudio.Text;

namespace Microsoft.R.Editor.Test.Completions {
    [ExcludeFromCodeCoverage]
    [Category.R.Navigation]
    public class GotoDefinitionTest {
        [Test]
        public void GoToDefFunction01() {
            string content =
@"
x <- function(a) { }
z <- 1
x()
";
            RunGotoDefTest(content, 3, 0, content.IndexOf("x <-"));
        }

        [Test]
        public void GoToDefFunction02() {
            string content =
@"
x <- function(a) { }
z <- 1
if(TRUE) {
    if(FALSE)
        x()
}";
            RunGotoDefTest(content, 5, 8, content.IndexOf("x <-"));
        }

        [Test]
        public void GoToDefVariable01() {
            string content =
@"
z <- 1
if(TRUE) {
    if(FALSE)
        z
}";
            RunGotoDefTest(content, 4, 8, 2);
        }

        [Test]
        public void GoToDefVariable02() {
            string content =
@"
z <- 1
if(TRUE) {
    z <- 3
    if(FALSE)
        z
}";
            RunGotoDefTest(content, 5, 8, content.IndexOf("z <- 3"));
        }

        [Test]
        public void GoToDefArgument01() {
            string content =
@"
x <- function(a, b, c)
if(TRUE) {
    x <- a
    if(FALSE)
        z
}";
            RunGotoDefTest(content, 3, 9, content.IndexOf("a, b"));
        }

        [Test]
        public void GoToDefArgument02() {
            string content =
@"
x <- function(a, z, c) {
if(TRUE) {
    a <- 3
    if(FALSE)
        z
}}";
            RunGotoDefTest(content, 5, 8, content.IndexOf("z,"));
        }

        [Test]
        public void GoToDefArgument03() {
            string content =
@"
x <- function(a, z = 1, c) {
if(TRUE) {
    a <- 3
    if(FALSE)
        z
}}";
            RunGotoDefTest(content, 5, 8, content.IndexOf("z ="));
        }

        private void RunGotoDefTest(string content, int startLineNumber, int startColumn, int finalCaretPosition) {
            var tb = new TextBufferMock(content, RContentTypeDefinition.ContentType);
            var line = tb.CurrentSnapshot.GetLineFromLineNumber(startLineNumber);
            RunGotoDefTest(content, line.Start + startColumn, finalCaretPosition);
        }

        private void RunGotoDefTest(string content, int startCaretPosition, int finalCaretPosition) {
            var document = new EditorDocumentMock(content);
            var tv = new TextViewMock(document.TextBuffer);
            tv.Caret.MoveTo(new SnapshotPoint(tv.TextBuffer.CurrentSnapshot, startCaretPosition));

            var cmd = new GoToDefinitionCommand(tv, document.TextBuffer);

            var o = new object();
            var result = cmd.Invoke(typeof(VSConstants.VSStd97CmdID).GUID, (int)VSConstants.VSStd97CmdID.GotoDefn, null, ref o);
            result.Should().Be(CommandResult.Executed);
            tv.Caret.Position.BufferPosition.Position.Should().Be(finalCaretPosition);
        }
    }
}
