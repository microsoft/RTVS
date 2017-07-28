// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Common.Core.Services;
using Microsoft.Markdown.Editor.ContentTypes;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;

namespace Microsoft.R.Editor.Application.Test.Markdown {
    [ExcludeFromCodeCoverage]
    [Collection(CollectionNames.NonParallel)]
    [Category.Interactive]
    public class MarkdownSmartIndentTest {
        private readonly IServiceContainer _services;
        private readonly EditorHostMethodFixture _editorHost;

        public MarkdownSmartIndentTest(IServiceContainer services, EditorHostMethodFixture editorHost) {
            _services = services;
            _editorHost = editorHost;
        }

        [Test]
        public async Task ScopeIndent() {
            using (var script = await _editorHost.StartScript(_services, string.Empty, MdContentTypeDefinition.ContentType)) {

                script.Type("```{r}{ENTER}{ENTER}```");
                script.MoveUp();
                script.Type("if(true){");
                script.DoIdle(200);
                script.Enter();
                script.DoIdle(200);
                script.Type("a");

                var expected =
@"```{r}
if (TRUE) {
    a
}
```";
                var actual = script.EditorText;
                actual.Should().Be(expected);
            }
        }

        [Test]
        public async Task ExpressionIndent() {
            using (var script = await _editorHost.StartScript(_services, "```{r}\r\n\r\n```", MdContentTypeDefinition.ContentType)) {

                script.DoIdle(500);
                script.MoveDown();
                script.Type("x +");
                script.DoIdle(200);
                script.Enter();
                script.Type("y");

                var actual = script.EditorText;
                actual.Should().Be("```{r}\r\nx +\r\n    y\r\n```");
            }
        }
    }
}
