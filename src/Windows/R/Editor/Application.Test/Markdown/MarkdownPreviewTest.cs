// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Threading;
using Microsoft.Markdown.Editor.ContentTypes;
using Microsoft.Markdown.Editor.Preview.Margin;
using Microsoft.Markdown.Editor.Settings;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Host.Client;
using Microsoft.UnitTests.Core.Threading;
using Microsoft.UnitTests.Core.XUnit;
using mshtml;
using Microsoft.Common.Core.Test.Utility;
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

        [CompositeTest]
        [InlineData("Preview01.rmd")]
        public async Task PreviewAutomatic(string sourceFile) {
            _settings.AutomaticSync = true;
            var content = _files.LoadDestinationFile(sourceFile);
            var baselinePath = Path.Combine(_files.DestinationPath, sourceFile) + ".html";

            using (var script = await _editorHost.StartScript(_services, content, sourceFile, MdContentTypeDefinition.ContentType, _sessionProvider)) {
                script.DoIdle(500);
                var margin = script.View.Properties.GetProperty<PreviewMargin>(typeof(PreviewMargin));
                var control = margin?.Browser?.Control;
                control.Should().NotBeNull();

                await _services.MainThread().SwitchToAsync();
                var htmlDoc = (HTMLDocument) control.Document;
                var actual = htmlDoc.documentElement.outerHTML.Replace("\n", "\r\n");
                TestFiles.CompareToBaseLine(baselinePath, actual);
            }
        }
    }
}
