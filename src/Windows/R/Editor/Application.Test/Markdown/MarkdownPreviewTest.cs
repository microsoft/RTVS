// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.Common.Core.Services;
using Microsoft.Markdown.Editor.ContentTypes;
using Microsoft.Markdown.Editor.Settings;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Host.Client;
using Microsoft.UnitTests.Core.Threading;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;

namespace Microsoft.R.Editor.Application.Test.Markdown {
    [ExcludeFromCodeCoverage]
    [Collection(CollectionNames.NonParallel)]
    [Category.Interactive]
    public class MarkdownPreviewTest : IAsyncLifetime {
        private readonly IServiceContainer _services;
        private readonly IRMarkdownEditorSettings _settings;
        private readonly IRSessionProvider _sessionProvider;
        private readonly EditorHostMethodFixture _editorHost;
        private readonly EditorAppTestFilesFixture _files;

        public MarkdownPreviewTest(IServiceContainer services, EditorHostMethodFixture editorHost, EditorAppTestFilesFixture files) {
            _services = services;
            _sessionProvider = UIThreadHelper.Instance.Invoke(() => _services.GetService<IRInteractiveWorkflowProvider>().GetOrCreate()).RSessions;
            _editorHost = editorHost;
            _files = files;

            _settings = _services.GetService<IRMarkdownEditorSettings>();
            _settings.EnablePreview = true;
        }

        public Task InitializeAsync() => _sessionProvider.TrySwitchBrokerAsync(nameof(MarkdownPreviewTest));

        public Task DisposeAsync() => Task.CompletedTask;

        //[Test]
        public async Task PreviewAutomatic() {
            _settings.AutomaticSync = true;
            const string filename = "Preview01.rmd";
            var content = _files.LoadDestinationFile("Preview01.rmd");

            using (var script = await _editorHost.StartScript(_services, content, filename, MdContentTypeDefinition.ContentType, _sessionProvider)) {
                var actual = script.EditorText;
            }
        }
    }
}
