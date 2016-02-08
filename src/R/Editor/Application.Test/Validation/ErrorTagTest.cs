using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.Languages.Editor.TaskList.Definitions;
using Microsoft.R.Editor.Application.Test.TestShell;
using Microsoft.R.Editor.ContentType;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;

namespace Microsoft.R.Editor.Application.Test.Validation {
    [ExcludeFromCodeCoverage]
    [Collection(CollectionNames.NonParallel)]
    public class ErrorTagTest {
        [Test]
        [Category.Interactive]
        public void R_ErrorTagsTest01() {
            using (var script = new TestScript(RContentTypeDefinition.ContentType)) {
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
