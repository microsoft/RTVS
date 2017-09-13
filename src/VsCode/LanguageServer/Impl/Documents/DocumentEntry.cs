// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core.Services;
using Microsoft.Languages.Editor.Text;
using Microsoft.R.Editor.Document;
using Microsoft.R.LanguageServer.Text;

namespace Microsoft.R.LanguageServer.Documents {
    internal sealed class DocumentEntry : IDisposable {
        public IEditorView View { get; }
        public IEditorBuffer EditorBuffer { get; }
        public IREditorDocument Document { get; }

        public void Dispose() => Document?.Close();

        public DocumentEntry(string content, IServiceContainer services) {
            EditorBuffer = new EditorBuffer(content, "R");
            View = new EditorView(EditorBuffer);
            Document = new REditorDocument(EditorBuffer, services, false);
        }
    }
}
