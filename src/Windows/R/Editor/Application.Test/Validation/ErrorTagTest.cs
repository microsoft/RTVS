// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Common.Core.Services;
using Microsoft.Languages.Editor.TaskList;
using Microsoft.R.Components.ContentTypes;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;

namespace Microsoft.R.Editor.Application.Test.Validation {
    [ExcludeFromCodeCoverage]
    [Collection(CollectionNames.NonParallel)]
    [Category.Interactive]
    public class ErrorTagTest {
        private readonly IServiceContainer _services;
        private readonly EditorHostMethodFixture _editorHost;

        public ErrorTagTest(IServiceContainer services, EditorHostMethodFixture editorHost) {
            _services = services;
            _editorHost = editorHost;
        }

        [Test]
        public async Task R_ErrorTags() {
            using (var script = await _editorHost.StartScript(_services, RContentTypeDefinition.ContentType)) {
                // Force tagger creation
                var tagSpans = script.GetErrorTagSpans();

                script.Type("x <- {");
                script.Delete(); // two delete b/c of autoformat to { }
                script.Delete();
                script.DoIdle(500);

                tagSpans = script.GetErrorTagSpans();
                var errorTags = script.WriteErrorTags(tagSpans);
                errorTags.Should().Be("[5 - 6] } expected\r\n");

                var item = tagSpans[0].Tag as IEditorTaskListItem;
                item.Line.Should().Be(1);
                item.Column.Should().Be(6);
                item.FileName.Should().Be("filename");
                item.HelpKeyword.Should().Be("vs.r.validationerror");

                script.Type("}");
                script.DoIdle(500);

                tagSpans = script.GetErrorTagSpans();
                tagSpans.Should().BeEmpty();
            }
        }

        [Test]
        public async Task R_LinterTags01() {
            _services.GetService<IREditorSettings>().LintOptions.Enabled = true;

            using (var script = await _editorHost.StartScript(_services, RContentTypeDefinition.ContentType)) {
                // Force tagger creation
                var tagSpans = script.GetErrorTagSpans();

                script.Type("x = 1");
                script.DoIdle(500);

                tagSpans = script.GetErrorTagSpans();
                var errorTags = script.WriteErrorTags(tagSpans);
                errorTags.Should().Be("[2 - 3] ’<-’ should always be used for assignment\r\n");

                var item = tagSpans[0].Tag as IEditorTaskListItem;
                item.Line.Should().Be(1);
                item.Column.Should().Be(3);

                script.MoveLeft(2);
                script.Backspace();

                script.Type("<-");
                script.DoIdle(500);

                tagSpans = script.GetErrorTagSpans();
                tagSpans.Should().BeEmpty();
            }
        }

        [Test]
        public async Task R_LinterTags02() {
            _services.GetService<IREditorSettings>().LintOptions.Enabled = true;

            using (var script = await _editorHost.StartScript(_services, RContentTypeDefinition.ContentType)) {
                // Force tagger creation
                var tagSpans = script.GetErrorTagSpans();

                script.Type("x <- 1{ENTER}{ENTER}");
                script.DoIdle(500);

                tagSpans = script.GetErrorTagSpans();
                var errorTags = script.WriteErrorTags(tagSpans);
                errorTags.Should().Be("[8 - 9] There should not be trailing blank lines in the file.\r\n");

                var item = tagSpans[0].Tag as IEditorTaskListItem;
                item.Line.Should().Be(2);
                item.Column.Should().Be(1);

                script.Backspace();
                script.DoIdle(500);

                tagSpans = script.GetErrorTagSpans();
                tagSpans.Should().BeEmpty();
            }
        }
    }
}
