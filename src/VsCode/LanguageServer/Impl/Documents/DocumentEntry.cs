// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using LanguageServer.VsCode.Contracts;
using Microsoft.Common.Core.Services;
using Microsoft.Languages.Core;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.Completions;
using Microsoft.Languages.Editor.Text;
using Microsoft.R.Editor.Completions;
using Microsoft.R.Editor.Completions.Engine;
using Microsoft.R.Editor.Document;
using Microsoft.R.LanguageServer.Completions;
using Microsoft.R.LanguageServer.Extensions;
using Microsoft.R.LanguageServer.Text;

namespace Microsoft.R.LanguageServer.Documents {
    internal sealed class DocumentEntry : IDisposable {
        private readonly IServiceContainer _services;

        public IEditorView View { get; }
        public IEditorBuffer EditorBuffer { get; }
        public IREditorDocument Document { get; }

        public DocumentEntry(string content, IServiceContainer services) {
            _services = services;

            EditorBuffer = new EditorBuffer(content, "R");
            View = new EditorView(EditorBuffer);
            Document = new REditorDocument(EditorBuffer, services, false);
        }

        public void ProcessChanges(ICollection<TextDocumentContentChangeEvent> contentChanges) {
            foreach (var change in contentChanges) {
                if (!change.HasRange) {
                    continue;
                }
                var position = EditorBuffer.ToStreamPosition(change.Range.Start);
                var range = new TextRange(position, change.RangeLength);
                if (!string.IsNullOrEmpty(change.Text)) {
                    // Insert or replace
                    if (change.RangeLength == 0) {
                        EditorBuffer.Insert(position, change.Text);
                    } else {
                        EditorBuffer.Replace(range, change.Text);
                    }
                } else {
                    EditorBuffer.Delete(range);
                }
            }
        }

        public void Dispose() => Document?.Close();

        public CompletionList GetCompletions(Position position) {
            var root = Document.EditorTree.AstRoot;
            var bufferPosition = EditorBuffer.ToStreamPosition(position);

            var session = new EditorIntellisenseSession(View);
            var context = new RIntellisenseContext(session, EditorBuffer, root, bufferPosition);

            var completionEngine = new RCompletionEngine(_services);
            var providers = completionEngine.GetCompletionForLocation(context);

            if (providers == null || providers.Count == 0) {
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
