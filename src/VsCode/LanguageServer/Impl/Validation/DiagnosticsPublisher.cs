// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using LanguageServer.VsCode.Contracts;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.Threading;
using Microsoft.Languages.Editor.Text;
using Microsoft.R.Core.Parser;
using Microsoft.R.Editor;
using Microsoft.R.Editor.Document;
using Microsoft.R.Editor.Tree;
using Microsoft.R.Editor.Validation;
using Microsoft.R.Editor.Validation.Errors;
using Microsoft.R.LanguageServer.Client;
using Microsoft.R.LanguageServer.Extensions;
using Microsoft.R.LanguageServer.Threading;

namespace Microsoft.R.LanguageServer.Validation {
    internal sealed class DiagnosticsPublisher {
        private readonly IVsCodeClient _client;
        private readonly ConcurrentQueue<IValidationError> _resultsQueue;
        private readonly IMainThreadPriority _mainThread;
        private readonly IREditorSettings _settings;
        private readonly IIdleTimeService _idleTime;
        private readonly Uri _documentUri;
        private IREditorDocument _document;
        private List<Diagnostic> _lastDisgnostic = new List<Diagnostic>();

        public DiagnosticsPublisher(IVsCodeClient client, IREditorDocument document, Uri documentUri, IServiceContainer services) {
            _client = client;
            _document = document;
            _documentUri = documentUri;

            _settings = services.GetService<IREditorSettings>();
            _idleTime = services.GetService<IIdleTimeService>();
            _mainThread = services.GetService<IMainThreadPriority>();

            var validator = _document.EditorBuffer.GetService<TreeValidator>();
            validator.Cleared += OnCleared;
            _resultsQueue = validator.ValidationResults;
            _idleTime.Idle += OnIdle;

            _document.Closing += OnDocumentClosing;
            _document.EditorTree.UpdateCompleted += OnTreeUpdateCompleted;
        }

        private void OnIdle(object sender, EventArgs eventArgs) {
            if (!_settings.SyntaxCheckEnabled || _document == null) {
                return;
            }

            var errors = _resultsQueue.ToArray();
            var diagnostic = new List<Diagnostic>();

            foreach (var e in errors) {
                var range = GetRange(e);
                if (range != null) {
                    diagnostic.Add(new Diagnostic {
                        Message = e.Message,
                        Severity = ToDiagnosticSeverity(e.Severity),
                        Range = range.Value,
                        Source = "R"
                    });
                }
            }

            if (!diagnostic.SequenceEqual(_lastDisgnostic, new DiagnosticComparer())) {
                _lastDisgnostic = diagnostic;
                _mainThread.Post(() => _client.TextDocument.PublishDiagnostics(_documentUri, diagnostic), ThreadPostPriority.Idle);
            }
        }

        private static DiagnosticSeverity ToDiagnosticSeverity(ErrorSeverity s) {
            switch (s) {
                case ErrorSeverity.Warning:
                    return DiagnosticSeverity.Warning;
                case ErrorSeverity.Informational:
                    return DiagnosticSeverity.Information;
            }
            return DiagnosticSeverity.Error;
        }

        private Range? GetRange(IValidationError e) {
            try {
                return _document.EditorBuffer.ToLineRange(e.Start, e.End);
            } catch (ArgumentException) { }
            return null;
        }

        private void OnCleared(object sender, EventArgs e) => ClearAllDiagnostic();

        private void OnTreeUpdateCompleted(object sender, TreeUpdatedEventArgs e) {
            if (e.UpdateType != TreeUpdateType.PositionsOnly || _settings.LintOptions.Enabled) {
                ClearAllDiagnostic();
            }
        }

        private void ClearAllDiagnostic() {
            _client.TextDocument.PublishDiagnostics(_documentUri, new Diagnostic[0]);
        }

        private void OnDocumentClosing(object sender, EventArgs e) {
            if (_document != null) {
                _idleTime.Idle -= OnIdle;

                _document.EditorTree.UpdateCompleted -= OnTreeUpdateCompleted;
                _document.Closing -= OnDocumentClosing;
                _document = null;
            }
        }

        private class DiagnosticComparer : IEqualityComparer<Diagnostic> {
            public bool Equals(Diagnostic x, Diagnostic y)
                => x.Range == y.Range && x.Message == y.Message;

            public int GetHashCode(Diagnostic obj)
                => obj != null ? obj.GetHashCode() : 0;
        }
    }
}
