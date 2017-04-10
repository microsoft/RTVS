// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.R.Components.ContentTypes;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.Parser;
using Microsoft.R.Editor.Signatures;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.Editor.Mocks;
using Microsoft.VisualStudio.Text;

namespace Microsoft.R.Editor.Test.Signatures {
    [ExcludeFromCodeCoverage]
    [Category.R.Signatures]
    public class ParameterTest {
        [Test]
        public void ParameterTest01() {
            string content = @"x <- foo(a,b,c,d)";
            AstRoot ast = RParser.Parse(content);

            ITextBuffer textBuffer = new TextBufferMock(content, RContentTypeDefinition.ContentType);
            ParameterInfo parametersInfo = SignatureHelp.GetParametersInfoFromBuffer(ast, textBuffer.CurrentSnapshot, 10);

            parametersInfo.Should().NotBeNull()
                .And.HaveFunctionCall()
                .And.HaveFunctionName("foo")
                .And.HaveParameterIndex(0)
                .And.HaveSignatureEnd(17);

            parametersInfo = SignatureHelp.GetParametersInfoFromBuffer(ast, textBuffer.CurrentSnapshot, 11);
            parametersInfo.Should().HaveParameterIndex(1);
            parametersInfo = SignatureHelp.GetParametersInfoFromBuffer(ast, textBuffer.CurrentSnapshot, 12);
            parametersInfo.Should().HaveParameterIndex(1);

            parametersInfo = SignatureHelp.GetParametersInfoFromBuffer(ast, textBuffer.CurrentSnapshot, 13);
            parametersInfo.Should().HaveParameterIndex(2);
            parametersInfo = SignatureHelp.GetParametersInfoFromBuffer(ast, textBuffer.CurrentSnapshot, 14);
            parametersInfo.Should().HaveParameterIndex(2);

            parametersInfo = SignatureHelp.GetParametersInfoFromBuffer(ast, textBuffer.CurrentSnapshot, 15);
            parametersInfo.Should().HaveParameterIndex(3);
            parametersInfo = SignatureHelp.GetParametersInfoFromBuffer(ast, textBuffer.CurrentSnapshot, 16);
            parametersInfo.Should().HaveParameterIndex(3);
        }

        [Test]
        public void ParameterTest02() {
            string content = @"x <- foo(,,,)";
            AstRoot ast = RParser.Parse(content);

            ITextBuffer textBuffer = new TextBufferMock(content, RContentTypeDefinition.ContentType);
            ParameterInfo parametersInfo = SignatureHelp.GetParametersInfoFromBuffer(ast, textBuffer.CurrentSnapshot, 9);

            parametersInfo.Should().NotBeNull()
                .And.HaveFunctionCall()
                .And.HaveFunctionName("foo")
                .And.HaveParameterIndex(0);

            parametersInfo = SignatureHelp.GetParametersInfoFromBuffer(ast, textBuffer.CurrentSnapshot, 10);
            parametersInfo.Should().HaveParameterIndex(1);
            parametersInfo = SignatureHelp.GetParametersInfoFromBuffer(ast, textBuffer.CurrentSnapshot, 11);
            parametersInfo.Should().HaveParameterIndex(2);
            parametersInfo = SignatureHelp.GetParametersInfoFromBuffer(ast, textBuffer.CurrentSnapshot, 12);
            parametersInfo.Should().HaveParameterIndex(3);
        }

        [Test]
        public void ParameterTest03() {
            string content = @"x <- foo(,,";

            AstRoot ast = RParser.Parse(content);
            ITextBuffer textBuffer = new TextBufferMock(content, RContentTypeDefinition.ContentType);

            var parametersInfo = SignatureHelp.GetParametersInfoFromBuffer(ast, textBuffer.CurrentSnapshot, 11);
            parametersInfo.Should().HaveParameterIndex(2);
        }

        [Test]
        public void ParameterTest04() {
            string content =
@"x <- foo(,, 

if(x > 1) {";
            ParameterInfo parametersInfo;

            AstRoot ast = RParser.Parse(content);
            ITextBuffer textBuffer = new TextBufferMock(content, RContentTypeDefinition.ContentType);

            parametersInfo = SignatureHelp.GetParametersInfoFromBuffer(ast, textBuffer.CurrentSnapshot, 11);
            parametersInfo.Should().HaveParameterIndex(2);
            parametersInfo = SignatureHelp.GetParametersInfoFromBuffer(ast, textBuffer.CurrentSnapshot, 12);
            parametersInfo.Should().HaveParameterIndex(2);
        }

        [Test]
        public void ParameterTest05() {
            string content =
@"x <- abs(cos(


while";
            AstRoot ast = RParser.Parse(content);

            ITextBuffer textBuffer = new TextBufferMock(content, RContentTypeDefinition.ContentType);
            ParameterInfo parametersInfo = SignatureHelp.GetParametersInfoFromBuffer(ast, textBuffer.CurrentSnapshot, content.Length - 5);

            parametersInfo.Should().NotBeNull()
                .And.HaveFunctionCall()
                .And.HaveFunctionName("cos")
                .And.HaveParameterIndex(0)
                .And.HaveSignatureEnd(content.Length - 5);
        }

        [Test]
        public void ParameterTest06() {
            string content =
@"x <- abs(

function(a) {
";
            AstRoot ast = RParser.Parse(content);

            ITextBuffer textBuffer = new TextBufferMock(content, RContentTypeDefinition.ContentType);
            ParameterInfo parametersInfo = SignatureHelp.GetParametersInfoFromBuffer(ast, textBuffer.CurrentSnapshot, content.Length - 1);

            parametersInfo.Should().NotBeNull()
                .And.HaveFunctionCall()
                .And.HaveFunctionName("abs")
                .And.HaveParameterIndex(0)
                .And.HaveSignatureEnd(content.Length);
        }

        [Test]
        public void ParameterTest07() {
            string content = "x <- a(b( ), )";
            AstRoot ast = RParser.Parse(content);

            ITextBuffer textBuffer = new TextBufferMock(content, RContentTypeDefinition.ContentType);
            var parametersInfo = SignatureHelp.GetParametersInfoFromBuffer(ast, textBuffer.CurrentSnapshot, 12);
            parametersInfo.Should().NotBeNull()
                .And.HaveFunctionCall()
                .And.HaveFunctionName("a")
                .And.HaveParameterIndex(1);

            parametersInfo = SignatureHelp.GetParametersInfoFromBuffer(ast, textBuffer.CurrentSnapshot, 13);
            parametersInfo.Should().NotBeNull()
                .And.HaveFunctionCall()
                .And.HaveFunctionName("a")
                .And.HaveParameterIndex(1);
        }
    }
}
