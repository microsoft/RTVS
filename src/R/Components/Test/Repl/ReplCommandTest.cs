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
using Microsoft.R.Components.Controller;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Test;
using Microsoft.R.Support.Settings;
using Microsoft.UnitTests.Core.Threading;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.Editor.Mocks;
using Microsoft.VisualStudio.R.Package.Repl.Commands;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using NSubstitute;
using Xunit;

namespace Microsoft.R.Components.Test.Repl {
    [ExcludeFromCodeCoverage]
    [Collection(CollectionNames.NonParallel)]
    public class ReplCommandTest : IAsyncLifetime {
        private readonly MethodInfo _testMethod;
        private readonly ExportProvider _exportProvider;
        private readonly IRInteractiveWorkflow _workflow;
        private readonly IInteractiveWindowComponentContainerFactory _componentContainerFactory;

        public ReplCommandTest(RComponentsMefCatalogFixture catalog, TestMethodFixture testMethod) {
            _testMethod = testMethod.MethodInfo;
            _exportProvider = catalog.CreateExportProvider();
            _workflow = _exportProvider.GetExportedValue<IRInteractiveWorkflowProvider>().GetOrCreate();
            _componentContainerFactory = _exportProvider.GetExportedValue<IInteractiveWindowComponentContainerFactory>();
        }

        public async Task InitializeAsync() {
            await _workflow.RSession.StartHostAsync(new RHostStartupInfo {
                Name = _testMethod.Name,
                RBasePath = RToolsSettings.Current.RBasePath,
                RHostCommandLineArguments = RToolsSettings.Current.RCommandLineArguments,
                CranMirrorName = RToolsSettings.Current.CranMirror,
                CodePage = RToolsSettings.Current.RCodePage
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
            command.Status.Should().HaveFlag(CommandStatus.Supported | CommandStatus.Invisible)
                .And.NotHaveFlag(CommandStatus.Enabled);

            using (await UIThreadHelper.Instance.Invoke(() => _workflow.GetOrCreateVisualComponent(_componentContainerFactory))) {
                command.Status.Should().HaveFlag(CommandStatus.Supported)
                    .And.NotHaveFlag(CommandStatus.Enabled | CommandStatus.Invisible);

                const string code = "sourced <- TRUE";
                var textBuffer = new TextBufferMock(code, RContentTypeDefinition.ContentType);
                var textView = new WpfTextViewMock(textBuffer);

                tracker.GetLastActiveTextView(RContentTypeDefinition.ContentType).Returns(textView);

                command.Status.Should().HaveFlag(CommandStatus.Supported)
                    .And.NotHaveFlag(CommandStatus.Enabled | CommandStatus.Invisible);

                using (var sf = new SourceFile(code)) {
                    var document = new TextDocumentMock(textBuffer, sf.FilePath);
                    document.Encoding = Encoding.GetEncoding(encoding);
                    textBuffer.Properties[typeof(ITextDocument)] = document;

                    command.Status.Should().HaveFlag(CommandStatus.Supported | CommandStatus.Enabled)
                        .And.NotHaveFlag(CommandStatus.Invisible);

                    var mutatedTask = EventTaskSources.IRSession.Mutated.Create(session);

                    object outArg = null;
                    command.Invoke(null, ref outArg);

                    await mutatedTask;
                    (await session.EvaluateAsync<bool>("sourced", REvaluationKind.Normal)).Should().BeTrue();
                }
            }
        }
    }
}