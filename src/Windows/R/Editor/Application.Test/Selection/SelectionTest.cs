// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.UI.Commands;
using Microsoft.R.Components.ContentTypes;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;

namespace Microsoft.R.Editor.Application.Test.Selection {
    [ExcludeFromCodeCoverage]
    [Collection(CollectionNames.NonParallel)]
    public class SignatureTest {
        private readonly ICoreShell _coreShell;
        private readonly EditorHostMethodFixture _editorHost;

        public SignatureTest(IServiceContainer services, EditorHostMethodFixture editorHost) {
            _coreShell = services.GetService<ICoreShell>();
            _editorHost = editorHost;
        }

        [Test]
        [Category.Interactive]
        public async Task R_SelectWord01() {
            using (var script = await _editorHost.StartScript(_coreShell, "\r\nabc$def['test test']", RContentTypeDefinition.ContentType)) {

                script.MoveDown();
                script.Execute(VSConstants.VSStd2KCmdID.SELECTCURRENTWORD);
                var span = script.View.Selection.StreamSelectionSpan;
                var selectedWord = span.GetText();
                selectedWord.Should().Be("abc");

                script.MoveRight(2);
                script.Execute(VSConstants.VSStd2KCmdID.SELECTCURRENTWORD);
                span = script.View.Selection.StreamSelectionSpan;
                selectedWord = span.GetText();
                selectedWord.Should().Be("def");

                script.MoveRight(3);
                script.Execute(VSConstants.VSStd2KCmdID.SELECTCURRENTWORD);
                span = script.View.Selection.StreamSelectionSpan;
                selectedWord = span.GetText();
                selectedWord.Should().Be("test");
            }
        }

        [Test]
        [Category.Interactive]
        public async Task R_SelectWord02() {
            using (var script = await _editorHost.StartScript(_coreShell, "`abc`$\"def\"", RContentTypeDefinition.ContentType)) {

                script.Execute(VSConstants.VSStd2KCmdID.SELECTCURRENTWORD);
                var span = script.View.Selection.StreamSelectionSpan;
                var selectedWord = span.GetText();
                selectedWord.Should().Be("`abc`");

                script.MoveRight(3);
                script.Execute(VSConstants.VSStd2KCmdID.SELECTCURRENTWORD);
                span = script.View.Selection.StreamSelectionSpan;
                selectedWord = span.GetText();
                selectedWord.Should().Be("def");
            }
        }

        [Test]
        [Category.Interactive]
        public async Task R_SelectWord03() {
            using (var script = await _editorHost.StartScript(_coreShell, "abc\'def", RContentTypeDefinition.ContentType)) {

                script.Execute(VSConstants.VSStd2KCmdID.SELECTCURRENTWORD);
                var span = script.View.Selection.StreamSelectionSpan;
                var selectedWord = span.GetText();
                selectedWord.Should().Be("abc");

                script.MoveRight(2);
                script.Execute(VSConstants.VSStd2KCmdID.SELECTCURRENTWORD);
                span = script.View.Selection.StreamSelectionSpan;
                selectedWord = span.GetText();
                selectedWord.Should().Be("def");
            }
        }
    }
}
