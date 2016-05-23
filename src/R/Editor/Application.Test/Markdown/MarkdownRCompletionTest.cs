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
            using (var script = new TestScript("", MdContentTypeDefinition.ContentType)) {
                script.Type("```{r}{ENTER}{ENTER}```");
                script.MoveUp();
                script.Type("x");
                script.DoIdle(200);
                script.Type("<-");
                script.DoIdle(200);
                script.Type("funct");
                script.DoIdle(200);
                script.Type("{TAB}(){");
                script.DoIdle(200);
                script.Type("{ENTER}abbr{TAB}");
                script.DoIdle(200);
                script.Type("(nam{TAB}");

                string expected = 
@"```{r}
x <- function() {
    abbreviate(name)
}
```";
                string actual = script.EditorText;
                actual.Should().Be(expected);
            }
        }
    }
}
