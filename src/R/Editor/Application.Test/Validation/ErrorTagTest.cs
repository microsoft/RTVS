// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Languages.Editor.TaskList;
using Microsoft.R.Components.ContentTypes;
using Microsoft.UnitTests.Core.Mef;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;

namespace Microsoft.R.Editor.Application.Test.Validation {
    [ExcludeFromCodeCoverage]
    [Collection(CollectionNames.NonParallel)]
    public class ErrorTagTest {
        private readonly IExportProvider _exportProvider;
        private readonly EditorHostMethodFixture _editorHost;

        public ErrorTagTest(IExportProvider exportProvider, EditorHostMethodFixture editorHost) {
            _exportProvider = exportProvider;
            _editorHost = editorHost;
        }

        [Test]
        [Category.Interactive]
        public async Task R_ErrorTagsTest01() {
            using (var script = await _editorHost.StartScript(_exportProvider, RContentTypeDefinition.ContentType)) {
                // Force tagger creation
                var tagSpans = script.GetErrorTagSpans();

                script.Type("x <- {");
                script.Delete();
                script.DoIdle(500);

                tagSpans = script.GetErrorTagSpans();
                string errorTags = script.WriteErrorTags(tagSpans);
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
    }
}
