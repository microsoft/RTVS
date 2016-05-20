// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.R.Components.ContentTypes;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Components.InteractiveWorkflow.Commands;
using Microsoft.R.Components.Settings;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Test;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.Editor.Mocks;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using NSubstitute;
using Xunit;

namespace Microsoft.R.Components.Test.InteractiveWorkflow {
    [ExcludeFromCodeCoverage]
    public class RInteractiveWorkflowCommandTest : IAsyncLifetime {
        private readonly MethodInfo _testMethod;
        private readonly ExportProvider _exportProvider;
        private readonly IRInteractiveWorkflow _workflow;
        private readonly IInteractiveWindowComponentContainerFactory _componentContainerFactory;

        public RInteractiveWorkflowCommandTest(RComponentsMefCatalogFixture catalog, TestMethodFixture testMethod) {
            _testMethod = testMethod.MethodInfo;
            _exportProvider = catalog.CreateExportProvider();
            _workflow = _exportProvider.GetExportedValue<IRInteractiveWorkflowProvider>().GetOrCreate();
            _componentContainerFactory = _exportProvider.GetExportedValue<IInteractiveWindowComponentContainerFactory>();
        }

        public async Task InitializeAsync() {
            var settings = _exportProvider.GetExportedValue<IRSettings>();
            await _workflow.RSession.StartHostAsync(new RHostStartupInfo {
                Name = _testMethod.Name,
                RBasePath = settings.RBasePath,
                RHostCommandLineArguments = settings.RCommandLineArguments,
                CranMirrorName = settings.CranMirror,
                CodePage = settings.RCodePage
            }, null, 50000);
        }


        public Task DisposeAsync() {
            (_exportProvider as IDisposable)?.Dispose();
            return Task.CompletedTask;
        }

        [CompositeTest(ThreadType.UI)]
        [Category.Repl]
        [InlineData(false, "utf-8")]
        [InlineData(true, "Windows-1252")]
        public async Task SourceRScriptTest(bool echo, string encoding) {
            var session = _workflow.RSession;
            await session.ExecuteAsync("sourced <- FALSE");

            var tracker = Substitute.For<IActiveWpfTextViewTracker>();
            tracker.GetLastActiveTextView(RContentTypeDefinition.ContentType).Returns((IWpfTextView)null);

            var command = new SourceRScriptCommand(_workflow, tracker, echo);
            command.Should().BeSupported()
                .And.BeInvisibleAndDisabled();

            using (await _workflow.GetOrCreateVisualComponent(_componentContainerFactory)) {
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

                    await command.InvokeAsync();

                    await mutatedTask;
                    (await session.EvaluateAsync<bool>("sourced", REvaluationKind.Normal)).Should().BeTrue();
                }
            }
        }
    }
}