// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Common.Core.Services;
using Microsoft.Markdown.Editor.ContentTypes;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Host.Client;
using Microsoft.UnitTests.Core.Threading;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;

namespace Microsoft.R.Editor.Application.Test.Markdown {
    [ExcludeFromCodeCoverage]
    [Collection(CollectionNames.NonParallel)]
    public class MarkdownRCompletionTest : IAsyncLifetime {
        private readonly IServiceContainer _services;
        private readonly IRSessionProvider _sessionProvider;
        private readonly EditorHostMethodFixture _editorHost;

        public MarkdownRCompletionTest(IServiceContainer services, EditorHostMethodFixture editorHost) {
            _services = services;
            _sessionProvider = UIThreadHelper.Instance.Invoke(() => _services.GetService<IRInteractiveWorkflowProvider>().GetOrCreate()).RSessions;
            _editorHost = editorHost;
        }

        public Task InitializeAsync() => _sessionProvider.TrySwitchBrokerAsync(nameof(MarkdownRCompletionTest));

        public Task DisposeAsync() => Task.CompletedTask;

        [Test]
        [Category.Interactive]
        public async Task TypeRBlock() {
            using (var script = await _editorHost.StartScript(_services, string.Empty, "filename", MdContentTypeDefinition.ContentType, _sessionProvider)) {
                var packageName = await _editorHost.FunctionIndex.GetPackageNameAsync("abbreviate");
                packageName.Should().NotBeNull();

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
                script.Type("{ENTER}abbr{TAB}(");

                var expected =
@"```{r}
x <- function() {
    abbreviate()
}
```";
                var actual = script.EditorText;
                actual.Should().Be(expected);
            }
        }

        [Test]
        [Category.Interactive]
        public async Task RSignature() {
            using (var script = await _editorHost.StartScript(_services, "```{r}\r\n\r\n```", "filename", MdContentTypeDefinition.ContentType, _sessionProvider)) {
                var packageName = await _editorHost.FunctionIndex.GetPackageNameAsync("lm");
                packageName.Should().NotBeNull();

                script.DoIdle(500);
                script.MoveDown();
                script.Type("x <- lm(");
                script.DoIdle(2000);

                var session = script.GetSignatureSession();
                session.Should().NotBeNull();
                var parameter = session.SelectedSignature.CurrentParameter;
                parameter.Should().NotBeNull();
                parameter.Name.Should().Be("formula");

                script.Type("sub");
                script.DoIdle(500);
                script.Type("{TAB}");
                script.DoIdle(1000);

                parameter = session.SelectedSignature.CurrentParameter;
                parameter.Name.Should().Be("subset");

                var actual = script.EditorText;
                actual.Should().Be("```{r}\r\nx <- lm(subset = )\r\n```");
            }
        }
    }
}
