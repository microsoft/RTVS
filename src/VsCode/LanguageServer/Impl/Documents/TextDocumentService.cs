// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// Based on https://github.com/CXuesong/LanguageServer.NET

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JsonRpc.Standard.Contracts;
using LanguageServer.VsCode.Contracts;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.Text;
using Microsoft.R.Core.Tokens;
using Microsoft.R.LanguageServer.Diagnostics;
using Microsoft.R.LanguageServer.Extensions;
using Microsoft.R.LanguageServer.Server;
using Microsoft.R.LanguageServer.Services;
using Microsoft.R.LanguageServer.Text;
using Microsoft.R.LanguageServer.Threading;

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
        public Task<TextEdit[]> formatting(TextDocumentIdentifier textDocument, FormattingOptions options) {
            using (new DebugMeasureTime("textDocument/formatting")) {
                var doc = Documents.GetDocument(textDocument.Uri);
                return DoFormatActionAsync(doc, Task.FromResult(doc.Format()));
            }
        }

        [JsonRpcMethod]
        public Task<TextEdit[]> rangeFormatting(TextDocumentIdentifier textDocument, Range range, FormattingOptions options) {
            using (new DebugMeasureTime("textDocument/rangeFormatting")) {
                var doc = Documents.GetDocument(textDocument.Uri);
                return DoFormatActionAsync(doc, Task.FromResult(doc.FormatRange(range)));
            }
        }

        [JsonRpcMethod]
        public Task<TextEdit[]> onTypeFormatting(TextDocumentIdentifier textDocument, Position position, string ch, FormattingOptions options) {
            using (new DebugMeasureTime("textDocument/onTypeFormatting")) {
                var doc = Documents.GetDocument(textDocument.Uri);
                return DoFormatActionAsync(doc, doc.AutoformatAsync(position, ch));
            }
        }

        [JsonRpcMethod]
        public SymbolInformation[] documentSymbol(TextDocumentIdentifier textDocument, FormattingOptions options) {
            using (new DebugMeasureTime("textDocument/documentSymbol")) {
                var doc = Documents.GetDocument(textDocument.Uri);
                return doc != null ? doc.GetSymbols(textDocument.Uri) : new SymbolInformation[0];
            }
        }

        private async Task<TextEdit[]> DoFormatActionAsync(DocumentEntry doc, Task t) {
            TextEdit[] result;
            if (doc != null) {
                var before = doc.EditorBuffer.CurrentSnapshot;
                await t;
                var after = doc.EditorBuffer.CurrentSnapshot;
                result = GetDifference(before, after);
            } else {
                result = new TextEdit[0];
            }
            _ignoreNextChange = result.Length > 0;
            return result;
        }

        private TextEdit[] GetDifference(IEditorBufferSnapshot before, IEditorBufferSnapshot after) {
            var tokenizer = new RTokenizer();
            var oldTokens = tokenizer.Tokenize(before.GetText());
            var newTokens = tokenizer.Tokenize(after.GetText());

            if (newTokens.Count != oldTokens.Count) {
                return new[] { new TextEdit {
                    NewText = after.GetText(),
                    Range = TextRange.FromBounds(0, before.Length).ToLineRange(before)
                }};
            }

            var edits = new List<TextEdit>();
            var oldEnd = before.Length;
            var newEnd = after.Length;
            for (var i = newTokens.Count - 1; i >= 0; i--) {
                var oldText = before.GetText(TextRange.FromBounds(oldTokens[i].End, oldEnd));
                var newText = after.GetText(TextRange.FromBounds(newTokens[i].End, newEnd));
                if (oldText != newText) {
                    var range = new TextRange(oldTokens[i].End, oldEnd - oldTokens[i].End);
                    edits.Add(new TextEdit {
                        Range = range.ToLineRange(before),
                        NewText = newText
                    });

                }
                oldEnd = oldTokens[i].Start;
                newEnd = newTokens[i].Start;
            }

            var r = new TextRange(0, oldEnd);
            edits.Add(new TextEdit {
                NewText = after.GetText(TextRange.FromBounds(0, newEnd)),
                Range = r.ToLineRange(before)
            });

            return edits.ToArray();
        }
    }
}
