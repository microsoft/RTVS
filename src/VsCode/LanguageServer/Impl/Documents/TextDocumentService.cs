// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// Based on https://github.com/CXuesong/LanguageServer.NET

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JsonRpc.Standard.Contracts;
using LanguageServer.VsCode.Contracts;
using Microsoft.R.LanguageServer.Diagnostics;
using Microsoft.R.LanguageServer.Server;
using Microsoft.R.LanguageServer.Services;
using Microsoft.R.LanguageServer.Threading;

namespace Microsoft.R.LanguageServer.Documents {
    [JsonRpcScope(MethodPrefix = "textDocument/")]
    public sealed class TextDocumentService : LanguageServiceBase {
        private IDocumentCollection _documents;
        private IIdleTimeNotification _idleTimeNotification;
        private IMainThreadPriority _mainThread;

        private IMainThreadPriority MainThreadPriority => _mainThread ?? (_mainThread = Services.GetService<IMainThreadPriority>());
        private IDocumentCollection Documents => _documents ?? (_documents = Services.GetService<IDocumentCollection>());
        private IIdleTimeNotification IdleTimeNotification => _idleTimeNotification ?? (_idleTimeNotification = Services.GetService<IIdleTimeNotification>());

        [JsonRpcMethod]
        public async Task<Hover> Hover(TextDocumentIdentifier textDocument, Position position, CancellationToken ct) {
            using (new DebugMeasureTime("textDocument/hover")) {
                var doc = Documents.GetDocument(textDocument.Uri);
                return doc != null ? await doc.GetHoverAsync(position, ct) : null;
            }
        }

        [JsonRpcMethod]
        public async Task<SignatureHelp> SignatureHelp(TextDocumentIdentifier textDocument, Position position) {
            using (new DebugMeasureTime("textDocument/signatureHelp")) {
                var doc = Documents.GetDocument(textDocument.Uri);
                return doc != null ? await doc.GetSignatureHelpAsync(position) : null;
            }
        }

        [JsonRpcMethod(IsNotification = true)]
        public void didOpen(TextDocumentItem textDocument)
            => MainThreadPriority.Post(() => Documents.AddDocument(textDocument.Text, textDocument.Uri), ThreadPostPriority.Normal);

        [JsonRpcMethod(IsNotification = true)]
        public void didChange(TextDocumentIdentifier textDocument, ICollection<TextDocumentContentChangeEvent> contentChanges) {
            using (new DebugMeasureTime("textDocument/didChange")) {
                IdleTimeNotification.NotifyUserActivity();
                MainThreadPriority.Post(() => Documents.GetDocument(textDocument.Uri)?.ProcessChanges(contentChanges), ThreadPostPriority.Normal);
            }
        }

        [JsonRpcMethod(IsNotification = true)]
        public void willSave(TextDocumentIdentifier textDocument, TextDocumentSaveReason reason) { }

        [JsonRpcMethod(IsNotification = true)]
        public void didClose(TextDocumentIdentifier textDocument)
            => MainThreadPriority.Post(() => Documents.RemoveDocument(textDocument.Uri), ThreadPostPriority.Normal);

        [JsonRpcMethod]
        public CompletionList completion(TextDocumentIdentifier textDocument, Position position) {
            using (new DebugMeasureTime("textDocument/completion")) {
                var doc = Documents.GetDocument(textDocument.Uri);
                return doc?.GetCompletions(position);
            }
        }
    }
}
