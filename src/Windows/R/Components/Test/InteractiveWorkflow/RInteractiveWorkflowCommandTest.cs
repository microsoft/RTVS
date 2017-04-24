// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Components.ContentTypes;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Components.InteractiveWorkflow.Commands;
using Microsoft.R.Components.Settings;
using Microsoft.R.Components.Test.Fakes.Trackers;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Test;
using Microsoft.UnitTests.Core.FluentAssertions;
using Microsoft.UnitTests.Core.Threading;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.Editor.Mocks;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using NSubstitute;
using Xunit;

namespace Microsoft.R.Components.Test.InteractiveWorkflow {
    [ExcludeFromCodeCoverage]
    public class RInteractiveWorkflowCommandTest {
        private readonly IServiceContainer _services;
        private readonly IRInteractiveWorkflowVisual _workflow;
        private readonly IRSettings _settings;

        public RInteractiveWorkflowCommandTest(IServiceContainer services) {
            _services = services;
            _workflow = services.GetService<IRInteractiveWorkflowVisualProvider>().GetOrCreate();
            _settings = services.GetService<IRSettings>();
        }

        [CompositeTest(ThreadType.UI)]
        [Category.Repl]
        [InlineData(false, "utf-8")]
        [InlineData(true, "Windows-1252")]
        public async Task SourceRScriptTest(bool echo, string encoding) {
            await _workflow.RSessions.TrySwitchBrokerAsync(nameof(RInteractiveWorkflowCommandTest));
            await _workflow.RSession.EnsureHostStartedAsync(new RHostStartupInfo(
                cranMirrorName: _settings.CranMirror,
                codePage: _settings.RCodePage
            ), null, 50000);

            var session = _workflow.RSession;
            await session.ExecuteAsync("sourced <- FALSE");

            var tracker = Substitute.For<IActiveWpfTextViewTracker>();
            tracker.GetLastActiveTextView(RContentTypeDefinition.ContentType).Returns((IWpfTextView)null);

            var command = new SourceRScriptCommand(_workflow, tracker, echo);
            command.Should().BeSupported()
                .And.BeInvisibleAndDisabled();

            using (await _workflow.GetOrCreateVisualComponentAsync()) {
                const string code = "sourced <- TRUE";
                var textBuffer = new TextBufferMock(code, RContentTypeDefinition.ContentType);
                var textView = new WpfTextViewMock(textBuffer);

                tracker.GetLastActiveTextView(RContentTypeDefinition.ContentType).Returns(textView);
                tracker.LastActiveTextView.Returns(textView);

                command.Should().BeSupported()
                    .And.BeVisibleAndDisabled();

                using (var sf = new SourceFile(code)) {
                    var document = new TextDocumentMock(textBuffer, sf.FilePath) {
                        Encoding = Encoding.GetEncoding(encoding)
                    };

                    textBuffer.Properties[typeof(ITextDocument)] = document;

                    command.Should().BeSupported()
                        .And.BeVisibleAndEnabled();

                    var mutatedTask = EventTaskSources.IRSession.Mutated.Create(session);

                    await command.InvokeAsync().Should().BeCompletedAsync();

                    await mutatedTask.Should().BeCompletedAsync();
                    (await session.EvaluateAsync<bool>("sourced", REvaluationKind.Normal)).Should().BeTrue();
                }
            }
        }

        [Test]
        [Category.Repl]
        public async Task InterruptRStatusTest() {
            var debuggerModeTracker = _services.GetService<TestDebuggerModeTracker>();
            var command = new InterruptRCommand(_workflow, debuggerModeTracker);
            command.Should().BeInvisibleAndDisabled();

            await _workflow.RSessions.TrySwitchBrokerAsync(nameof(RInteractiveWorkflowCommandTest));
            using (await UIThreadHelper.Instance.Invoke(() => _workflow.GetOrCreateVisualComponentAsync())) {
                command.Should().BeVisibleAndDisabled();

                using (var interaction = await _workflow.RSession.BeginInteractionAsync()) {
                    var afterRequestTask = EventTaskSources.IRSession.AfterRequest.Create(_workflow.RSession);
                    var task = interaction.RespondAsync("while(TRUE) {}");
                    await afterRequestTask;
                    command.Should().BeVisibleAndEnabled();

                    debuggerModeTracker.IsInBreakMode = true;
                    command.Should().BeVisibleAndDisabled();

                    debuggerModeTracker.IsInBreakMode = false;
                    command.Should().BeVisibleAndEnabled();

                    await command.InvokeAsync();
                    command.Should().BeVisibleAndDisabled();

                    await task.Should().BeCanceledAsync();
                }
            }

            command.Should().BeVisibleAndDisabled();
        }

    }
}