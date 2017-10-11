// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// Based on https://github.com/CXuesong/LanguageServer.NET

using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        /// <remarks>
        /// In VS Code editor operations such as formatting are not supposed
        /// to change local copy of the text buffer. Instead, they return
        /// a set of edits that VS Code applies to its buffer and then sends
        /// <see cref="didChange(TextDocumentIdentifier, ICollection{TextDocumentContentChangeEvent})"/>
        /// event. However, existing R formatters works by modifying underlying buffer.
        /// Therefore, in formatting operations we let formatter to change local copy 
        /// of the buffer, then calculate difference with the original state and send edits
        /// to VS Code, which then will ivokes 'didChange'. Since local buffer is already 
        /// up to date, we must ignore this call.
        /// </remarks>
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
        public Task<SignatureHelp> SignatureHelp(TextDocumentIdentifier textDocument, Position position) {
            using (new DebugMeasureTime("textDocument/signatureHelp")) {
                return MainThreadPriority.SendAsync(async () => {
                    var doc = Documents.GetDocument(textDocument.Uri);
                    return doc != null ? await doc.GetSignatureHelpAsync(position) : new SignatureHelp();
                }, ThreadPostPriority.Background);
            }
        }

        [JsonRpcMethod(IsNotification = true)]
        public void didOpen(TextDocumentItem textDocument)
            => MainThreadPriority.Post(() => Documents.AddDocument(textDocument.Text, textDocument.Uri), ThreadPostPriority.Normal);

        [JsonRpcMethod(IsNotification = true)]
        public async Task didChange(TextDocumentIdentifier textDocument, ICollection<TextDocumentContentChangeEvent> contentChanges) {
            if (_ignoreNextChange) {
                _ignoreNextChange = false;
                return;
            }

            IdleTimeNotification.NotifyUserActivity();

            using (new DebugMeasureTime("textDocument/didChange")) {
                await MainThreadPriority.SendAsync(async () => {
                    var doc = Documents.GetDocument(textDocument.Uri);
                    if (doc != null) {
                        await doc.ProcessChangesAsync(contentChanges);
                    }
                     return true;
                }, ThreadPostPriority.Normal);
            }
        }

        [JsonRpcMethod(IsNotification = true)]
        public void willSave(TextDocumentIdentifier textDocument, TextDocumentSaveReason reason) { }

        [JsonRpcMethod(IsNotification = true)]
        public void didClose(TextDocumentIdentifier textDocument)
            => MainThreadPriority.Post(() => Documents.RemoveDocument(textDocument.Uri), ThreadPostPriority.Normal);

        [JsonRpcMethod]
        public Task<CompletionList> completion(TextDocumentIdentifier textDocument, Position position) {
            using (new DebugMeasureTime("textDocument/completion")) {
                return MainThreadPriority.SendAsync(() => {
                    var doc = Documents.GetDocument(textDocument.Uri);
                    return Task.FromResult(doc != null ? doc.GetCompletions(position) : new CompletionList());
                }, ThreadPostPriority.Background);
            }
        }

        [JsonRpcMethod]
        public Task<TextEdit[]> formatting(TextDocumentIdentifier textDocument, FormattingOptions options) {
            using (new DebugMeasureTime("textDocument/formatting")) {
                return MainThreadPriority.SendAsync(async () => {
                    var doc = Documents.GetDocument(textDocument.Uri);
                    var result = doc != null ? await doc.FormatAsync() : new TextEdit[0];
                    _ignoreNextChange = result.Length > 0;
                    return result;
                }, ThreadPostPriority.Background);
            }
        }

        [JsonRpcMethod]
        public Task<TextEdit[]> rangeFormatting(TextDocumentIdentifier textDocument, Range range, FormattingOptions options) {
            using (new DebugMeasureTime("textDocument/rangeFormatting")) {
                return MainThreadPriority.SendAsync(async () => {
                    var doc = Documents.GetDocument(textDocument.Uri);
                    var result = await doc.FormatRangeAsync(range);
                    _ignoreNextChange = result.Length > 0;
                    return result;
                }, ThreadPostPriority.Background);
            }
        }

        [JsonRpcMethod]
        public Task<TextEdit[]> onTypeFormatting(TextDocumentIdentifier textDocument, Position position, string ch, FormattingOptions options) {
            using (new DebugMeasureTime("textDocument/onTypeFormatting")) {
                return MainThreadPriority.SendAsync(async () => {
                    var doc = Documents.GetDocument(textDocument.Uri);
                    var result = await doc.AutoformatAsync(position, ch);
                    _ignoreNextChange = result.Length > 0;
                    return result;
                }, ThreadPostPriority.Background);
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
