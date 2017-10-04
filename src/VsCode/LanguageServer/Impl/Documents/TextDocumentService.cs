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
using TextEdit = LanguageServer.VsCode.Contracts.TextEdit;

namespace Microsoft.R.LanguageServer.Documents {
    [JsonRpcScope(MethodPrefix = "textDocument/")]
    public sealed class TextDocumentService : LanguageServiceBase {
        private static volatile bool _ignoreNextChange;

        private IDocumentCollection _documents;
        private IIdleTimeNotification _idleTimeNotification;
        private IMainThreadPriority _mainThread;

        private IMainThreadPriority MainThreadPriority => _mainThread ?? (_mainThread = Services.GetService<IMainThreadPriority>());
        private IDocumentCollection Documents => _documents ?? (_documents = Services.GetService<IDocumentCollection>());
        private IIdleTimeNotification IdleTimeNotification => _idleTimeNotification ?? (_idleTimeNotification = Services.GetService<IIdleTimeNotification>());

        [JsonRpcMethod]
        public Task<Hover> Hover(TextDocumentIdentifier textDocument, Position position, CancellationToken ct) {
            using (new DebugMeasureTime("textDocument/hover")) {
                var doc = Documents.GetDocument(textDocument.Uri);
                return doc != null ? doc.GetHoverAsync(position, ct) : Task.FromResult((Hover)null);
            }
        }

        [JsonRpcMethod]
        public async Task<SignatureHelp> SignatureHelp(TextDocumentIdentifier textDocument, Position position) {
            using (new DebugMeasureTime("textDocument/signatureHelp")) {
                var doc = Documents.GetDocument(textDocument.Uri);
                return doc != null ? await doc.GetSignatureHelpAsync(position) : new SignatureHelp();
            }
        }

        [JsonRpcMethod(IsNotification = true)]
        public void didOpen(TextDocumentItem textDocument)
            => MainThreadPriority.Post(() => Documents.AddDocument(textDocument.Text, textDocument.Uri), ThreadPostPriority.Normal);

        [JsonRpcMethod(IsNotification = true)]
        public void didChange(TextDocumentIdentifier textDocument, ICollection<TextDocumentContentChangeEvent> contentChanges) {
            if (_ignoreNextChange) {
                _ignoreNextChange = false;
                return;
            }

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
        public async Task<CompletionList> completion(TextDocumentIdentifier textDocument, Position position) {
            using (new DebugMeasureTime("textDocument/completion")) {
                var doc = Documents.GetDocument(textDocument.Uri);
                return doc != null ? await doc.GetCompletions(position) : new CompletionList();
            }
        }

        [JsonRpcMethod]
        public async Task<TextEdit[]> formatting(TextDocumentIdentifier textDocument, FormattingOptions options) {
            using (new DebugMeasureTime("textDocument/formatting")) {
                var doc = Documents.GetDocument(textDocument.Uri);
                var result = await doc.FormatAsync();
                _ignoreNextChange = result.Length > 0;
                return result;
            }
        }

        [JsonRpcMethod]
        public async Task<TextEdit[]> rangeFormatting(TextDocumentIdentifier textDocument, Range range, FormattingOptions options) {
            using (new DebugMeasureTime("textDocument/rangeFormatting")) {
                var doc = Documents.GetDocument(textDocument.Uri);
                var result = await doc.FormatRangeAsync(range);
                _ignoreNextChange = result.Length > 0;
                return result;
            }
        }

        [JsonRpcMethod]
        public async Task<TextEdit[]> onTypeFormatting(TextDocumentIdentifier textDocument, Position position, string ch, FormattingOptions options) {
            using (new DebugMeasureTime("textDocument/onTypeFormatting")) {
                var doc = Documents.GetDocument(textDocument.Uri);
                var result = await doc.AutoformatAsync(position, ch);
                _ignoreNextChange = result.Length > 0;
                return result;
            }
        }

        [JsonRpcMethod]
        public SymbolInformation[] documentSymbol(TextDocumentIdentifier textDocument, FormattingOptions options) {
            using (new DebugMeasureTime("textDocument/documentSymbol")) {
                var doc = Documents.GetDocument(textDocument.Uri);
                return doc != null ? doc.GetSymbols(textDocument.Uri) : new SymbolInformation[0];
            }
        }
    }
}
