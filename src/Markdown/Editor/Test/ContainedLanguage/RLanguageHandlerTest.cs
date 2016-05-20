// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.Languages.Editor.Projection;
using Microsoft.Languages.Editor.Shell;
using Microsoft.Markdown.Editor.ContainedLanguage;
using Microsoft.Markdown.Editor.ContentTypes;
using Microsoft.Markdown.Editor.Utility;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.Editor.Mocks;
using Microsoft.VisualStudio.Text.Projection;
using Microsoft.VisualStudio.Utilities;
using Xunit;

namespace Microsoft.Markdown.Editor.Test.Classification {
    [ExcludeFromCodeCoverage]
    public class RLanguageHandlerTest {
        private readonly IProjectionBufferFactoryService _projectionBufferFactoryService;
        private readonly IContentTypeRegistryService _ctrs;

        public RLanguageHandlerTest() {
            _projectionBufferFactoryService = EditorShell.Current.ExportProvider.GetExportedValue<IProjectionBufferFactoryService>();
            _ctrs = EditorShell.Current.ExportProvider.GetExportedValue<IContentTypeRegistryService>();
        }

        [CompositeTest]
        [Category.Md.RCode]
        [InlineData("{r}", "")]
        [InlineData("{r}\n\n", "")]
        [InlineData("{r x=1}\nplot()", "plot()")]
        [InlineData("{r x=1,\ny=2}\nx <- 1\n", "x <- 1")]
        [InlineData("{r x=function() {\n}}\nx <- 1\n", "x <- 1")]
        [InlineData("{r}\nparams$a = 3\nx <- 1\n", "x <- 1")]
        public void RCode(string markdown, int start, int oldLength, string newCode) {
            var tb = new TextBufferMock(markdown, MdContentTypeDefinition.ContentType);
            var handler = new RLanguageHandler(tb);
        }
    }
}
