// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.UI.Commands;
using Microsoft.R.Components.ContentTypes;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;

namespace Microsoft.R.Editor.Application.Test.Selection {
    [ExcludeFromCodeCoverage]
    [Category.Interactive]
    [Collection(CollectionNames.NonParallel)]
    public sealed class SignatureTest {
        private readonly IServiceContainer _services;
        private readonly EditorHostMethodFixture _editorHost;

        public SignatureTest(IServiceContainer services, EditorHostMethodFixture editorHost) {
            _services = services;
            _editorHost = editorHost;
        }

        [Test]
        public async Task R_SelectWord01() {
            using (var script = await _editorHost.StartScript(_services, "\r\nabc$def['test test']", RContentTypeDefinition.ContentType)) {

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
        public async Task R_SelectWord02() {
            using (var script = await _editorHost.StartScript(_services, "`abc`$\"def\"", RContentTypeDefinition.ContentType)) {

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
        public async Task R_SelectWord03() {
            using (var script = await _editorHost.StartScript(_services, "abc\'def", RContentTypeDefinition.ContentType)) {

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
