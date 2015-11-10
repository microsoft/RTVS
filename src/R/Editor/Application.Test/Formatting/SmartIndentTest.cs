using System.Diagnostics.CodeAnalysis;
using Microsoft.R.Editor.Application.Test.TestShell;
using Microsoft.R.Editor.ContentType;
using Microsoft.R.Editor.Settings;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.R.Editor.Application.Test.Formatting {
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class SmartIndentTest {
        [TestMethod]
        [TestCategory("Interactive")]
        public void R_SmartIndentTest() {
            using (var script = new TestScript(string.Empty, RContentTypeDefinition.ContentType)) {
                REditorSettings.FormatOptions.BracesOnNewLine = false;
                script.MoveRight();
                script.Type("{{ENTER}a");
                script.DoIdle(300);

                string expected = "{\r\n    a\r\n}";
                string actual = script.EditorText;

                Assert.AreEqual(expected, actual);
            }
        }
    }
}
