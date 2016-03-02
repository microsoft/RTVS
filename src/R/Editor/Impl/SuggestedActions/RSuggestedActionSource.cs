// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Languages.Editor.Composition;
using Microsoft.Languages.Editor.Services;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.AST.Definitions;
using Microsoft.R.Editor.ContentType;
using Microsoft.R.Editor.Document;
using Microsoft.R.Editor.Document.Definitions;
using Microsoft.R.Editor.SuggestedActions.Definitions;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.R.Editor.SuggestedActions {
    internal sealed class RSuggestedActionSource : ISuggestedActionsSource {
        private static readonly Guid _treeUser = new Guid("{62D552E2-5895-4388-ACD7-E76764E0A27A}");
        private IEnumerable<IRSuggestedActionProvider> _suggestedActionProviders;
        private ITextBuffer _textBuffer;
        private ITextView _textView;
        IREditorDocument _document;
        IAstNode _lastNode;

        public RSuggestedActionSource(ITextView textView, ITextBuffer textBuffer, IEnumerable<IRSuggestedActionProvider> suggestedActionProviders) {
            _textBuffer = textBuffer;
            _textView = textView;
            _textView.Caret.PositionChanged += OnCaretPositionChanged;
            _suggestedActionProviders = suggestedActionProviders;
            _document = REditorDocument.FromTextBuffer(_textBuffer);
            ServiceManager.AddService(this, _textView);
        }

        public static ISuggestedActionsSource FromViewAndBuffer(ITextView textView, ITextBuffer textBuffer) {
            var suggestedActionsSource = ServiceManager.GetService<RSuggestedActionSource>(textView);
            if (suggestedActionsSource == null) {
                IEnumerable<IRSuggestedActionProvider> suggestedActionProviders = ComponentLocator<IRSuggestedActionProvider>.ImportMany().Select(p => p.Value);
                suggestedActionsSource = new RSuggestedActionSource(textView, textBuffer, suggestedActionProviders);
            }
            return suggestedActionsSource;
        }

        private void OnCaretPositionChanged(object sender, CaretPositionChangedEventArgs e) {
            int caretPosition = e.NewPosition.BufferPosition;
            SnapshotPoint? bufferPoint = _textView.MapDownToR(caretPosition);
            if (bufferPoint.HasValue && _document != null && _document.EditorTree != null) {
                var node = _document.EditorTree.AstRoot.GetNodeOfTypeFromPosition<TokenNode>(e.NewPosition.BufferPosition);
                if (node != _lastNode) {
                    _lastNode = node;
                    SuggestedActionsChanged?.Invoke(this, new EventArgs());
                }
            }
        }

        #region ISuggestedActionsSource
        public event EventHandler<EventArgs> SuggestedActionsChanged;

        public IEnumerable<SuggestedActionSet> GetSuggestedActions(ISuggestedActionCategorySet requestedActionCategories, SnapshotSpan range, CancellationToken cancellationToken) {
            if (cancellationToken.IsCancellationRequested) {
                return Enumerable.Empty<SuggestedActionSet>();
            }

            List<SuggestedActionSet> actionSets = new List<SuggestedActionSet>();
            int caretPosition = _textView.Caret.Position.BufferPosition;
            SnapshotPoint? bufferPoint = _textView.MapDownToR(caretPosition);
            if (bufferPoint.HasValue) {
                AstRoot ast = _document.EditorTree.AstRoot;
                int bufferPosition = bufferPoint.Value.Position;
                _lastNode = ast.GetNodeOfTypeFromPosition<TokenNode>(bufferPosition);
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
            int caretPosition = _textView.Caret.Position.BufferPosition;
            foreach (IRSuggestedActionProvider actionProvider in _suggestedActionProviders) {
                if (actionProvider.HasSuggestedActions(_textView, _textBuffer, caretPosition)) {
                    return Task.FromResult(true);
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
                _textView.Caret.PositionChanged -= OnCaretPositionChanged;
                _document = null;
                _textBuffer = null;
                _textView = null;
            }
        }
        #endregion
    }
}
