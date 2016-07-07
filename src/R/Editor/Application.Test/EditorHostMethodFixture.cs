// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading.Tasks;
using Microsoft.Common.Core.Shell;
using Microsoft.Languages.Editor.Application.Core;
using Microsoft.UnitTests.Core.Mef;
using Microsoft.UnitTests.Core.XUnit.MethodFixtures;
using static Microsoft.UnitTests.Core.Threading.UIThreadTools;

namespace Microsoft.R.Editor.Application.Test {
    public class EditorHostMethodFixture : ContainerHostMethodFixture {
        public Task<IEditorScript> StartScript(IExportProvider exportProvider, string contentType) => StartScript(exportProvider, string.Empty, "filename", contentType);
        public Task<IEditorScript> StartScript(IExportProvider exportProvider, string text, string contentType) => StartScript(exportProvider, text, "filename", contentType);

        public async Task<IEditorScript> StartScript(IExportProvider exportProvider, string text, string filename, string contentType) {
            var shell = exportProvider.GetExportedValue<ICoreShell>();
            var coreEditor = await InUI(() => new CoreEditor(shell, text, filename, contentType));
            var containerDisposable = await AddToHost(coreEditor.Control);
            return new EditorScript(exportProvider, coreEditor, containerDisposable);
        }
    }
}
