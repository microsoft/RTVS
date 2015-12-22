using System.Diagnostics.CodeAnalysis;
using Microsoft.Common.Core.Tests.Utility;
using Microsoft.Languages.Core.Tests.Utility;
using Microsoft.R.Editor.Application.Test.TestShell;
using Microsoft.R.Editor.ContentType;
using Microsoft.R.Support.RD.ContentTypes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.R.Editor.Application.Test.Typing {
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class TypeFileTest : UnitTestBase {
        //[TestMethod]
        [TestCategory("Interactive")]
        public void TypeFile_R() {
            string actual = TypeFileInEditor("lsfit-part.r", RContentTypeDefinition.ContentType);
            string expected = "";
            Assert.AreEqual(expected, actual);
        }

        //[TestMethod]
        [TestCategory("Interactive")]
        public void TypeFile_RD() {
            TypeFileInEditor("01.rd", RdContentTypeDefinition.ContentType);
        }

        /// <summary>
        /// Opens file in an editor window
        /// </summary>
        /// <param name="fileName">File name</param>
        private string TypeFileInEditor(string fileName, string contentType) {
            using (var script = new TestScript(contentType)) {
                string text = TestFiles.LoadFile(this.TestContext, fileName);
                script.Type(text, idleTime: 10);
                return script.EditorText;
            }
        }
    }
}
