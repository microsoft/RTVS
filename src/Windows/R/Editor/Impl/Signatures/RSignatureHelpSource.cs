// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Disposables;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Threading;
using Microsoft.Languages.Editor.Completions;
using Microsoft.Languages.Editor.Signatures;
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
        private readonly RFunctionSignatureEngine _engine;
        private IList<ISignature> _signatures;

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
            if (document != null) {
                if (!document.EditorTree.IsReady) {
                    document.EditorTree.InvokeWhenReady((p) => {
                        RetriggerSignatureHelp((ITextView)p);
                   }, session.TextView, this.GetType(), processNow: true);
                } else {
                    AugmentSignatureHelpSession(session, signatures, document.EditorTree.AstRoot, (textView, sigs) => {
                        _signatures = sigs;
                        RetriggerSignatureHelp(textView);
                    });
                }
            }
        }

        public bool AugmentSignatureHelpSession(ISignatureHelpSession session, IList<ISignature> signatures, AstRoot ast, Action<ITextView, IList<ISignature>> callback) {
            if (_signatures != null) {
                foreach (var s in _signatures) {
                    signatures.Add(s);
                }
                _signatures = null;
                return true;
            }

            if (callback != null) {
                var editorBuffer = _textBuffer.ToEditorBuffer();
                var eis = new EditorIntellisenseSession(session, _services);
                var position = session.GetTriggerPoint(_textBuffer).GetCurrentPosition();
                var context = new RIntellisenseContext(eis, editorBuffer, ast, position);

                _engine.GetSignaturesAsync(context).ContinueWith(async t => {
                    await _services.MainThread().SwitchToAsync();
                    callback(session.TextView, MakeSignatures(t.Result));
                }).DoNotWait();
            }

            return false;
        }

        private List<ISignature> MakeSignatures(IEnumerable<IFunctionSignatureHelp> functionHelp) {
            var list = new List<ISignature>();
            foreach (var f in functionHelp) {
                list.Add(new RSignatureHelp(f));
            }
            return list;
        }

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
