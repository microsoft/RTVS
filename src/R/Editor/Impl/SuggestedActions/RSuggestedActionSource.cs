// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Shell;
using Microsoft.Languages.Editor.Composition;
using Microsoft.Languages.Editor.Services;
using Microsoft.R.Components.ContentTypes;
using Microsoft.R.Core.AST;
using Microsoft.R.Editor.Document;
using Microsoft.R.Editor.SuggestedActions.Definitions;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.R.Editor.SuggestedActions {
    internal sealed class RSuggestedActionSource : ISuggestedActionsSource {
        private IEnumerable<IRSuggestedActionProvider> _suggestedActionProviders;
        private ITextBuffer _textBuffer;
        private ITextView _textView;
        IREditorDocument _document;
        IAstNode _lastNode;

        public RSuggestedActionSource(ITextView textView, ITextBuffer textBuffer, IEnumerable<IRSuggestedActionProvider> suggestedActionProviders, ICoreShell shell) {
            _textBuffer = textBuffer;
            _textView = textView;
            _textView.Caret.PositionChanged += OnCaretPositionChanged;
            _suggestedActionProviders = suggestedActionProviders;

            _document = REditorDocument.TryFromTextBuffer(_textBuffer);
            _document.DocumentClosing += OnDocumentClosing;

            ServiceManager.AddService(this, _textView, shell);
        }

        private void OnDocumentClosing(object sender, EventArgs e) {
            Dispose();
        }

        public static ISuggestedActionsSource FromViewAndBuffer(ITextView textView, ITextBuffer textBuffer, ICoreShell shell) {
            var suggestedActionsSource = ServiceManager.GetService<RSuggestedActionSource>(textView);
            if (suggestedActionsSource == null) {
                // Check for detached documents in the interactive window projected buffers
                var document = REditorDocument.TryFromTextBuffer(textBuffer);
                if(document == null || document.IsClosed) {
                    return null;
                }
                IEnumerable<IRSuggestedActionProvider> suggestedActionProviders = ComponentLocator<IRSuggestedActionProvider>.ImportMany(shell.CompositionService).Select(p => p.Value);
                suggestedActionsSource = new RSuggestedActionSource(textView, textBuffer, suggestedActionProviders, shell);
            }
            return suggestedActionsSource;
        }

        private void OnCaretPositionChanged(object sender, CaretPositionChangedEventArgs e) {
            if (_document != null && !_document.IsClosed && _document.EditorTree != null) {
                SnapshotPoint? bufferPoint = REditorDocument.MapCaretPositionFromView(_textView);
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
            if (cancellationToken.IsCancellationRequested ||
                !range.Snapshot.TextBuffer.ContentType.TypeName.EqualsOrdinal(RContentTypeDefinition.ContentType)) {
                return Enumerable.Empty<SuggestedActionSet>();
            }

            List<SuggestedActionSet> actionSets = new List<SuggestedActionSet>();
            var caretPosition = _textView.Caret.Position.BufferPosition;
            SnapshotPoint? bufferPoint = _textView.MapDownToR(caretPosition);
            if (bufferPoint.HasValue) {
                AstRoot ast = _document?.EditorTree?.AstRoot;
                int bufferPosition = bufferPoint.Value.Position;
                _lastNode = ast?.GetNodeOfTypeFromPosition<TokenNode>(bufferPosition);
                if (_lastNode != null) {
                    foreach (IRSuggestedActionProvider actionProvider in _suggestedActionProviders) {
                        if (actionProvider.HasSuggestedActions(_textView, _textBuffer, bufferPosition)) {
                            IEnumerable<ISuggestedAction> actions = actionProvider.GetSuggestedActions(_textView, _textBuffer, bufferPosition);
                            Span applicableSpan = new Span(_lastNode.Start, _lastNode.Length);
                            SuggestedActionSet actionSet = new SuggestedActionSet(actions, applicableToSpan: applicableSpan);
                            actionSets.Add(actionSet);
                        }
                    }
                }
            }
            return actionSets;
        }

        public Task<bool> HasSuggestedActionsAsync(ISuggestedActionCategorySet requestedActionCategories, SnapshotSpan range, CancellationToken cancellationToken) {
            if (!_textView.Caret.InVirtualSpace) {
                var rPosition = _textView.MapDownToR(_textView.Caret.Position.BufferPosition);
                if (rPosition.HasValue) {
                    foreach (IRSuggestedActionProvider actionProvider in _suggestedActionProviders) {
                        if (actionProvider.HasSuggestedActions(_textView, _textBuffer, rPosition.Value.Position)) {
                            return Task.FromResult(true);
                        }
                    }
                }
            }
            return Task.FromResult(false);
        }

        public bool TryGetTelemetryId(out Guid telemetryId) {
            telemetryId = Guid.Empty;
            return false;
        }

        public void Dispose() {
            if (_textView != null) {
                ServiceManager.RemoveService<RSuggestedActionSource>(_textView);
                _textView.Caret.PositionChanged -= OnCaretPositionChanged;
                _textBuffer = null;
                _textView = null;
            }
            if (_document != null) {
                _document.DocumentClosing -= OnDocumentClosing;
                _document = null;
            }
        }
        #endregion
    }
}
