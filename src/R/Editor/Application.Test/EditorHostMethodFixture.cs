// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading.Tasks;
using Microsoft.Languages.Editor.Application.Core;
using Microsoft.UnitTests.Core.Mef;
using Microsoft.UnitTests.Core.XUnit.MethodFixtures;

namespace Microsoft.R.Editor.Application.Test {
    public class EditorHostMethodFixture : ContainerHostMethodFixture {
        public async Task<IEditorScript> StartScript(IExportProvider exportProvider, string contentType) {
            var coreEditor = new CoreEditor(string.Empty, "filename", contentType);
            var containerDisposable = await AddToHost(coreEditor.Control);
            return new EditorScript(exportProvider, coreEditor, containerDisposable);
        }

        public async Task<IEditorScript> StartScript(IExportProvider exportProvider, string text, string contentType) {
            var coreEditor = new CoreEditor(text, "filename", contentType);
            var containerDisposable = await AddToHost(coreEditor.Control);
            return new EditorScript(exportProvider, coreEditor, containerDisposable);
        }
    }
}
