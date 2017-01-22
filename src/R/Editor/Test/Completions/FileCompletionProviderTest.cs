// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Editor.Completion.Providers;
using Microsoft.R.Editor.Imaging;
using Microsoft.R.Host.Client;
using Microsoft.UnitTests.Core.Mef;
using Microsoft.UnitTests.Core.Threading;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.Language.Intellisense;
using NSubstitute;

namespace Microsoft.R.Editor.Test.Completions {
    [ExcludeFromCodeCoverage]
    [Category.R.Completion]
    public class FileCompletionProviderTest: IDisposable {
        private const string _testFolderName = "_Rtvs_FileCompletionTest_";

        private readonly IExportProvider _exportProvider;

        private readonly IImagesProvider _imagesProvider;
        private readonly IGlyphService _glyphService;
        private readonly string _testFolder;

        public FileCompletionProviderTest(IExportProvider exportProvider) {
            _exportProvider = exportProvider;

            _imagesProvider = Substitute.For<IImagesProvider>();
            _glyphService = Substitute.For<IGlyphService>();

            var myDocs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            _testFolder = Path.Combine(myDocs, _testFolderName);
            if (!Directory.Exists(_testFolder)) {
                Directory.CreateDirectory(_testFolder);
            }
        }

        public void Dispose() {
            if (Directory.Exists(_testFolder)) {
                Directory.Delete(_testFolder);
            }
        }

        [Test]
        public void LocalFiles() {
            var workflow = Substitute.For<IRInteractiveWorkflow>();

            var provider = new FilesCompletionProvider(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), workflow, _imagesProvider, _glyphService);
            var entries = provider.GetEntries(null);
            entries.Should().NotBeEmpty();
            entries.Should().Contain(e => e.DisplayText == _testFolderName);
        }

        [Test]
        public async Task RemoteFiles() {
            using (var workflow = UIThreadHelper.Instance.Invoke(() => _exportProvider.GetExportedValue<IRInteractiveWorkflowProvider>().GetOrCreate())) {
                await workflow.RSessions.TrySwitchBrokerAsync(nameof(FileCompletionProviderTest));
                await workflow.RSession.EnsureHostStartedAsync(new RHostStartupInfo(), null, 50000);

                var provider = new FilesCompletionProvider(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), workflow, _imagesProvider, _glyphService, forceR: true);
                var entries = provider.GetEntries(null);
                entries.Should().NotBeEmpty();
                entries.Should().Contain(e => e.DisplayText == _testFolderName);
            }
        }
    }
}
