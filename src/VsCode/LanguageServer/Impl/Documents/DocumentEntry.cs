// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LanguageServer.VsCode.Contracts;
using Microsoft.Common.Core.Services;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.Completions;
using Microsoft.Languages.Editor.Text;
using Microsoft.R.Editor.Completions;
using Microsoft.R.Editor.Document;
using Microsoft.R.LanguageServer.Client;
using Microsoft.R.LanguageServer.Completions;
using Microsoft.R.LanguageServer.Extensions;
using Microsoft.R.LanguageServer.Formatting;
using Microsoft.R.LanguageServer.Text;
using Microsoft.R.LanguageServer.Validation;

namespace Microsoft.R.LanguageServer.Documents {
    internal sealed class DocumentEntry : IDisposable {
        private readonly IServiceContainer _services;
        private readonly CompletionManager _completionManager;
        private readonly SignatureManager _signatureManager;
        private readonly DiagnosticsPublisher _diagnosticsPublisher;
        private readonly CodeFormatter _formatter;

        public IEditorView View { get; }
        public IEditorBuffer EditorBuffer { get; }
        public IREditorDocument Document { get; }

        public DocumentEntry(string content, Uri uri, IServiceContainer services) {
            _services = services;
 
            EditorBuffer = new EditorBuffer(content, "R");
            View = new EditorView(EditorBuffer);
            Document = new REditorDocument(EditorBuffer, services, false);

            _completionManager = new CompletionManager(services);
            _signatureManager = new SignatureManager(services);
            _diagnosticsPublisher = new DiagnosticsPublisher(services.GetService<IVsCodeClient>(), Document, uri, services);
            _formatter = new CodeFormatter(_services);
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

        [DebuggerStepThrough]
        public void Dispose() => Document?.Close();

        [DebuggerStepThrough]
        public CompletionList GetCompletions(Position position)
            => _completionManager.GetCompletions(CreateContext(position));

        public Task<SignatureHelp> GetSignatureHelpAsync(Position position)
            => _signatureManager.GetSignatureHelpAsync(CreateContext(position));

        [DebuggerStepThrough]
        public Task<Hover> GetHoverAsync(Position position, CancellationToken ct)
            => _signatureManager.GetHoverAsync(CreateContext(position), ct);

        [DebuggerStepThrough]
        public TextEdit[] Format() 
            => _formatter.Format(EditorBuffer.CurrentSnapshot);

        [DebuggerStepThrough]
        public TextEdit[] FormatRange(Range range) 
            => _formatter.FormatRange(EditorBuffer.CurrentSnapshot, range);

        private IRIntellisenseContext CreateContext(Position position) {
            var bufferPosition = EditorBuffer.ToStreamPosition(position);
            var session = new EditorIntellisenseSession(View, _services);
            return new RIntellisenseContext(session, EditorBuffer, Document.EditorTree, bufferPosition);
        }
    }
}
