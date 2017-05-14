// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Common.Core.Imaging;
using Microsoft.Common.Core.IO;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Editor.Completions.Providers;
using Microsoft.R.Host.Client;
using Microsoft.UnitTests.Core.Threading;
using Microsoft.UnitTests.Core.XUnit;
using NSubstitute;

namespace Microsoft.R.Editor.Test.Completions {
    [ExcludeFromCodeCoverage]
    [Category.R.Completion]
    public class FileCompletionProviderTest: IDisposable {
        private const string _testFolderName = "_Rtvs_FileCompletionTest_";

        private readonly IServiceContainer _services;
        private readonly string _testFolder;

        public FileCompletionProviderTest(IServiceContainer services) {
            _services = services;

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
            var services = new ServiceManager();
            services.AddService(new FileSystem());
            services.AddService(Substitute.For<IRInteractiveWorkflowProvider>());
            services.AddService(Substitute.For<IImageService>());
            var provider = new FilesCompletionProvider(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), services);
            var entries = provider.GetEntries(null);
            entries.Should().NotBeEmpty();
            entries.Should().Contain(e => e.DisplayText == _testFolderName);
        }

        [Test]
        public async Task RemoteFiles() {
            using (var workflow = UIThreadHelper.Instance.Invoke(() => _services.GetService<IRInteractiveWorkflowProvider>().GetOrCreate())) {
                await workflow.RSessions.TrySwitchBrokerAsync(nameof(FileCompletionProviderTest));
                await workflow.RSession.EnsureHostStartedAsync(new RHostStartupInfo(), null, 50000);

                var provider = new FilesCompletionProvider(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), _services, forceR: true);
                var entries = provider.GetEntries(null);
                entries.Should().NotBeEmpty();
                entries.Should().Contain(e => e.DisplayText == _testFolderName);
            }
        }
    }
}
