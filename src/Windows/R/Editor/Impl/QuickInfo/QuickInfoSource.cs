// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Services;
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
        private readonly IRFunctionSignatureEngine _engine;
        private readonly ITextBuffer _subjectBuffer;
        private int _lastPosition = -1;
        private IEnumerable<IRFunctionQuickInfo> _infos;

        public QuickInfoSource(ITextBuffer subjectBuffer, IServiceContainer services) {
            _services = services;
            _engine = new RFunctionSignatureEngine(services);

            _subjectBuffer = subjectBuffer;
            _subjectBuffer.Changed += OnTextBufferChanged;
        }

        void OnTextBufferChanged(object sender, TextContentChangedEventArgs e) => _lastPosition = -1;

        #region IQuickInfoSource
        public void AugmentQuickInfoSession(IQuickInfoSession session, IList<object> quickInfoContent, out ITrackingSpan applicableToSpan) {
            applicableToSpan = null;
            var document = _subjectBuffer.GetEditorDocument<IREditorDocument>();
            if (document == null) {
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
                                        RetriggerQuickInfoSession);
            }
        }

        internal bool AugmentQuickInfoSession(AstRoot ast, ITextBuffer textBuffer, int position, IQuickInfoSession session,
                                              IList<object> quickInfoContent, out ITrackingSpan applicableToSpan,
                                              Action<IEnumerable<IRFunctionQuickInfo>, IQuickInfoSession> callback) {
            // Try cached if this was a re-trigger on async information retrieval
            var eis = new EditorIntellisenseSession(session, _services);
            if (GetCachedSignatures(quickInfoContent, textBuffer, position, _infos, out applicableToSpan)) {
                return true;
            }

            var context = new RIntellisenseContext(eis, textBuffer.ToEditorBuffer(), ast, position);
            // See if information is immediately available
            var infos = _engine.GetQuickInfosAsync(context, null);
            if (infos != null) {
                AddQuickInfos(quickInfoContent, MakeQuickInfos(infos));
                return true;
            }

            // If not available, start async retrieval. Session wil be re-triggered 
            // when information becomes available.
            _engine.GetQuickInfosAsync(context, x => callback(x, session));
            return false;
        }
        #endregion

        internal static bool GetCachedSignatures(IList<object> quickInfos, ITextBuffer textBuffer, int position, IEnumerable<IRFunctionQuickInfo> infos, out ITrackingSpan applicableSpan) {
            applicableSpan = null;
            if (infos == null || !infos.Any()) {
                return false;
            }

            applicableSpan = infos.First().ApplicableToRange.As<ITrackingSpan>();
            var content = MakeQuickInfos(infos);
            foreach (var s in content) {
                quickInfos.Add(s);
            }
            return true;
        }

        private static void AddQuickInfos(IList<object> quickInfos, IEnumerable<string> infosToAdd) {
            foreach (var x in infosToAdd) {
                quickInfos.Add(x);
            }
        }

        private static IEnumerable<string> MakeQuickInfos(IEnumerable<IRFunctionQuickInfo> infos)
            => infos.Select(x => x.Content.FirstOrDefault()).ExcludeDefault();

        private void RetriggerQuickInfoSession(IEnumerable<IRFunctionQuickInfo> infos, IQuickInfoSession session) {
            if (session == null) {
                return;
            }

            var broker = _services.GetService<IQuickInfoBroker>();
            if (!session.IsDismissed) {
                session.Dismiss();
            }

            _infos = infos;
            _lastPosition = -1;

            _services.MainThread().Post(() => broker.TriggerQuickInfo(session.TextView));
        }

        #region IDisposable
        public void Dispose() {
        }
        #endregion
    }
}
