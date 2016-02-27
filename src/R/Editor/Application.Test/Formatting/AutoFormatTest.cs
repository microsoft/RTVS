using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.R.Editor.Application.Test.TestShell;
using Microsoft.R.Editor.ContentType;
using Microsoft.R.Editor.Settings;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;

namespace Microsoft.R.Editor.Application.Test.Formatting {
    [ExcludeFromCodeCoverage]
    [Collection(CollectionNames.NonParallel)]
    public class AutoFormatTest {
        [Test]
        [Category.Interactive]
        public void R_AutoFormatFunctionBraces() {
            using (var script = new TestScript(RContentTypeDefinition.ContentType)) {
                script.Type("function(a,b){");
                script.DoIdle(300);
                script.Type("{ENTER}a");

                string expected = "function(a, b) {\r\n    a\r\n}";
                string actual = script.EditorText;

                actual.Should().Be(expected);
            }
        }

        [Test]
        [Category.Interactive]
        public void R_AutoFormatScopeBraces01() {
            using (var script = new TestScript(RContentTypeDefinition.ContentType)) {
                REditorSettings.FormatOptions.BracesOnNewLine = false;

                script.Type("if(x>1){ENTER}{");
                script.DoIdle(300);
                script.Type("{ENTER}a");

                string expected = "if (x > 1) {\r\n    a\r\n}";
                string actual = script.EditorText;

                actual.Should().Be(expected);
            }
        }

        [Test]
        [Category.Interactive]
        public void R_AutoFormatScopeBraces02() {
            using (var script = new TestScript(RContentTypeDefinition.ContentType)) {
                REditorSettings.FormatOptions.BracesOnNewLine = true;

                script.Type("if(x>1){ENTER}{");
                script.DoIdle(300);
                script.Type("{ENTER}a");

                string expected = "if (x > 1) \r\n{\r\n    a\r\n}";
                string actual = script.EditorText;

                actual.Should().Be(expected);
            }
        }

        [Test]
        [Category.Interactive]
        public void R_AutoFormatScopeBraces03() {
            using (var script = new TestScript(RContentTypeDefinition.ContentType)) {
                REditorSettings.FormatOptions.BracesOnNewLine = false;

                script.Type("while(true) {");
                script.DoIdle(300);
                script.Type("{ENTER}if(x>1) {");
                script.DoIdle(300);
                script.Type("{ENTER}foo");

                string expected = "while (true) {\r\n    if (x > 1) {\r\n        foo\r\n    }\r\n}";
                string actual = script.EditorText;

                actual.Should().Be(expected);
            }
        }

        [Test]
        [Category.Interactive]
        public void R_AutoFormatScopeBraces04() {
            using (var script = new TestScript(RContentTypeDefinition.ContentType)) {
                REditorSettings.FormatOptions.BracesOnNewLine = false;

                script.Type("while(true) {");
                script.DoIdle(300);
                script.Type("}");

                string expected = "while (true) { }";
                string actual = script.EditorText;

                actual.Should().Be(expected);
            }
        }

        [Test]
        [Category.Interactive]
        public void R_AutoFormatScopeBraces05() {
            using (var script = new TestScript(RContentTypeDefinition.ContentType)) {
                REditorSettings.FormatOptions.BracesOnNewLine = false;

                script.Type("while(true) {");
                script.DoIdle(300);
                script.Type("{ENTER}if(x>1) {");
                script.DoIdle(300);
                script.Type("{ENTER}");
                script.Type("}}");

                string expected = "while (true) {\r\n    if (x > 1) {\r\n    }\r\n}";
                string actual = script.EditorText;

                actual.Should().Be(expected);
            }
        }

        [Test]
        [Category.Interactive]
        public void R_AutoFormatScopeBraces06() {
            using (var script = new TestScript(RContentTypeDefinition.ContentType)) {
                REditorSettings.FormatOptions.BracesOnNewLine = true;

                script.Type("x <-function(a) {");
                script.DoIdle(300);
                script.Type("{ENTER}a");

                string expected = "x <- function(a) \r\n{\r\n    a\r\n}";
                string actual = script.EditorText;

                actual.Should().Be(expected);
            }
        }

        [Test]
        [Category.Interactive]
        public void R_AutoFormatScopeBraces07() {
            using (var script = new TestScript(RContentTypeDefinition.ContentType)) {
                REditorSettings.FormatOptions.BracesOnNewLine = true;

                script.Type("x <-function(a,{ENTER}{TAB}b){ENTER}{");
                script.DoIdle(300);
                script.Type("{ENTER}a");

                string expected = "x <- function(a,\r\n    b) \r\n{\r\n    a\r\n}";
                string actual = script.EditorText;

                actual.Should().Be(expected);
            }
        }

        [Test]
        [Category.Interactive]
        public void R_AutoFormatScopeBraces08() {
            using (var script = new TestScript("while (true) {\r\n}", RContentTypeDefinition.ContentType)) {
                REditorSettings.FormatOptions.BracesOnNewLine = true;

                script.MoveDown();
                script.Enter();

                string expected = "while (true) {\r\n\r\n}";
                string actual = script.EditorText;

                actual.Should().Be(expected);
            }
        }

        [Test]
        [Category.Interactive]
        public void R_AutoFormatScopeBraces09() {
            using (var script = new TestScript(RContentTypeDefinition.ContentType)) {
                REditorSettings.FormatOptions.BracesOnNewLine = false;

                script.Type("if(TRUE){ENTER}while(TRUE){");
                script.DoIdle(300);
                script.Type("{ENTER}a");

                string expected = "if (TRUE)\r\n    while (TRUE) {\r\n        a\r\n    }";
                string actual = script.EditorText;
                actual.Should().Be(expected);

                script.MoveDown();
                script.MoveRight();
                script.Type("{ENTER}");

                expected = "if (TRUE)\r\n    while (TRUE) {\r\n        a\r\n    }\r\n";
                actual = script.EditorText;
                actual.Should().Be(expected);
            }
        }

        [Test]
        [Category.Interactive]
        public void R_AutoFormatScopeBraces10() {
            using (var script = new TestScript(RContentTypeDefinition.ContentType)) {
                REditorSettings.FormatOptions.BracesOnNewLine = false;

                script.Type("if(TRUE){");
                script.DoIdle(300);
                script.Type("{ENTER}a");
                script.MoveDown();
                script.MoveRight();
                script.Type("{ENTER}");
                string expected = "if (TRUE) {\r\n    a\r\n}\r\n";

                string actual = script.EditorText;
                actual.Should().Be(expected);
            }
        }

        [Test]
        [Category.Interactive]
        public void R_AutoFormatFunctionArgument() {
            using (var script = new TestScript(RContentTypeDefinition.ContentType)) {
                REditorSettings.FormatOptions.BracesOnNewLine = false;

                script.Type("zzzz(a=1,{ENTER}");
                script.DoIdle(300);
                script.Type("b=2");
                string expected = "zzzz(a = 1,\r\n    b=2)";

                string actual = script.EditorText;
                actual.Should().Be(expected);
            }
        }

        [Test]
        [Category.Interactive]
        public void R_AutoFormatIfNoScope() {
            using (var script = new TestScript(RContentTypeDefinition.ContentType)) {
                script.Type("if(x>1)");
                script.DoIdle(300);
                script.Type("{ENTER}a");

                string expected = "if (x > 1)\r\n    a";
                string actual = script.EditorText;

                actual.Should().Be(expected);
            }
        }

        [Test]
        [Category.Interactive]
        public void R_AutoFormatOnSemicolon() {
            using (var script = new TestScript(RContentTypeDefinition.ContentType)) {
                script.Type("x<-1;");

                string expected = "x <- 1;";
                string actual = script.EditorText;

                actual.Should().Be(expected);
            }
        }

        [Test]
        [Category.Interactive]
        public void R_AutoFormatFuncionDefinition01() {
            using (var script = new TestScript(RContentTypeDefinition.ContentType)) {
                REditorSettings.FormatOptions.BracesOnNewLine = true;
                string text = "library ( abind){ENTER}x <-function (x,y, wt= NULL, intercept =TRUE, tolerance=1e-07, {ENTER}          yname = NULL){ENTER}{{ENTER}abind(a, )";

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
                actual.Should().Be(expected);
            }
        }
    }
}
