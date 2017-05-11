// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.Common.Core.Services;
using Microsoft.Languages.Editor.Projection;
using Microsoft.Markdown.Editor.ContainedLanguage;
using Microsoft.Markdown.Editor.ContentTypes;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.Editor.Mocks;
using NSubstitute;
using Xunit;

namespace Microsoft.Markdown.Editor.Test.ContainedLanguage {
    [ExcludeFromCodeCoverage]
    public class RLanguageHandlerTest {
        private readonly IServiceContainer _services;

        public RLanguageHandlerTest(IServiceContainer services) {
            _services = services;
        }

        [CompositeTest]
        [Category.Md.RCode]
        [InlineData("```{r}\n\n```")]
        [InlineData("```{r x=1}\nplot()\n```")]
        [InlineData("```{r x=1,\ny=2}\nx <- 1\n```")]
        [InlineData("```{r x=function() {\n}}\nx <- 1\n```")]
        [InlineData("```{r}\nparams$a = 3\nx <- 1\n```")]
        public void RCodeGen(string markdown) {
            var tb = new TextBufferMock(markdown, MdContentTypeDefinition.ContentType);
            var pbm = Substitute.For<IProjectionBufferManager>();

            string secondaryBuffer = null;
            ProjectionMapping[] mappings = null;
            pbm.When(x => x.SetProjectionMappings(Arg.Any<string>(), Arg.Any<ProjectionMapping[]>())).Do(x => {
                secondaryBuffer = ((string)x[0]);
                mappings = (ProjectionMapping[])x[1];
            });

            var expectedSecondaryBuffer = markdown.Replace("```", string.Empty) + Environment.NewLine;
            var handler = new RLanguageHandler(tb, pbm, _services);
            secondaryBuffer.Should().Be(expectedSecondaryBuffer);

            mappings.Should().HaveCount(1);
            mappings[0].SourceStart.Should().Be(3);
            mappings[0].ProjectionRange.Start.Should().Be(0);
            mappings[0].Length.Should().Be(expectedSecondaryBuffer.Length - Environment.NewLine.Length);
        }
    }
}
