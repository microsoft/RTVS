// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Disposables;
using Microsoft.Common.Core.Services;
using Microsoft.Languages.Editor.Completions;
using Microsoft.Languages.Editor.Text;
using Microsoft.R.Core.AST;
using Microsoft.R.Editor.Completions;
using Microsoft.R.Editor.Document;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.R.Editor.Signatures {
    /// <summary>
    /// Implements <see cref="ISignatureHelpSource"/>. Provides 
    /// function signature help to the Visual Studio editor
    /// </summary>
    internal sealed class RSignatureHelpSource : ISignatureHelpSource {
        private readonly DisposeToken _disposeToken;
        private readonly ITextBuffer _textBuffer;
        private readonly IServiceContainer _services;
        private readonly IREditorSettings _settings;
        private readonly ISignatureHelpBroker _broker;
        private readonly IRFunctionSignatureEngine _engine;
        private IEnumerable<IRFunctionSignatureHelp> _signatures;

        public RSignatureHelpSource(ITextBuffer textBuffer, IServiceContainer services) {
            _disposeToken = DisposeToken.Create<RSignatureHelpSource>();
            _textBuffer = textBuffer;
            _services = services;
            _settings = _services.GetService<IREditorSettings>();
            _broker = _services.GetService<ISignatureHelpBroker>();
            _engine = new RFunctionSignatureEngine(services);
            textBuffer.AddService(this);
        }

        #region ISignatureHelpSource
        public void AugmentSignatureHelpSession(ISignatureHelpSession session, IList<ISignature> signatures) {
            if (!_settings.SignatureHelpEnabled || session.IsDismissed) {
                return;
            }

            var document = _textBuffer.GetEditorDocument<IREditorDocument>();
            if (document == null) {
                return;
            }
            // If document is not ready, let it parse and call us back when ready.
            // The parsing is asyncronous so we'll need to re-trigger the session.
            if (!document.EditorTree.IsReady) {
                document.EditorTree.InvokeWhenReady(p => RetriggerSignatureHelp((ITextView)p), session.TextView, GetType(), processNow: true);
            } else {
                // Try get signatures. If there is no cached data, there will be async call to R
                // and when it is done, we will re-trigger the session.
                AugmentSignatureHelpSession(session, signatures, document.EditorTree.AstRoot, (textView, sigs) => {
                    _signatures = sigs;
                    RetriggerSignatureHelp(textView);
                });
            }
        }

        public bool AugmentSignatureHelpSession(ISignatureHelpSession session, IList<ISignature> signatures, AstRoot ast, 
                                                Action<ITextView, IEnumerable<IRFunctionSignatureHelp>> callback) {
            // Try cached if this was a re-trigger on async information retrieval
            var eis = new EditorIntellisenseSession(session, _services);
            if (GetCachedSignatures(signatures, eis)) {
                return true;
            }

            var editorBuffer = _textBuffer.ToEditorBuffer();
            var position = session.GetTriggerPoint(_textBuffer).GetCurrentPosition();
            var context = new RIntellisenseContext(eis, editorBuffer, ast, position);

            // See if information is immediately available
            var sigs = _engine.GetSignaturesAsync(context, null);
            if (sigs != null) {
                AddSignatures(signatures, ToVsEditorSignatures(sigs));
                return true;
            }

            // If not available, start async retrieval. Session wil be re-triggered 
            // when information becomes available.
            _engine.GetSignaturesAsync(context, s => callback(session.TextView, s));
            return false;
        }

        private bool GetCachedSignatures(IList<ISignature> signatures, IEditorIntellisenseSession session) {
            if (_signatures != null) {
                foreach(var s in _signatures) {
                    s.Session = session;
                }
                AddSignatures(signatures, ToVsEditorSignatures(_signatures));
                _signatures = null;
                return true;
            }
            return false;
        }

        private static void AddSignatures(IList<ISignature> signatures, IEnumerable<ISignature> signaturesToAdd) {
            foreach (var s in signaturesToAdd) {
                signatures.Add(s);
            }
        }

        public static IList<ISignature> ToVsEditorSignatures(IEnumerable<IRFunctionSignatureHelp> signatures)
            => signatures.Select(x => new RSignatureHelp(x)).Cast<ISignature>().ToList();

        public ISignature GetBestMatch(ISignatureHelpSession session) {
            if (session.Signatures.Count > 0) {
                var applicableToSpan = session.Signatures[0].ApplicableToSpan;
                var text = applicableToSpan.GetText(applicableToSpan.TextBuffer.CurrentSnapshot);
                foreach (var sig in session.Signatures) {
                    var sh = sig as RSignatureHelp;
                    if (sh != null && sh.FunctionName.StartsWithOrdinal(text)) {
                        return sig;
                    }
                }
            }
            return null;
        }
        #endregion

        #region IDisposable
        public void Dispose() {
            if (_disposeToken.TryMarkDisposed()) {
                _textBuffer?.RemoveService(this);
            }
        }
        #endregion

        private void RetriggerSignatureHelp(ITextView textView) {
            var broker = _services.GetService<ISignatureHelpBroker>();
            broker.DismissAllSessions(textView);
            broker.TriggerSignatureHelp(textView);
        }
    }
}
