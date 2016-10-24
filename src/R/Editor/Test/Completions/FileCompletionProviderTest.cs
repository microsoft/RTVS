// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Languages.Core.Text;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Editor.Completion.Providers;
using Microsoft.R.Editor.Imaging;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Session;
using Microsoft.R.Host.Client.Test.Script;
using Microsoft.UnitTests.Core.Mef;
using Microsoft.UnitTests.Core.Threading;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.Language.Intellisense;
using NSubstitute;
using Xunit;

namespace Microsoft.R.Editor.Test.Completions {
    [ExcludeFromCodeCoverage]
    [Category.R.Completion]
    public class FileCompletionProviderTest {
        private readonly REditorMefCatalogFixture _catalog;
        private readonly IExportProvider _exportProvider;

        private readonly IImagesProvider _imagesProvider;
        private readonly IGlyphService _glyphService;

        public FileCompletionProviderTest(REditorMefCatalogFixture catalog) {
            _catalog = catalog;
            _exportProvider = _catalog.CreateExportProvider();

            _imagesProvider = Substitute.For<IImagesProvider>();
            _glyphService = Substitute.For<IGlyphService>();
        }

        [Test]
        public void LocalFiles() {
            var workflow = Substitute.For<IRInteractiveWorkflow>();

            var provider = new FilesCompletionProvider(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), workflow, _imagesProvider, _glyphService);
            var entries = provider.GetEntries(null);
            entries.Should().NotBeEmpty();
            entries.Should().Contain(e => e.DisplayText == "MyLibrary");
        }

        [Test]
        public void RemoteFiles() {
            var completionSets = new List<CompletionSet>();
            var workflowProvider = UIThreadHelper.Instance.Invoke(() => _exportProvider.GetExportedValue<IRInteractiveWorkflowProvider>().GetOrCreate());
            using (var script = new RHostScript(workflowProvider.RSessions)) {
                var provider = new FilesCompletionProvider(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), workflowProvider, _imagesProvider, _glyphService, forceR: true);
                var entries = provider.GetEntries(null);
                entries.Should().NotBeEmpty();
                entries.Should().Contain(e => e.DisplayText == "MyLibrary");
            }
        }
    }
}
