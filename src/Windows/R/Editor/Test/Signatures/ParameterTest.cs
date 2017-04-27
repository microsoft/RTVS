// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.Languages.Editor.Text;
using Microsoft.R.Components.ContentTypes;
using Microsoft.R.Core.Parser;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.Editor.Mocks;

namespace Microsoft.R.Editor.Test.Signatures {
    [ExcludeFromCodeCoverage]
    [Category.R.Signatures]
    public class ParameterTest {
        [Test]
        public void ParameterTest01() {
            var content = @"x <- foo(a,b,c,d)";
            var ast = RParser.Parse(content);

            var editorBuffer = new TextBufferMock(content, RContentTypeDefinition.ContentType).ToEditorBuffer();
            var parametersInfo = ast.GetSignatureInfoFromBuffer(editorBuffer.CurrentSnapshot, 10);

            parametersInfo.Should().NotBeNull()
                .And.HaveFunctionCall()
                .And.HaveFunctionName("foo")
                .And.HaveParameterIndex(0)
                .And.HaveSignatureEnd(17);

            parametersInfo = ast.GetSignatureInfoFromBuffer(editorBuffer.CurrentSnapshot, 11);
            parametersInfo.Should().HaveParameterIndex(1);
            parametersInfo = ast.GetSignatureInfoFromBuffer(editorBuffer.CurrentSnapshot, 12);
            parametersInfo.Should().HaveParameterIndex(1);

            parametersInfo = ast.GetSignatureInfoFromBuffer(editorBuffer.CurrentSnapshot, 13);
            parametersInfo.Should().HaveParameterIndex(2);
            parametersInfo = ast.GetSignatureInfoFromBuffer(editorBuffer.CurrentSnapshot, 14);
            parametersInfo.Should().HaveParameterIndex(2);

            parametersInfo = ast.GetSignatureInfoFromBuffer(editorBuffer.CurrentSnapshot, 15);
            parametersInfo.Should().HaveParameterIndex(3);
            parametersInfo = ast.GetSignatureInfoFromBuffer(editorBuffer.CurrentSnapshot, 16);
            parametersInfo.Should().HaveParameterIndex(3);
        }

        [Test]
        public void ParameterTest02() {
            var content = @"x <- foo(,,,)";
            var ast = RParser.Parse(content);

            var editorBuffer = new TextBufferMock(content, RContentTypeDefinition.ContentType).ToEditorBuffer();
            var parametersInfo = ast.GetSignatureInfoFromBuffer(editorBuffer.CurrentSnapshot, 9);

            parametersInfo.Should().NotBeNull()
                .And.HaveFunctionCall()
                .And.HaveFunctionName("foo")
                .And.HaveParameterIndex(0);

            parametersInfo = ast.GetSignatureInfoFromBuffer(editorBuffer.CurrentSnapshot, 10);
            parametersInfo.Should().HaveParameterIndex(1);
            parametersInfo = ast.GetSignatureInfoFromBuffer(editorBuffer.CurrentSnapshot, 11);
            parametersInfo.Should().HaveParameterIndex(2);
            parametersInfo = ast.GetSignatureInfoFromBuffer(editorBuffer.CurrentSnapshot, 12);
            parametersInfo.Should().HaveParameterIndex(3);
        }

        [Test]
        public void ParameterTest03() {
            var content = @"x <- foo(,,";
            var ast = RParser.Parse(content);
            var editorBuffer = new TextBufferMock(content, RContentTypeDefinition.ContentType).ToEditorBuffer();

            var parametersInfo = ast.GetSignatureInfoFromBuffer(editorBuffer.CurrentSnapshot, 11);
            parametersInfo.Should().HaveParameterIndex(2);
        }

        [Test]
        public void ParameterTest04() {
            string content =
@"x <- foo(,, 

if(x > 1) {";
            var ast = RParser.Parse(content);
            var editorBuffer = new TextBufferMock(content, RContentTypeDefinition.ContentType).ToEditorBuffer();

            var parametersInfo = ast.GetSignatureInfoFromBuffer(editorBuffer.CurrentSnapshot, 11);
            parametersInfo.Should().HaveParameterIndex(2);
            parametersInfo = ast.GetSignatureInfoFromBuffer(editorBuffer.CurrentSnapshot, 12);
            parametersInfo.Should().HaveParameterIndex(2);
        }

        [Test]
        public void ParameterTest05() {
            var content =
@"x <- abs(cos(


while";
            var ast = RParser.Parse(content);
            var editorBuffer = new TextBufferMock(content, RContentTypeDefinition.ContentType).ToEditorBuffer();
            var parametersInfo = ast.GetSignatureInfoFromBuffer(editorBuffer.CurrentSnapshot, content.Length - 5);

            parametersInfo.Should().NotBeNull()
                .And.HaveFunctionCall()
                .And.HaveFunctionName("cos")
                .And.HaveParameterIndex(0)
                .And.HaveSignatureEnd(content.Length - 5);
        }

        [Test]
        public void ParameterTest06() {
            var content =
@"x <- abs(

function(a) {
";
            var ast = RParser.Parse(content);

            var editorBuffer = new TextBufferMock(content, RContentTypeDefinition.ContentType).ToEditorBuffer();
            var parametersInfo = ast.GetSignatureInfoFromBuffer(editorBuffer.CurrentSnapshot, content.Length - 1);

            parametersInfo.Should().NotBeNull()
                .And.HaveFunctionCall()
                .And.HaveFunctionName("abs")
                .And.HaveParameterIndex(0)
                .And.HaveSignatureEnd(content.Length);
        }

        [Test]
        public void ParameterTest07() {
            var content = "x <- a(b( ), )";
            var ast = RParser.Parse(content);

            var editorBuffer = new TextBufferMock(content, RContentTypeDefinition.ContentType).ToEditorBuffer();
            var parametersInfo = ast.GetSignatureInfoFromBuffer(editorBuffer.CurrentSnapshot, 12);
            parametersInfo.Should().NotBeNull()
                .And.HaveFunctionCall()
                .And.HaveFunctionName("a")
                .And.HaveParameterIndex(1);

            parametersInfo = ast.GetSignatureInfoFromBuffer(editorBuffer.CurrentSnapshot, 13);
            parametersInfo.Should().NotBeNull()
                .And.HaveFunctionCall()
                .And.HaveFunctionName("a")
                .And.HaveParameterIndex(1);
        }
    }
}
