// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.R.Components.ContentTypes;
using Microsoft.R.Editor.Application.Test.TestShell;
using Microsoft.UnitTests.Core.Mef;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;

namespace Microsoft.R.Editor.Application.Test.Selection {
    [ExcludeFromCodeCoverage]
    [Collection(CollectionNames.NonParallel)]
    public class SignatureTest : IDisposable {
        private readonly IExportProvider _exportProvider;
        private readonly EditorHostMethodFixture _editorHost;

        public SignatureTest(REditorApplicationMefCatalogFixture catalogFixture, EditorHostMethodFixture editorHost) {
            _exportProvider = catalogFixture.CreateExportProvider();
            _editorHost = editorHost;
        }

        public void Dispose() {
            _exportProvider.Dispose();
        }
        
        [Test]
        [Category.Interactive]
        public void R_SelectWord01() {
            using (var script = _editorHost.StartScript(_exportProvider, "\r\nabc$def['test test']", RContentTypeDefinition.ContentType)) {

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
            using (var script = _editorHost.StartScript(_exportProvider, "`abc`$\"def\"", RContentTypeDefinition.ContentType)) {

                script.Execute(Languages.Editor.Controller.Constants.VSConstants.VSStd2KCmdID.SELECTCURRENTWORD);
                var span = EditorWindow.CoreEditor.View.Selection.StreamSelectionSpan;
                var selectedWord = span.GetText();
                selectedWord.Should().Be("`abc`");

                script.MoveRight(3);
                script.Execute(Languages.Editor.Controller.Constants.VSConstants.VSStd2KCmdID.SELECTCURRENTWORD);
                span = EditorWindow.CoreEditor.View.Selection.StreamSelectionSpan;
                selectedWord = span.GetText();
                selectedWord.Should().Be("def");
            }
        }

        [Test]
        [Category.Interactive]
        public void R_SelectWord03() {
            using (var script = _editorHost.StartScript(_exportProvider, "abc\'def", RContentTypeDefinition.ContentType)) {

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
