using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.R.Editor.Application.Test.TestShell;
using Microsoft.R.Editor.ContentType;
using Microsoft.R.Support.RD.ContentTypes;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;

namespace Microsoft.R.Editor.Application.Test.Typing {
    [ExcludeFromCodeCoverage]
    [Collection(CollectionNames.NonParallel)]
    public class TypeFileTest {
        private readonly EditorAppTestFilesFixture _files;

        public TypeFileTest(EditorAppTestFilesFixture files) {
            _files = files;
        }

        //[Test(Skip = "Unstable")]
        //[Category.Interactive]
        public void TypeFile_R() {
            string actual = TypeFileInEditor("lsfit-part.r", RContentTypeDefinition.ContentType);
            string expected = "";
            actual.Should().Be(expected);
        }

        //[Test(Skip="Unstable")]
        //[Category.Interactive]
        public void TypeFile_RD() {
            TypeFileInEditor("01.rd", RdContentTypeDefinition.ContentType);
        }

        /// <summary>
        /// Opens file in an editor window
        /// </summary>
        /// <param name="fileName">File name</param>
        /// <param name="contentType">File content type</param>
        private string TypeFileInEditor(string fileName, string contentType) {
            using (var script = new TestScript(contentType)) {
                string text = _files.LoadDestinationFile(fileName);

                script.Type(text, idleTime: 10);
                return script.EditorText;
            }
        }
    }
}
