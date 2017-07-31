// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.UI.Commands;
using Microsoft.Markdown.Editor.ContentTypes;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;

namespace Microsoft.R.Editor.Application.Test.Markdown {
    [ExcludeFromCodeCoverage]
    [Collection(CollectionNames.NonParallel)]
    [Category.Interactive]
    public class MarkdownFormatTest {
        private readonly IServiceContainer _services;
        private readonly EditorHostMethodFixture _editorHost;

        public MarkdownFormatTest(IServiceContainer services, EditorHostMethodFixture editorHost) {
            _services = services;
            _editorHost = editorHost;
        }

        [Test]
        public async Task FormatDocument() {
            const string content =
@"Text
```{r echo=FALSE}
x<-1
```
Text
```{r }
x<-function(a,b) {
  
  }
```
";
            using (var script = await _editorHost.StartScript(_services, content, MdContentTypeDefinition.ContentType)) {
                script.Execute(VSConstants.VSStd2KCmdID.FORMATDOCUMENT, 50);
                var expected =
@"Text
```{r echo=FALSE}
x <- 1
```
Text
```{r }
x <- function(a, b) {

}
```
"; ;
                var actual = script.EditorText;
                actual.Should().Be(expected);
            }
        }
    }
}
