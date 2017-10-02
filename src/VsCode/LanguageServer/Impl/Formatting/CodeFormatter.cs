// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using LanguageServer.VsCode.Contracts;
using Microsoft.Common.Core.Services;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.Text;
using Microsoft.R.Core.Formatting;
using Microsoft.R.Editor;
using Microsoft.R.Editor.Formatting;
using Microsoft.R.LanguageServer.Extensions;
using Microsoft.R.LanguageServer.Text;
using Microsoft.R.LanguageServer.Threading;

namespace Microsoft.R.LanguageServer.Formatting {
    internal sealed class CodeFormatter {
        private readonly IServiceContainer _services;

        public CodeFormatter(IServiceContainer services) {
            _services = services;
        }

        public TextEdit[] Format(IEditorBufferSnapshot snapshot) {
            var settings = _services.GetService<IREditorSettings>();
            var formatter = new RFormatter(settings.FormatOptions);
            var formattedText = formatter.Format(snapshot.GetText());
            return new[] {
                new TextEdit {
                    Range = TextRange.FromBounds(0, snapshot.Length).ToLineRange(snapshot),
                    NewText = formattedText
                }
            };
        }

        public TextEdit[] FormatRange(IEditorBufferSnapshot snapshot, Range range) {
            var changeHandler = new IncrementalTextChangeHandler();
            var rangeFormatter = new RangeFormatter(_services);
            rangeFormatter.FormatRange(null, snapshot.EditorBuffer, range.ToTextRange(snapshot), changeHandler);
            return changeHandler.Result;
        }

        public TextEdit[] Autoformat(IEditorBufferSnapshot snapshot, Position position, string typedChar) {
            var settings = _services.GetService<IREditorSettings>();
            var editorBuffer = snapshot.EditorBuffer;
            var formatter = new AutoFormat(new EditorView(editorBuffer), editorBuffer, _services);
            // Attach event handler for recording
            using (var recorder = new TextEditRecorder(editorBuffer)) { 
                formatter.HandleTyping(typedChar[0], position.ToStreamPosition(snapshot));
                return recorder.Edits.ToArray();
            };
        }

        private class TextEditRecorder : IDisposable {
            private readonly IEditorBuffer _editorBuffer;
            private readonly IEditorBufferSnapshot _snapshot;
            private readonly List<TextEdit> _edits = new List<TextEdit>();

            public TextEditRecorder(IEditorBuffer editorBuffer) {
                _editorBuffer = editorBuffer;
                _snapshot = _editorBuffer.CurrentSnapshot;
                _editorBuffer.Changed += OnTextChanged;
            }

            public IReadOnlyList<TextEdit> Edits => _edits;

            public void Dispose() => _editorBuffer.Changed -= OnTextChanged;

            private void OnTextChanged(object sender, TextChangeEventArgs e) {
                _edits.Add(new TextEdit {
                    Range = e.Change.OldRange.ToLineRange(_snapshot),
                    NewText = e.Change.NewText
                });
            }
        }
    }
}

