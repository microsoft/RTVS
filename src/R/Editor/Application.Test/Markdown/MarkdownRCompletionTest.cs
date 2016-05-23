// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.Markdown.Editor.ContentTypes;
using Microsoft.R.Editor.Application.Test.TestShell;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;

namespace Microsoft.R.Editor.Application.Test.Markdown {
    [ExcludeFromCodeCoverage]
    [Collection(CollectionNames.NonParallel)]
    public class MarkdownRCompletionTest {
        [Test]
        [Category.Interactive]
        public void TypeRBlock() {
            using (var script = new TestScript("```{r}\r\nfunction\r\n```", MdContentTypeDefinition.ContentType)) {
                script.DoIdle(3000);
                script.MoveDown();
                script.Type("x");
                script.DoIdle(1000);
                script.Type("<-");
                script.DoIdle(200);
                script.Type("function");
                script.DoIdle(4000);
                script.Type("{TAB}(){");
                script.DoIdle(200);
                script.Type("{ENTER}a");

                string expected = 
@"
```{r}
x <- function() {
    a
}
```";
                string actual = script.EditorText;
                actual.Should().Be(expected);
            }
        }
    }
}
