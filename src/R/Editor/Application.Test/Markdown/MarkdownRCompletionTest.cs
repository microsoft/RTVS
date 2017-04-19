// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Common.Core.Shell;
using Microsoft.Markdown.Editor.ContentTypes;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Host.Client;
using Microsoft.UnitTests.Core.Threading;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.Language.Intellisense;
using Xunit;

namespace Microsoft.R.Editor.Application.Test.Markdown {
    [ExcludeFromCodeCoverage]
    [Collection(CollectionNames.NonParallel)]
    public class MarkdownRCompletionTest : IAsyncLifetime {
        private readonly ICoreShell _coreShell;
        private readonly IRSessionProvider _sessionProvider;
        private readonly EditorHostMethodFixture _editorHost;

        public MarkdownRCompletionTest(REditorApplicationShellProviderFixture shellProvider, EditorHostMethodFixture editorHost) {
            _coreShell = shellProvider.CoreShell;
            _sessionProvider = UIThreadHelper.Instance.Invoke(() => _coreShell.GetService<IRInteractiveWorkflowProvider>().GetOrCreate()).RSessions;
            _editorHost = editorHost;
        }

        public Task InitializeAsync() => _sessionProvider.TrySwitchBrokerAsync(nameof(MarkdownRCompletionTest));

        public Task DisposeAsync() => Task.CompletedTask;

        [Test]
        [Category.Interactive]
        public async Task TypeRBlock() {
            using (var script = await _editorHost.StartScript(_coreShell, string.Empty, "filename", MdContentTypeDefinition.ContentType, _sessionProvider)) {
                var info = await _editorHost.FunctionIndex.GetFunctionInfoAsync("abbreviate");
                info.Should().NotBeNull();

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

                string expected =
@"```{r}
x <- function() {
    abbreviate()
}
```";
                string actual = script.EditorText;
                actual.Should().Be(expected);
            }
        }

        [Test]
        [Category.Interactive]
        public async Task RSignature() {
            using (var script = await _editorHost.StartScript(_coreShell, "```{r}\r\n\r\n```", "filename", MdContentTypeDefinition.ContentType, _sessionProvider)) {
                var info = await _editorHost.FunctionIndex.GetFunctionInfoAsync("lm");
                info.Should().NotBeNull();

                script.DoIdle(500);
                script.MoveDown();
                script.Type("x <- lm(");
                script.DoIdle(2000);

                ISignatureHelpSession session = script.GetSignatureSession();
                session.Should().NotBeNull();
                IParameter parameter = session.SelectedSignature.CurrentParameter;
                parameter.Should().NotBeNull();
                parameter.Name.Should().Be("formula");

                script.Type("sub");
                script.DoIdle(500);
                script.Type("{TAB}");
                script.DoIdle(1000);

                parameter = session.SelectedSignature.CurrentParameter;
                parameter.Name.Should().Be("subset");

                string actual = script.EditorText;
                actual.Should().Be("```{r}\r\nx <- lm(subset = )\r\n```");

                session = script.GetSignatureSession();
                parameter = session.SelectedSignature.CurrentParameter;
            }
        }
    }
}
