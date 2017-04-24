// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading.Tasks;
using Microsoft.Common.Core.Shell;
using Microsoft.Languages.Editor.Application.Core;
using Microsoft.R.Editor.Functions;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Test.Script;
using Microsoft.UnitTests.Core.XUnit.MethodFixtures;
using static Microsoft.UnitTests.Core.Threading.UIThreadTools;

namespace Microsoft.R.Editor.Application.Test {
    public class EditorHostMethodFixture : ContainerHostMethodFixture {
        public RHostScript HostScript { get; private set; }
        public IPackageIndex PackageIndex { get; private set; }
        public IFunctionIndex FunctionIndex { get; private set; }

        public Task<IEditorScript> StartScript(ICoreShell coreShell, string contentType) =>
            StartScript(coreShell, string.Empty, "filename", contentType, null);

        public Task<IEditorScript> StartScript(ICoreShell coreShell, string contentType, IRSessionProvider sessionProvider) =>
            StartScript(coreShell, string.Empty, "filename", contentType, sessionProvider);

        public Task<IEditorScript> StartScript(ICoreShell coreShell, string text, string contentType) =>
            StartScript(coreShell, text, "filename", contentType, null);

        public async Task<IEditorScript> StartScript(ICoreShell coreShell, string text, string filename, string contentType, IRSessionProvider sessionProvider) {
            var coreEditor = await InUI(() => new CoreEditor(coreShell, text, filename, contentType));
            var containerDisposable = await AddToHost(coreEditor.Control);

            if (sessionProvider != null) {
                IntelliSenseRSession.HostStartTimeout = 10000;
                HostScript = new RHostScript(sessionProvider);

                PackageIndex = coreShell.GetService<IPackageIndex>();
                await PackageIndex.BuildIndexAsync();

                FunctionIndex = coreShell.GetService<IFunctionIndex>();
                await FunctionIndex.BuildIndexAsync();
            }

            return new EditorScript(coreShell, coreEditor, containerDisposable);
        }
    }
}
