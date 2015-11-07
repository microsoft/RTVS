using System.Diagnostics.CodeAnalysis;
using Microsoft.R.Editor.Application.Test.TestShell;
using Microsoft.R.Editor.ContentType;
using Microsoft.R.Editor.Settings;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.R.Editor.Application.Test.Formatting {
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class AutoFormatTest {
        [TestMethod]
        public void R_AutoFormatFunctionBraces() {
            var script = new TestScript(RContentTypeDefinition.ContentType);

            try {
                script.Type("function(a,b){");
                script.DoIdle(300);
                script.Type("{ENTER}a");

                string expected = "function(a, b) {\r\n    a\r\n}";
                string actual = script.EditorText;

                Assert.AreEqual(expected, actual);
            } finally {
                script.Close();
            }
        }

        [TestMethod]
        public void R_AutoFormatScopeBraces01() {
            var script = new TestScript(RContentTypeDefinition.ContentType);
            REditorSettings.FormatOptions.BracesOnNewLine = false;

            try {
                script.Type("if(x>1){ENTER}{");
                script.DoIdle(300);
                script.Type("{ENTER}a");

                string expected = "if (x > 1) {\r\n    a\r\n}";
                string actual = script.EditorText;

                Assert.AreEqual(expected, actual);
            } finally {
                script.Close();
            }
        }

        [TestMethod]
        public void R_AutoFormatScopeBraces02() {
            var script = new TestScript(RContentTypeDefinition.ContentType);
            REditorSettings.FormatOptions.BracesOnNewLine = true;

            try {
                script.Type("if(x>1){ENTER}{");
                script.DoIdle(300);
                script.Type("{ENTER}a");

                string expected = "if (x > 1)\r\n{\r\n    a\r\n}";
                string actual = script.EditorText;

                Assert.AreEqual(expected, actual);
            } finally {
                script.Close();
            }
        }

        [TestMethod]
        public void R_AutoFormatScopeBraces03() {
            var script = new TestScript(RContentTypeDefinition.ContentType);
            REditorSettings.FormatOptions.BracesOnNewLine = false;

            try {
                script.Type("while(true) {");
                script.DoIdle(300);
                script.Type("{ENTER}if(x>1) {");
                script.DoIdle(300);
                script.Type("{ENTER}foo");

                string expected = "while (true) {\r\n    if (x > 1) {\r\n        foo\r\n    }\r\n}";
                string actual = script.EditorText;

                Assert.AreEqual(expected, actual);
            } finally {
                script.Close();
            }
        }

        [TestMethod]
        public void R_AutoFormatIfNoScope() {
            var script = new TestScript(RContentTypeDefinition.ContentType);

            try {
                script.Type("if(x>1)");
                script.DoIdle(300);
                script.Type("{ENTER}a");

                string expected = "if (x > 1)\r\n    a";
                string actual = script.EditorText;

                Assert.AreEqual(expected, actual);
            } finally {
                script.Close();
            }
        }

        [TestMethod]
        public void R_AutoFormatFuncionDefinition01() {
            var script = new TestScript(RContentTypeDefinition.ContentType);
            REditorSettings.FormatOptions.BracesOnNewLine = true;
            string text = "library ( abind){ENTER}x <-function (x,y, wt= NULL, intercept =TRUE, tolerance=1e-07, {ENTER}          yname = NULL){ENTER}{{ENTER}abind(a, )";

            try {
                script.Type(text);
                script.DoIdle(300);

                string actual = script.EditorText;
                string expected =
@"library(abind)
x <- function(x, y, wt = NULL, intercept = TRUE, tolerance = 1e-07,
          yname = NULL)
    {
        abind(a, )
    }";
                Assert.AreEqual(expected, actual);
            } finally {
                script.Close();
            }
        }
    }
}
