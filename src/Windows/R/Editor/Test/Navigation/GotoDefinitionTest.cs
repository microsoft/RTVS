// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Common.Core.UI.Commands;
using Microsoft.Languages.Editor.Document;
using Microsoft.R.Editor.Navigation.Commands;
using Microsoft.R.Editor.Test.Mocks;
using Microsoft.R.Host.Client;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.Editor.Mocks;
using Microsoft.VisualStudio.Text;
using NSubstitute;

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
            RunGotoDefTestUserDefinedItem(content, 3, 0, content.IndexOf("x <-"));
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
            RunGotoDefTestUserDefinedItem(content, 5, 8, content.IndexOf("x <-"));
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
            RunGotoDefTestUserDefinedItem(content, 4, 8, 2);
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
            RunGotoDefTestUserDefinedItem(content, 5, 8, content.IndexOf("z <- 3"));
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
            RunGotoDefTestUserDefinedItem(content, 3, 9, content.IndexOf("a, b"));
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
            RunGotoDefTestUserDefinedItem(content, 5, 8, content.IndexOf("z,"));
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
            RunGotoDefTestUserDefinedItem(content, 5, 8, content.IndexOf("z ="));
        }

        [Test]
        public void GoToDefInternalFunction() {
            string content = @"lm()";
            RunGotoDefTestInternalItem(content, 0, 1, "lm");
        }


        private void RunGotoDefTestInternalItem(string content, int startLineNumber, int startColumn, string itemName) {
            TextViewMock tv = SetupTextView(content, startLineNumber, startColumn);
            var session = Substitute.For<IRSession>();
            var viewer = Substitute.For<IObjectViewer>();

            viewer.ViewObjectDetails(session, REnvironments.GlobalEnv, itemName, itemName).Returns(Task.CompletedTask);
            var cmd = new GoToDefinitionCommand(tv, tv.TextBuffer, viewer, session);

            var o = new object();
            var result = cmd.Invoke(typeof(VSConstants.VSStd97CmdID).GUID, (int)VSConstants.VSStd97CmdID.GotoDefn, null, ref o);
            result.Should().Be(CommandResult.Executed);
            viewer.Received().ViewObjectDetails(session, REnvironments.GlobalEnv, itemName, itemName);
        }

        private void RunGotoDefTestUserDefinedItem(string content, int startLineNumber, int startColumn, int finalCaretPosition) {
            TextViewMock tv = SetupTextView(content, startLineNumber, startColumn);
            var cmd = new GoToDefinitionCommand(tv, tv.TextBuffer, Substitute.For<IObjectViewer>(), Substitute.For<IRSession>());

            var o = new object();
            var result = cmd.Invoke(typeof(VSConstants.VSStd97CmdID).GUID, (int)VSConstants.VSStd97CmdID.GotoDefn, null, ref o);
            result.Should().Be(CommandResult.Executed);
            tv.Caret.Position.BufferPosition.Position.Should().Be(finalCaretPosition);
        }

        private TextViewMock SetupTextView(string content, int startLineNumber, int startColumn) {
            var document = new EditorDocumentMock(content);
            var tv = new TextViewMock(document.TextBuffer());
            var line = document.EditorBuffer.CurrentSnapshot.GetLineFromLineNumber(startLineNumber);
            tv.Caret.MoveTo(new SnapshotPoint(tv.TextBuffer.CurrentSnapshot, line.Start + startColumn));
            return tv;
        }
    }
}
