using System.Diagnostics.CodeAnalysis;
using Microsoft.R.Editor.Application.Test.TestShell;
using Microsoft.R.Editor.ContentType;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.R.Editor.Application.Test.Formatting {
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class SmartIndentTest {
        [TestMethod]
        public void R_SmartIndentTest() {
            var script = new TestScript("{\r\n}", RContentTypeDefinition.ContentType);

            try {
                script.MoveRight();
                script.Type("{ENTER}a");
                script.DoIdle(300);

                string expected = "{\r\n    a\r\n}";
                string actual = script.EditorText;

                Assert.AreEqual(expected, actual);
            } finally {
                script.Close();
            }
        }
    }
}
