// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// Based on https://github.com/CXuesong/LanguageServer.NET

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JsonRpc.Standard.Contracts;
using LanguageServer.VsCode.Contracts;
using Microsoft.Common.Core.Services;
using Microsoft.Languages.Core;
using Microsoft.Languages.Editor.Completions;
using Microsoft.Languages.Editor.Text;
using Microsoft.R.Editor.Completions;
using Microsoft.R.Editor.Completions.Engine;
using Microsoft.R.Editor.Document;
using Microsoft.R.LanguageServer.Completions;
using Microsoft.R.LanguageServer.Extensions;
using Microsoft.R.LanguageServer.Text;

namespace Microsoft.R.LanguageServer.Server {
    [JsonRpcScope(MethodPrefix = "textDocument/")]
    public sealed class TextDocumentService : LanguageServiceBase {
        private readonly Guid _treeUserGuid = new Guid("DF3595E3-579C-48BD-9931-3E31F9FA7F46");
        public static IServiceContainer Services { get; set; }

        class DocumentEntry {
            public IEditorView View { get; set; }
            public IEditorBuffer EditorBuffer { get; set; }
            public IREditorDocument Document { get; set; }
        }

        private static readonly Dictionary<Uri, DocumentEntry> _documents = new Dictionary<Uri, DocumentEntry>();

        [JsonRpcMethod]
        public async Task<Hover> Hover(TextDocumentIdentifier textDocument, Position position, CancellationToken ct) {
            // Note that Hover is cancellable.
            await Task.Delay(1000, ct);
            return new Hover { Contents = "Test _hover_ @" + position + "\n\n" + textDocument };
        }

        [JsonRpcMethod]
        public SignatureHelp SignatureHelp(TextDocumentIdentifier textDocument, Position position) {
            return new SignatureHelp(new List<SignatureInformation>
            {
                new SignatureInformation("**Function1**", "Documentation1"),
                new SignatureInformation("**Function2** <strong>test</strong>", "Documentation2"),
            });
        }

        [JsonRpcMethod(IsNotification = true)]
        public void didOpen(TextDocumentItem textDocument) {
            var eb = new EditorBuffer(textDocument.Text, "R");
            var entry = new DocumentEntry {
                EditorBuffer = eb,
                View = new EditorView(eb),
                Document = new REditorDocument(eb, Services)
            };
            _documents[textDocument.Uri] = entry;
        }

        [JsonRpcMethod(IsNotification = true)]
        public void didChange(TextDocumentIdentifier textDocument, ICollection<TextDocumentContentChangeEvent> contentChanges) {
        }

        [JsonRpcMethod(IsNotification = true)]
        public void willSave(TextDocumentIdentifier textDocument, TextDocumentSaveReason reason) {
        }

        [JsonRpcMethod(IsNotification = true)]
        public void didClose(TextDocumentIdentifier textDocument) {
        }

        [JsonRpcMethod]
        public CompletionList completion(TextDocumentIdentifier textDocument, Position position) {

            if (!_documents.TryGetValue(textDocument.Uri, out DocumentEntry entry)) {
                return new CompletionList();
            }

            IReadOnlyCollection<IRCompletionListProvider> providers;
            RIntellisenseContext context;
            try {
                var root = entry.Document.EditorTree.AcquireReadLock(_treeUserGuid);
                var session = new EditorIntellisenseSession(entry.View);
                context = new RIntellisenseContext(session, entry.EditorBuffer, root, entry.EditorBuffer.ToStreamPosition(position));

                var completionEngine = new RCompletionEngine(Services);
                providers = completionEngine.GetCompletionForLocation(context);
            }
            finally {
                entry.Document.EditorTree.ReleaseReadLock(_treeUserGuid);
            }

            if (providers == null) {
                return new CompletionList();
            }

            var completions = new List<ICompletionEntry>();
            var sort = true;

            foreach (var provider in providers) {
                var entries = provider.GetEntries(context);
                if (entries.Count > 0) {
                    completions.AddRange(entries);
                }
                sort &= provider.AllowSorting;
            }

            if (sort) {
                completions.Sort(new CompletionEntryComparer(StringComparison.OrdinalIgnoreCase));
                completions.RemoveDuplicates(new CompletionEntryComparer(StringComparison.Ordinal));
            }

            var items = completions.Select(c => new CompletionItem(c.InsertionText, CompletionItemKind.Function, c.InsertionText, null));
            var list = new CompletionList(items);

            return list;
        }
    }
}
