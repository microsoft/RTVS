// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Common.Core.Services;
using Microsoft.R.Components.ContentTypes;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;

namespace Microsoft.R.Editor.Application.Test.Formatting {
    [ExcludeFromCodeCoverage]
    [Category.Interactive]
    [Collection(CollectionNames.NonParallel)]
    public class AutoFormatTest {
        private readonly IServiceContainer _services;
        private readonly EditorHostMethodFixture _editorHost;
        private readonly IREditorSettings _settings;

        public AutoFormatTest(IServiceContainer services, EditorHostMethodFixture editorHost) {
            _services = services;
            _editorHost = editorHost;
            _settings = _services.GetService<IREditorSettings>();
        }

        [Test]
        public async Task R_AutoFormatFunctionBraces() {
            using (var script = await _editorHost.StartScript(_services, RContentTypeDefinition.ContentType)) {
                script.Type("function(a1,b1){");
                script.DoIdle(300);
                script.Type("{ENTER}a1");

                var expected = "function(a1, b1) {\r\n    a1\r\n}";
                var actual = script.EditorText;

                actual.Should().Be(expected);
            }
        }

        [Test]
        public async Task R_AutoFormatScopeBraces01() {
            using (var script = await _editorHost.StartScript(_services, RContentTypeDefinition.ContentType)) {
                _settings.FormatOptions.BracesOnNewLine = false;

                script.Type("if(x>1){ENTER}{");
                script.DoIdle(300);
                script.Type("{ENTER}a");

                var expected = "if (x > 1) {\r\n    a\r\n}";
                var actual = script.EditorText;

                actual.Should().Be(expected);
            }
        }

        [Test]
        public async Task R_AutoFormatScopeBraces02() {
            using (var script = await _editorHost.StartScript(_services, RContentTypeDefinition.ContentType)) {
                _settings.FormatOptions.BracesOnNewLine = true;

                script.Type("if(x>1){ENTER}{");
                script.DoIdle(300);
                script.Type("{ENTER}a");

                var expected = "if (x > 1)\r\n{\r\n    a\r\n}";
                var actual = script.EditorText;

                actual.Should().Be(expected);
            }
        }

        [Test]
        public async Task R_AutoFormatScopeBraces03() {
            using (var script = await _editorHost.StartScript(_services, RContentTypeDefinition.ContentType)) {
                _settings.FormatOptions.BracesOnNewLine = false;

                script.Type("while(true) {");
                script.DoIdle(300);
                script.Type("{ENTER}if(x>1) {");
                script.DoIdle(300);
                script.Type("{ENTER}foo");

                var expected = "while (TRUE) {\r\n    if (x > 1) {\r\n        foo\r\n    }\r\n}";
                var actual = script.EditorText;

                actual.Should().Be(expected);
            }
        }

        [Test]
        public async Task R_AutoFormatScopeBraces04() {
            using (var script = await _editorHost.StartScript(_services, RContentTypeDefinition.ContentType)) {
                _settings.FormatOptions.BracesOnNewLine = false;

                script.Type("while(true) {");
                script.DoIdle(300);
                script.Type("}");

                var expected = "while (TRUE) { }";
                var actual = script.EditorText;

                actual.Should().Be(expected);
            }
        }

        [Test]
        public async Task R_AutoFormatScopeBraces05() {
            using (var script = await _editorHost.StartScript(_services, RContentTypeDefinition.ContentType)) {
                _settings.FormatOptions.BracesOnNewLine = false;

                script.Type("while(True) {");
                script.DoIdle(300);
                script.Type("{ENTER}if(x>1) {");
                script.DoIdle(300);
                script.Type("{ENTER}");
                script.DoIdle(300);
                script.Type("}}");

                var expected = "while (TRUE) {\r\n    if (x > 1) {\r\n    }\r\n}";
                var actual = script.EditorText;

                actual.Should().Be(expected);
            }
        }

        [Test]
        public async Task R_AutoFormatScopeBraces06() {
            using (var script = await _editorHost.StartScript(_services, RContentTypeDefinition.ContentType)) {
                _settings.FormatOptions.BracesOnNewLine = true;

                script.Type("x <-function(a) {");
                script.DoIdle(300);
                script.Type("{ENTER}a");

                var expected = "x <- function(a)\r\n{\r\n    a\r\n}";
                var actual = script.EditorText;

                actual.Should().Be(expected);
            }
        }

        [Test]
        public async Task R_AutoFormatScopeBraces07() {
            using (var script = await _editorHost.StartScript(_services, RContentTypeDefinition.ContentType)) {
                _settings.FormatOptions.BracesOnNewLine = true;

                script.Type("x <-function(a,{ENTER}b){ENTER}{");
                script.DoIdle(300);
                script.Type("{ENTER}a");

                var expected = "x <- function(a,\r\n    b)\r\n{\r\n    a\r\n}";
                var actual = script.EditorText;
                actual.Should().Be(expected);
            }
        }

        [Test]
        public async Task R_AutoFormatScopeBraces08() {
            using (var script = await _editorHost.StartScript(_services, "while (true) {\r\n}", RContentTypeDefinition.ContentType)) {
                _settings.FormatOptions.BracesOnNewLine = true;

                script.MoveDown();
                script.Enter();

                var expected = "while (true)\r\n{\r\n\r\n}";
                var actual = script.EditorText;

                actual.Should().Be(expected);
            }
        }

        [Test]
        public async Task R_AutoFormatScopeBraces09() {
            using (var script = await _editorHost.StartScript(_services, RContentTypeDefinition.ContentType)) {
                _settings.FormatOptions.BracesOnNewLine = false;

                script.Type("if(TRUE){ENTER}while(TRUE){");
                script.DoIdle(300);
                script.Type("{ENTER}a");

                var expected = "if (TRUE)\r\n    while (TRUE) {\r\n        a\r\n    }";
                var actual = script.EditorText;
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
        public async Task R_AutoFormatScopeBraces10() {
            using (var script = await _editorHost.StartScript(_services, RContentTypeDefinition.ContentType)) {
                _settings.FormatOptions.BracesOnNewLine = false;

                script.Type("if(TRUE){");
                script.DoIdle(300);
                script.Type("{ENTER}a");
                script.MoveDown();
                script.MoveRight();
                script.Type("{ENTER}");
                var expected = "if (TRUE) {\r\n    a\r\n}\r\n";

                var actual = script.EditorText;
                actual.Should().Be(expected);
            }
        }

        [Test]
        public async Task R_AutoFormatScopeBraces11() {
            using (var script = await _editorHost.StartScript(_services, RContentTypeDefinition.ContentType)) {
                _settings.FormatOptions.BracesOnNewLine = false;

                script.Type("{");
                script.DoIdle(200);
                script.Type("{ENTER}{");
                script.DoIdle(200);
                script.Type("{ENTER}{");
                script.DoIdle(200);
                script.Type("{ENTER}a");
                var expected = "{\r\n    {\r\n        {\r\n            a\r\n        }\r\n    }\r\n}";

                var actual = script.EditorText;
                actual.Should().Be(expected);
            }
        }

        [Test]
        public async Task R_AutoFormatFunctionArgument() {
            using (var script = await _editorHost.StartScript(_services, RContentTypeDefinition.ContentType)) {
                _settings.FormatOptions.BracesOnNewLine = false;

                script.Type("zzzz(a=1,{ENTER}");
                script.DoIdle(300);
                script.Type("ba=2");
                var expected = "zzzz(a = 1,\r\n    ba=2)";

                var actual = script.EditorText;
                actual.Should().Be(expected);
            }
        }

        [Test]
        public async Task R_AutoFormatIfNoScope() {
            using (var script = await _editorHost.StartScript(_services, RContentTypeDefinition.ContentType)) {
                script.Type("if(x>1)");
                script.DoIdle(300);
                script.Type("{ENTER}a");

                var expected = "if (x > 1)\r\n    a";
                var actual = script.EditorText;

                actual.Should().Be(expected);
            }
        }

        [Test]
        public async Task R_AutoFormatOnSemicolon() {
            using (var script = await _editorHost.StartScript(_services, RContentTypeDefinition.ContentType)) {
                script.Type("x<-1;");

                var expected = "x <- 1;";
                var actual = script.EditorText;

                actual.Should().Be(expected);
            }
        }

        [Test]
        public async Task R_AutoFormatFunctonArguments01() {
            using (var script = await _editorHost.StartScript(_services, RContentTypeDefinition.ContentType)) {
                var text = "x <-function (x,{ENTER}y,{ENTER}wt= NULL){ENTER}";

                script.Type(text);
                script.DoIdle(300);

                var actual = script.EditorText;
                var expected =
"x <- function(x,\r\n" +
"    y,\r\n" +
"    wt = NULL)\r\n";
                actual.Should().Be(expected);
            }
        }

        [Test]
        public async Task R_AutoFormatFunctonArguments02() {
            using (var script = await _editorHost.StartScript(_services, RContentTypeDefinition.ContentType)) {
                var text = "x <-function (x,y,{ENTER}wt= NULL){ENTER}";

                script.Type(text);
                script.DoIdle(300);

                var actual = script.EditorText;
                var expected =
"x <- function(x, y,\r\n" +
"    wt = NULL)\r\n";
                actual.Should().Be(expected);
            }
        }

        [Test]
        public async Task R_AutoFormatFuncionDefinition01() {
            using (var script = await _editorHost.StartScript(_services, RContentTypeDefinition.ContentType)) {
                var text = "x <-function (x,y,{ENTER}wt= NULL){{ENTER}";
                _settings.FormatOptions.BracesOnNewLine = false;

                script.Type(text);
                script.DoIdle(300);
                script.Type("a");
                script.DoIdle(300);

                var actual = script.EditorText;
                var expected =
"x <- function(x, y,\r\n" +
"    wt = NULL) {\r\n" +
"    a\r\n" +
"}";
                actual.Should().Be(expected);
            }
        }

        [Test]
        public async Task R_AutoFormatFuncionDefinition02() {
            using (var script = await _editorHost.StartScript(_services, RContentTypeDefinition.ContentType)) {
                var text1 = "library ( abind){ENTER}x <-function (x,y, wt= NULL, intercept =TRUE, tolerance=1e-07,{ENTER}";
                var text2 = "yname = NULL){{ENTER}abind(a, )";

                script.Type(text1);
                script.DoIdle(300);
                script.Type(text2);
                script.DoIdle(300);

                var actual = script.EditorText;
                var expected =
"library(abind)\r\n" +
"x <- function(x, y, wt = NULL, intercept = TRUE, tolerance = 1e-07,\r\n" +
"    yname = NULL) {\r\n" +
"    abind(a, )\r\n" +
"}";
                actual.Should().Be(expected);
            }
        }

        [Test]
        public async Task R_AutoFormatFuncionDefinition03() {
            using (var script = await _editorHost.StartScript(_services, RContentTypeDefinition.ContentType)) {
                var text1 = "x <-function(x, y,{ENTER}";
                var text2 = "a,b,";
                var text3 = "c, d)";

                script.Type(text1);
                script.DoIdle(300);

                script.Backspace();
                script.DoIdle(300);
                script.Type(text2);
                script.Enter();

                script.Type(text3);

                var actual = script.EditorText;
                var expected =
"x <- function(x, y,\r\n" +
"   a, b,\r\n" +
"   c, d)";
                actual.Should().Be(expected);
            }
        }

        [Test]
        public async Task R_AutoFormatFuncionDefinition04() {
            using (var script = await _editorHost.StartScript(_services, RContentTypeDefinition.ContentType)) {
                var text = "function(){return(1)}";

                script.Type(text);
                script.DoIdle(300);
                script.Enter();

                var actual = script.EditorText;
                var expected = 
@"function() { return(1) }
";
                actual.Should().Be(expected);
            }
        }

        [Test]
        public async Task R_AutoFormatLineBreak() {
            using (var script = await _editorHost.StartScript(_services, "x <- 1", RContentTypeDefinition.ContentType)) {
                script.MoveRight(2);
                script.Enter();

                var actual = script.EditorText;
                var expected =
@"x
<- 1";
                actual.Should().Be(expected);
                script.View.Caret.Position.BufferPosition.Position.Should().Be(3);
            }
        }

        [Test]
        public async Task R_NoAutoFormatInsideString() {
            var content = 
@"s <- '
    string
   'x <- 1";
            using (var script = await _editorHost.StartScript(_services, content, RContentTypeDefinition.ContentType)) {
                script.MoveDown(2);
                script.MoveRight(4);
                script.Enter();

                var actual = script.EditorText;
                var expected = 
@"s <- '
    string
   '
x <- 1";
                actual.Should().Be(expected);
                script.View.Caret.Position.BufferPosition.Position.Should().Be(26);
            }
        }
    }
}
