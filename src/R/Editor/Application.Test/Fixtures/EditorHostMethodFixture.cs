// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading.Tasks;
using Microsoft.Common.Core.Test.Fakes.Shell;
using Microsoft.Language.Editor.Test.Settings;
using Microsoft.Languages.Editor.Application.Core;
using Microsoft.R.Editor.Settings;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Test.Script;
using Microsoft.R.Support.Help;
using Microsoft.UnitTests.Core.Mef;
using Microsoft.UnitTests.Core.XUnit.MethodFixtures;
using static Microsoft.UnitTests.Core.Threading.UIThreadTools;

namespace Microsoft.R.Editor.Application.Test {
    public class EditorHostMethodFixture : ContainerHostMethodFixture {
        public RHostScript HostScript { get; private set; }

        public IPackageIndex PackageIndex { get; private set; }
        public IFunctionIndex FunctionIndex { get; private set; }
        public IWritableREditorSettings Settings { get; private set; }

        public Task<IEditorScript> StartScript(IExportProvider exportProvider, string contentType) =>
            StartScript(exportProvider, string.Empty, "filename", contentType, null);

        public Task<IEditorScript> StartScript(IExportProvider exportProvider, string contentType, IRSessionProvider sessionProvider) =>
            StartScript(exportProvider, string.Empty, "filename", contentType, sessionProvider);

        public Task<IEditorScript> StartScript(IExportProvider exportProvider, string text, string contentType) =>
            StartScript(exportProvider, text, "filename", contentType, null);

        public async Task<IEditorScript> StartScript(IExportProvider exportProvider, string text, string filename, string contentType, IRSessionProvider sessionProvider) {
            var shell = new TestCoreShell(exportProvider);

            Settings = new REditorSettings(new TestSettingsStorage());
            shell.ServiceManager.AddService(Settings);

            var coreEditor = await InUI(() => new CoreEditor(shell, text, filename, contentType));
            var containerDisposable = await AddToHost(coreEditor.Control);

            if (sessionProvider != null) {
                IntelliSenseRSession.HostStartTimeout = 10000;
                HostScript = new RHostScript(sessionProvider);

                PackageIndex = exportProvider.GetExportedValue<IPackageIndex>();
                await PackageIndex.BuildIndexAsync();

                FunctionIndex = exportProvider.GetExportedValue<IFunctionIndex>();
                await FunctionIndex.BuildIndexAsync();
            }

            return new EditorScript(exportProvider, coreEditor, containerDisposable);
        }
    }
}
