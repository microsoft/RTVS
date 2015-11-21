using System.Diagnostics.CodeAnalysis;
using Microsoft.R.Editor.Application.Test.TestShell;
using Microsoft.R.Editor.ContentType;
using Microsoft.R.Editor.Settings;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.R.Editor.Application.Test.Completion {
    [ExcludeFromCodeCoverage]
    [TestClass]
    public sealed class RProvisionalTextTest {
        [TestMethod]
        [TestCategory("Interactive")]
        public void R_ProvisionalText01() {
            using (var script = new TestScript(RContentTypeDefinition.ContentType)) {
                script.Type("{");
                script.Type("(");
                script.Type("[");
                script.Type("\"");

                string expected = "{([\"\"])}";
                string actual = script.EditorText;

                Assert.AreEqual(expected, actual);

                REditorSettings.AutoFormat = false;

                script.Type("\"");
                script.Type("]");
                script.Type(")");
                script.Type("}");
                script.DoIdle(1000);

                expected = "{([\"\"])}";
                actual = script.EditorText;

                Assert.AreEqual(expected, actual);
            }
        }

        [TestMethod]
        [TestCategory("Interactive")]
        public void R_ProvisionalText02() {
            using (var script = new TestScript(RContentTypeDefinition.ContentType)) {
                script.Type("c(\"");

                string expected = "c(\"\")";
                string actual = script.EditorText;

                Assert.AreEqual(expected, actual);

                // Move caret outside of the provisional text area 
                // and back so provisional text becomes permanent.
                script.MoveRight();
                script.MoveLeft();

                // Let parser hit on idle so AST updates
                script.DoIdle(300);

                // There should not be completion inside ""
                script.Type("\"");

                expected = "c(\"\"\")";
                actual = script.EditorText;

                Assert.AreEqual(expected, actual);
            }
        }

        // Unstable test
        //[TestMethod]
        [TestCategory("Interactive")]
        public void R_ProvisionalCurlyBrace01() {
            using (var script = new TestScript(RContentTypeDefinition.ContentType)) {
                REditorSettings.FormatOptions.BracesOnNewLine = false;

                script.Type("while(1)");
                script.DoIdle(300);
                script.Type("{");
                script.DoIdle(300);
                script.Type("{ENTER}}");

                string expected = "while (1) {\r\n}";
                string actual = script.EditorText;

                Assert.AreEqual(expected, actual);
            }
        }
    }
}
