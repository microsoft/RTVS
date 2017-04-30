
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Common.Core.Services;
using Microsoft.Languages.Editor.Text;
using Microsoft.Languages.Editor.Utility;
using Microsoft.R.Core.AST;
using Microsoft.R.Editor.Document;
using Microsoft.R.Editor.Functions;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;

namespace Microsoft.R.Editor.QuickInfo {
    internal sealed class QuickInfoSource : IQuickInfoSource {
        private readonly IServiceContainer _services;
        private readonly IFunctionIndex _functionIndex;
        private readonly ITextBuffer _subjectBuffer;
        private int _lastPosition = -1;
        private string _packageName;

        public QuickInfoSource(ITextBuffer subjectBuffer, IServiceContainer services) {
            _services = services;
            _functionIndex = services.GetService<IFunctionIndex>();

            _subjectBuffer = subjectBuffer;
            _subjectBuffer.Changed += OnTextBufferChanged;
        }

        void OnTextBufferChanged(object sender, TextContentChangedEventArgs e) => _lastPosition = -1;

        #region IQuickInfoSource
        public void AugmentQuickInfoSession(IQuickInfoSession session, IList<object> quickInfoContent, out ITrackingSpan applicableToSpan) {
            applicableToSpan = null;
            var triggerPoint = session.GetTriggerPoint(_subjectBuffer.CurrentSnapshot);
            if (triggerPoint.HasValue) {
                int position = triggerPoint.Value;
                if (_lastPosition != position) {
                    _lastPosition = position;
                    var document = _subjectBuffer.GetEditorDocument<IREditorDocument>();
                    if (document != null) {
                        // Document may be null in REPL window as projections are not
                        // getting set immediately or may change as user moves mouse over.
                        AugmentQuickInfoSession(document.EditorTree.AstRoot, position,
                                                session, quickInfoContent, out applicableToSpan,
                                                (o, p) => RetriggerQuickInfoSession(o as IQuickInfoSession, p), null);
                    }
                }
            }
        }

        internal bool AugmentQuickInfoSession(AstRoot ast, int position, IQuickInfoSession session,
                                              IList<object> quickInfoContent, out ITrackingSpan applicableToSpan,
                                              Action<object, string> retriggerAction, string packageName) {
            int signatureEnd = position;
            var snapshot = session.TextView.TextBuffer.CurrentSnapshot;

            position = Math.Min(signatureEnd, position);
            var start = Math.Min(position, snapshot.Length);
            var end = Math.Min(signatureEnd, snapshot.Length);
            IFunctionInfo functionInfo = null;

            applicableToSpan = snapshot.CreateTrackingSpan(Span.FromBounds(start, end), SpanTrackingMode.EdgeInclusive);
            packageName = packageName ?? _packageName;
            _packageName = null;

            // Get function name from the AST. We don't use signature support here since
            // when caret or mouse is inside function arguments such as in abc(de|f(x)) 
            // it gives information of the outer function since signature is about help
            // on the function arguments.
            var functionName = ast.GetFunctionName(position);
            if (!string.IsNullOrEmpty(functionName)) {
                functionInfo = _functionIndex.GetFunctionInfo(functionName, packageName, retriggerAction, session);
            }
            if (functionInfo?.Signatures == null) {
                return false;
            }

            foreach (var sig in functionInfo.Signatures) {
                var signatureString = sig.GetSignatureString(functionName);
                var wrapLength = Math.Min(SignatureInfo.MaxSignatureLength, signatureString.Length);
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
                var broker = _services.GetService<IQuickInfoBroker>();
                broker.TriggerQuickInfo(session.TextView, session.GetTriggerPoint(session.TextView.TextBuffer), session.TrackMouse);
            }
        }

        #region IDisposable
        public void Dispose() {
        }
        #endregion
    }
}
