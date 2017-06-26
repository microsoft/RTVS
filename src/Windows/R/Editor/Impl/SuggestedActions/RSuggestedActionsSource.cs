// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Services;
using Microsoft.Languages.Editor.Composition;
using Microsoft.Languages.Editor.Document;
using Microsoft.Languages.Editor.SuggestedActions;
using Microsoft.Languages.Editor.Text;
using Microsoft.R.Components.ContentTypes;
using Microsoft.R.Core.AST;
using Microsoft.R.Editor.Commands;
using Microsoft.R.Editor.Document;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.R.Editor.SuggestedActions {
    internal sealed class RSuggestedActionsSource : SuggestedActionsSourceBase, ISuggestedActionsSource {
        private readonly IREditorDocument _document;
        private IAstNode _lastNode;

        private RSuggestedActionsSource(ITextView textView, ITextBuffer textBuffer, IEnumerable<ISuggestedActionProvider> suggestedActionProviders, IServiceContainer services) :
            base(textView, textBuffer, suggestedActionProviders, services) {
            _document = (IREditorDocument)Document;
        }

        public static ISuggestedActionsSource Create(ITextView textView, ITextBuffer textBuffer, IServiceContainer services) {
            // Check for detached documents in the interactive window projected buffers
            var document = textBuffer.GetEditorDocument<IREditorDocument>();
            if (document == null || document.IsClosed) {
                return null;
            }
            var cs = services.GetService<ICompositionService>();
            var ctrs = services.GetService<IContentTypeRegistryService>();
            var suggestedActionProviders =
                ComponentLocatorForContentType<ISuggestedActionProvider, IComponentContentTypes>
                    .ImportMany(cs, ctrs.GetContentType(RContentTypeDefinition.ContentType)).Select(p => p.Value);
            return new RSuggestedActionsSource(textView, textBuffer, suggestedActionProviders, services);
        }

        protected override void OnCaretPositionChanged(object sender, CaretPositionChangedEventArgs e) {
            if (!IsDisposed && _document?.EditorTree != null && !_document.IsClosed) {
                var bufferPoint = TextView.GetCaretPosition(_document.TextBuffer());
                if (bufferPoint.HasValue) {
                    var node = _document.EditorTree.AstRoot.GetNodeOfTypeFromPosition<TokenNode>(bufferPoint.Value);
                    if (node != _lastNode) {
                        _lastNode = node;
                        SuggestedActionsChanged?.Invoke(this, EventArgs.Empty);
                    }
                }
            }
        }

        #region ISuggestedActionsSource
        public event EventHandler<EventArgs> SuggestedActionsChanged;

        public IEnumerable<SuggestedActionSet> GetSuggestedActions(ISuggestedActionCategorySet requestedActionCategories, SnapshotSpan range, CancellationToken cancellationToken) {
            if (IsDisposed ||
                cancellationToken.IsCancellationRequested ||
                !range.Snapshot.TextBuffer.ContentType.TypeName.EqualsOrdinal(RContentTypeDefinition.ContentType)) {
                return Enumerable.Empty<SuggestedActionSet>();
            }

            var caretPosition = TextView.Caret.Position.BufferPosition;
            var bufferPoint = TextView.MapDownToR(caretPosition);
            if (!bufferPoint.HasValue) {
                return Enumerable.Empty<SuggestedActionSet>();
            }

            var ast = _document?.EditorTree?.AstRoot;
            var bufferPosition = bufferPoint.Value.Position;
            _lastNode = ast?.GetNodeOfTypeFromPosition<TokenNode>(bufferPosition);
            if (_lastNode == null) {
                return Enumerable.Empty<SuggestedActionSet>();
            }

            var applicableSpan = _lastNode.ToSpan();
            return SuggestedActionProviders
                    .Where(ap => ap.HasSuggestedActions(TextView, TextBuffer, bufferPosition))
                    .Select(ap => new SuggestedActionSet(ap.GetSuggestedActions(TextView, TextBuffer, bufferPosition), applicableToSpan: applicableSpan));
        }

        public Task<bool> HasSuggestedActionsAsync(ISuggestedActionCategorySet requestedActionCategories, SnapshotSpan range, CancellationToken cancellationToken) {
            if (TextView != null && !TextView.Caret.InVirtualSpace) {
                var rPosition = TextView.MapDownToR(TextView.Caret.Position.BufferPosition);
                if (rPosition.HasValue && SuggestedActionProviders.Any(actionProvider => actionProvider.HasSuggestedActions(TextView, TextBuffer, rPosition.Value.Position))) {
                    return Task.FromResult(true);
                }
            }
            return Task.FromResult(false);
        }

        public bool TryGetTelemetryId(out Guid telemetryId) {
            telemetryId = REditorCommands.REditorCmdSetGuid;
            return true;
        }
        #endregion
    }
}
