// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Shell;
using Microsoft.Languages.Editor.Composition;
using Microsoft.Languages.Editor.Document;
using Microsoft.Languages.Editor.Text;
using Microsoft.R.Components.ContentTypes;
using Microsoft.R.Core.AST;
using Microsoft.R.Editor.Commands;
using Microsoft.R.Editor.Document;
using Microsoft.R.Editor.SuggestedActions.Definitions;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.R.Editor.SuggestedActions {
    internal sealed class RSuggestedActionSource : ISuggestedActionsSource {
        private readonly IEnumerable<IRSuggestedActionProvider> _suggestedActionProviders;
        private ITextBuffer _textBuffer;
        private ITextView _textView;
        IREditorDocument _document;
        IAstNode _lastNode;

        public RSuggestedActionSource(ITextView textView, ITextBuffer textBuffer, IEnumerable<IRSuggestedActionProvider> suggestedActionProviders, ICoreShell shell) {
            _textBuffer = textBuffer;
            _textView = textView;
            _textView.Caret.PositionChanged += OnCaretPositionChanged;
            _suggestedActionProviders = suggestedActionProviders;

            _document = _textBuffer.GetEditorDocument<IREditorDocument>();
            _document.Closing += OnDocumentClosing;

            _textView.AddService(this);
        }

        private void OnDocumentClosing(object sender, EventArgs e) {
            Dispose();
        }

        public static ISuggestedActionsSource FromViewAndBuffer(ITextView textView, ITextBuffer textBuffer, ICoreShell shell) {
            var suggestedActionsSource = textView.GetService<RSuggestedActionSource>();
            if (suggestedActionsSource == null) {
                // Check for detached documents in the interactive window projected buffers
                var document = textBuffer.GetEditorDocument<IREditorDocument>();
                if(document == null || document.IsClosed) {
                    return null;
                }
                var suggestedActionProviders = 
                    ComponentLocator<IRSuggestedActionProvider>.ImportMany(shell.GetService<ICompositionService>()).Select(p => p.Value);
                suggestedActionsSource = new RSuggestedActionSource(textView, textBuffer, suggestedActionProviders, shell);
            }
            return suggestedActionsSource;
        }

        private void OnCaretPositionChanged(object sender, CaretPositionChangedEventArgs e) {
            if (_document?.EditorTree != null && !_document.IsClosed) {
                var bufferPoint = _textView.GetCaretPosition(_document.TextBuffer());
                if (bufferPoint.HasValue) {
                    var node = _document.EditorTree.AstRoot.GetNodeOfTypeFromPosition<TokenNode>(bufferPoint.Value);
                    if (node != _lastNode) {
                        _lastNode = node;
                        SuggestedActionsChanged?.Invoke(this, new EventArgs());
                    }
                }
            }
        }

        #region ISuggestedActionsSource
        public event EventHandler<EventArgs> SuggestedActionsChanged;

        public IEnumerable<SuggestedActionSet> GetSuggestedActions(ISuggestedActionCategorySet requestedActionCategories, SnapshotSpan range, CancellationToken cancellationToken) {
            if (_textView == null ||
                cancellationToken.IsCancellationRequested ||
                !range.Snapshot.TextBuffer.ContentType.TypeName.EqualsOrdinal(RContentTypeDefinition.ContentType)) {
                return Enumerable.Empty<SuggestedActionSet>();
            }

            var actionSets = new List<SuggestedActionSet>();
            var caretPosition = _textView.Caret.Position.BufferPosition;
            var bufferPoint = _textView.MapDownToR(caretPosition);
            if (bufferPoint.HasValue) {
                var ast = _document?.EditorTree?.AstRoot;
                var bufferPosition = bufferPoint.Value.Position;
                _lastNode = ast?.GetNodeOfTypeFromPosition<TokenNode>(bufferPosition);
                if (_lastNode != null) {
                    foreach (var actionProvider in _suggestedActionProviders) {
                        if (actionProvider.HasSuggestedActions(_textView, _textBuffer, bufferPosition)) {
                            var actions = actionProvider.GetSuggestedActions(_textView, _textBuffer, bufferPosition);
                            var applicableSpan = new Span(_lastNode.Start, _lastNode.Length);
                            var actionSet = new SuggestedActionSet(actions, applicableToSpan: applicableSpan);
                            actionSets.Add(actionSet);
                        }
                    }
                }
            }
            return actionSets;
        }

        public Task<bool> HasSuggestedActionsAsync(ISuggestedActionCategorySet requestedActionCategories, SnapshotSpan range, CancellationToken cancellationToken) {
            if (_textView != null && !_textView.Caret.InVirtualSpace) {
                var rPosition = _textView.MapDownToR(_textView.Caret.Position.BufferPosition);
                if (rPosition.HasValue) {
                    foreach (var actionProvider in _suggestedActionProviders) {
                        if (actionProvider.HasSuggestedActions(_textView, _textBuffer, rPosition.Value.Position)) {
                            return Task.FromResult(true);
                        }
                    }
                }
            }
            return Task.FromResult(false);
        }

        public bool TryGetTelemetryId(out Guid telemetryId) {
            telemetryId = REditorCommands.REditorCmdSetGuid;
            return true;
        }

        public void Dispose() {
            if (_textView != null) {
                _textView.RemoveService(this);
                _textView.Caret.PositionChanged -= OnCaretPositionChanged;
                _textBuffer = null;
                _textView = null;
            }
            if (_document != null) {
                _document.Closing -= OnDocumentClosing;
                _document = null;
            }
        }
        #endregion
    }
}
