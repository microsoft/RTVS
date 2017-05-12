// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Common.Core.Idle;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Shell;
using Microsoft.Languages.Editor.Completions;
using Microsoft.Languages.Editor.Text;
using Microsoft.R.Core.AST;
using Microsoft.R.Editor.Completions;
using Microsoft.R.Editor.Document;
using Microsoft.R.Editor.Signatures;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;

namespace Microsoft.R.Editor.QuickInfo {
    internal sealed class QuickInfoSource : IQuickInfoSource {
        private readonly IServiceContainer _services;
        private readonly ISignatureHelpBroker _signatureHelpBroker;
        private readonly IRFunctionSignatureEngine _engine;
        private readonly ITextBuffer _subjectBuffer;
        private int _lastPosition = -1;
        private IEnumerable<IRFunctionQuickInfo> _infos;

        public QuickInfoSource(ITextBuffer subjectBuffer, IServiceContainer services) {
            _services = services;
            _engine = new RFunctionSignatureEngine(services);
            _signatureHelpBroker = services.GetService<ISignatureHelpBroker>();

            _subjectBuffer = subjectBuffer;
            _subjectBuffer.Changed += OnTextBufferChanged;
        }

        void OnTextBufferChanged(object sender, TextContentChangedEventArgs e) => _lastPosition = -1;

        #region IQuickInfoSource
        public void AugmentQuickInfoSession(IQuickInfoSession session, IList<object> quickInfoContent, out ITrackingSpan applicableToSpan) {
            applicableToSpan = null;
            var document = _subjectBuffer.GetEditorDocument<IREditorDocument>();
            if (_signatureHelpBroker.IsSignatureHelpActive(session.TextView) || document == null) {
                return;
            }

            var textBuffer = document.EditorBuffer.As<ITextBuffer>();
            var triggerPoint = session.GetTriggerPoint(textBuffer.CurrentSnapshot);
            if (!triggerPoint.HasValue) {
                return;
            }

            int position = triggerPoint.Value;
            if (_lastPosition != position) {
                _lastPosition = position;
                // Document may be null in REPL window as projections are not
                // getting set immediately or may change as user moves mouse over.
                AugmentQuickInfoSession(document.EditorTree.AstRoot, textBuffer, position,
                                        session, quickInfoContent, out applicableToSpan,
                                        (infos, o) => RetriggerQuickInfoSession(infos, o));
            }
        }

        internal bool AugmentQuickInfoSession(AstRoot ast, ITextBuffer textBuffer, int position, IQuickInfoSession session,
                                              IList<object> quickInfoContent, out ITrackingSpan applicableToSpan,
                                              Action<IEnumerable<IRFunctionQuickInfo>, IQuickInfoSession> callback) {
            if (GetCachedSignatures(quickInfoContent, textBuffer, position, out applicableToSpan)) {
                return true;
            }

            var eis = new EditorIntellisenseSession(session, _services);
            var context = new RIntellisenseContext(eis, textBuffer.ToEditorBuffer(), ast, position);
            var idle = _services.GetService<IIdleTimeService>();

            IdleTimeAction.Create(() => _engine.GetQuickInfosAsync(context, infos => callback(infos, session)), 0, GetType(), idle);
            return false;
        }
        #endregion

        private bool GetCachedSignatures(IList<object> quickInfos, ITextBuffer textBuffer, int position, out ITrackingSpan applicableSpan) {
            applicableSpan = null;
            if (_infos == null || !_infos.Any()) {
                return false;
            }

            applicableSpan = _infos.First().ApplicableToRange.As<ITrackingSpan>();

            var span = applicableSpan.GetSpan(textBuffer.CurrentSnapshot);
            if (span.Contains(position)) {
                foreach (var s in _infos) {
                    quickInfos.Add(s.Content);
                }
            }
            _infos = null;
            return true;
        }

        private void RetriggerQuickInfoSession(IEnumerable<IRFunctionQuickInfo> infos, IQuickInfoSession session) {
            if (session == null) {
                return;
            }

            var broker = _services.GetService<IQuickInfoBroker>();
            if (broker.IsQuickInfoActive(session.TextView)) {
                foreach(var s in broker.GetSessions(session.TextView)) {
                    s.Dismiss();
                }
            }

            _infos = infos;
            _lastPosition = -1;
            broker.TriggerQuickInfo(session.TextView);
        }

        #region IDisposable
        public void Dispose() {
        }
        #endregion
    }
}
