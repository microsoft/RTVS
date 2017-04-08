// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.Common.Core.Shell;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.Utility;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.AST.Operators;
using Microsoft.R.Core.Tokens;
using Microsoft.R.Editor.Document;
using Microsoft.R.Support.Help;
using Microsoft.R.Support.Help.Functions;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Operations;

namespace Microsoft.R.Editor.QuickInfo {
    internal sealed class QuickInfoSource : IQuickInfoSource {
        [Import]
        internal ITextStructureNavigatorSelectorService NavigatorService { get; set; }

        [Import]
        internal ITextBufferFactoryService TextBufferFactoryService { get; set; }

        [Import]
        internal IFunctionIndex FunctionIndex { get; set; }

        private readonly ITextBuffer _subjectBuffer;
        private readonly ICompositionCatalog _catalog;
        private int _lastPosition = -1;
        private string _packageName;

        public QuickInfoSource(ITextBuffer subjectBuffer, ICompositionCatalog catalog) {
            _catalog = catalog;
            _catalog.CompositionService.SatisfyImportsOnce(this);

            _subjectBuffer = subjectBuffer;
            _subjectBuffer.Changed += OnTextBufferChanged;
        }

        void OnTextBufferChanged(object sender, TextContentChangedEventArgs e) {
            _lastPosition = -1;
        }

        #region IQuickInfoSource
        public void AugmentQuickInfoSession(IQuickInfoSession session, IList<object> quickInfoContent, out ITrackingSpan applicableToSpan) {
            applicableToSpan = null;
            SnapshotPoint? triggerPoint = session.GetTriggerPoint(_subjectBuffer.CurrentSnapshot);
            if (triggerPoint.HasValue) {
                int position = triggerPoint.Value;
                if (_lastPosition != position) {
                    _lastPosition = position;
                    ITextSnapshot snapshot = triggerPoint.Value.Snapshot;

                    IREditorDocument document = REditorDocument.TryFromTextBuffer(_subjectBuffer);
                    if (document != null) {
                        // Document may be null in REPL window as projections are not
                        // getting set immediately or may change as user moves mouse over.
                        AugmentQuickInfoSession(document.EditorTree.AstRoot, document.TextBuffer, position,
                                                session, quickInfoContent, out applicableToSpan,
                                                (object o, string p) => RetriggerQuickInfoSession(o as IQuickInfoSession, p), null);
                    }
                }
            }
        }

        internal bool AugmentQuickInfoSession(AstRoot ast, ITextBuffer textBuffer, int position, IQuickInfoSession session,
                                              IList<object> quickInfoContent, out ITrackingSpan applicableToSpan,
                                              Action<object, string> retriggerAction, string packageName) {
            int signatureEnd = position;
            var snapshot = textBuffer.CurrentSnapshot;
            position = Math.Min(signatureEnd, position);
            int start = Math.Min(position, snapshot.Length);
            int end = Math.Min(signatureEnd, snapshot.Length);
            IFunctionInfo functionInfo = null;

            applicableToSpan = snapshot.CreateTrackingSpan(Span.FromBounds(start, end), SpanTrackingMode.EdgeInclusive);
            packageName = packageName ?? _packageName;
            _packageName = null;

            // First try name under mouse or caret
            Span span;
            var line = snapshot.GetLineFromPosition(position);
            var functionName = TextViewExtensions.GetItemAtPosition(line, position, x => x == RTokenType.Identifier, out span);
            // Verify this is a function

            IAstNode node;
            ast.GetPositionNode(position, out node);
            if (node == null) {
                return false;
            }

            // In abc(de|f(x)) first find inner function, then outer.
            ITextRange range;
            if (node is TokenNode && node.Parent is FunctionCall) {
                range = node;
            } else {
                var fc = ast.GetNodeOfTypeFromPosition<FunctionCall>(position);
                range = fc?.RightOperand as TokenNode;
            }

            if (range != null) {
                functionName = ast.TextProvider.GetText(range);
                if (!string.IsNullOrEmpty(functionName)) {
                    functionInfo = FunctionIndex.GetFunctionInfo(functionName, packageName, retriggerAction, session);
                }
            }

            if (functionInfo == null || functionInfo.Signatures == null) {
                return false;
            }

            foreach (var sig in functionInfo.Signatures) {
                string signatureString = sig.GetSignatureString(functionName);
                int wrapLength = Math.Min(SignatureInfo.MaxSignatureLength, signatureString.Length);
                string text;

                if (string.IsNullOrWhiteSpace(functionInfo.Description)) {
                    text = string.Empty;
                } else {
                    // VS may end showing very long tooltip so we need to keep 
                    // description reasonably short: typically about
                    // same length as the function signature.
                    text = signatureString + "\r\n" + functionInfo.Description.Wrap(wrapLength);
                }

                if (text.Length > 0) {
                    quickInfoContent.Add(text);
                    return true;
                }
            }
            return false;
        }
        #endregion

        private void RetriggerQuickInfoSession(IQuickInfoSession session, string packageName) {
            if (session != null && !session.IsDismissed) {
                session.Dismiss();
            }

            _lastPosition = -1;
            _packageName = packageName;
            if (packageName != null) {
                IQuickInfoBroker broker = _catalog.ExportProvider.GetExport<IQuickInfoBroker>().Value;
                broker.TriggerQuickInfo(session.TextView, session.GetTriggerPoint(session.TextView.TextBuffer), session.TrackMouse);
            }
        }

        #region IDisposable
        public void Dispose() {
        }
        #endregion
    }
}
