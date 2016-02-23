using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.R.Editor.Application.Test.TestShell;
using Microsoft.R.Editor.ContentType;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;

namespace Microsoft.R.Editor.Application.Test.Selection {
    [ExcludeFromCodeCoverage]
    [Collection(CollectionNames.NonParallel)]
    public class SignatureTest {
        [Test]
        [Category.Interactive]
        public void R_SelectWord01() {
            using (var script = new TestScript("\r\nabc$def['test test']", RContentTypeDefinition.ContentType)) {

                script.MoveDown();
                script.Execute(Languages.Editor.Controller.Constants.VSConstants.VSStd2KCmdID.SELECTCURRENTWORD);
                var span = EditorWindow.CoreEditor.View.Selection.StreamSelectionSpan;
                var selectedWord = span.GetText();
                selectedWord.Should().Be("abc");

                script.MoveRight(2);
                script.Execute(Languages.Editor.Controller.Constants.VSConstants.VSStd2KCmdID.SELECTCURRENTWORD);
                span = EditorWindow.CoreEditor.View.Selection.StreamSelectionSpan;
                selectedWord = span.GetText();
                selectedWord.Should().Be("def");

                script.MoveRight(3);
                script.Execute(Languages.Editor.Controller.Constants.VSConstants.VSStd2KCmdID.SELECTCURRENTWORD);
                span = EditorWindow.CoreEditor.View.Selection.StreamSelectionSpan;
                selectedWord = span.GetText();
                selectedWord.Should().Be("test");
            }
        }

        [Test]
        [Category.Interactive]
        public void R_SelectWord02() {
            using (var script = new TestScript("`abc`$\"def\"", RContentTypeDefinition.ContentType)) {

                script.Execute(Languages.Editor.Controller.Constants.VSConstants.VSStd2KCmdID.SELECTCURRENTWORD);
                var span = EditorWindow.CoreEditor.View.Selection.StreamSelectionSpan;
                var selectedWord = span.GetText();
                selectedWord.Should().Be("`abc`");

                script.MoveRight(2);
                script.Execute(Languages.Editor.Controller.Constants.VSConstants.VSStd2KCmdID.SELECTCURRENTWORD);
                span = EditorWindow.CoreEditor.View.Selection.StreamSelectionSpan;
                selectedWord = span.GetText();
                selectedWord.Should().Be("def");
            }
        }

        [Test]
        [Category.Interactive]
        public void R_SelectWord03() {
            using (var script = new TestScript("abc\ndef", RContentTypeDefinition.ContentType)) {

                script.Execute(Languages.Editor.Controller.Constants.VSConstants.VSStd2KCmdID.SELECTCURRENTWORD);
                var span = EditorWindow.CoreEditor.View.Selection.StreamSelectionSpan;
                var selectedWord = span.GetText();
                selectedWord.Should().Be("abc");

                script.MoveRight(2);
                script.Execute(Languages.Editor.Controller.Constants.VSConstants.VSStd2KCmdID.SELECTCURRENTWORD);
                span = EditorWindow.CoreEditor.View.Selection.StreamSelectionSpan;
                selectedWord = span.GetText();
                selectedWord.Should().Be("def");
            }
        }
    }
}
