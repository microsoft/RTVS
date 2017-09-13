// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// Based on https://github.com/CXuesong/LanguageServer.NET

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JsonRpc.Standard.Contracts;
using LanguageServer.VsCode.Contracts;
using Microsoft.Common.Core.Services;
using Microsoft.R.LanguageServer.Completions;
using Microsoft.R.LanguageServer.Documents;
using Microsoft.R.LanguageServer.Extensions;
using Microsoft.R.LanguageServer.Text;

namespace Microsoft.R.LanguageServer.Server {
    [JsonRpcScope(MethodPrefix = "textDocument/")]
    public sealed class TextDocumentService : LanguageServiceBase {
        public static IServiceContainer Services { get; set; }

        private IDocumentCollection _documents;
        private ITextManager _textManager;
        private ICompletionManager _completionManager;

        private IDocumentCollection Documents => _documents ?? (_documents = Services.GetService<IDocumentCollection>());
        private ITextManager TextManager => _textManager ?? (_textManager = Services.GetService<ITextManager>());
        private ICompletionManager CompletionManager => _completionManager ?? (_completionManager = Services.GetService<ICompletionManager>());

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
        public void didOpen(TextDocumentItem textDocument) => Documents.AddDocument(textDocument.Text, textDocument.Uri);

        [JsonRpcMethod(IsNotification = true)]
        public void didChange(TextDocumentIdentifier textDocument, ICollection<TextDocumentContentChangeEvent> contentChanges) {
            var entry = Documents.GetDocument(textDocument.Uri);
            if(entry == null) {
                return;
            }

            TextManager.ProcessTextChanges(entry, contentChanges);
        }

        [JsonRpcMethod(IsNotification = true)]
        public void willSave(TextDocumentIdentifier textDocument, TextDocumentSaveReason reason) { }

        [JsonRpcMethod(IsNotification = true)]
        public void didClose(TextDocumentIdentifier textDocument) => Documents.RemoveDocument(textDocument.Uri);

        [JsonRpcMethod]
        public CompletionList completion(TextDocumentIdentifier textDocument, Position position) {
            var entry = Documents.GetDocument(textDocument.Uri);
            return entry == null 
                ? new CompletionList() 
                : CompletionManager.GetCompletions(entry, entry.EditorBuffer.ToStreamPosition(position));
        }
    }
}
