// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.Common.Core.Diagnostics;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Shell;
using Microsoft.Languages.Editor.Text;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.Languages.Editor.Completions {
    [Export(typeof(IViewCompletionBroker))]
    public sealed class ViewCompletionBroker : IViewCompletionBroker {
        private readonly IServiceContainer _services;
        private readonly ICompletionBroker _completionBroker;
        private readonly ISignatureHelpBroker _signatureBroker;
        private readonly IQuickInfoBroker _quickInfoBroker;

        [ImportingConstructor]
        public ViewCompletionBroker(ICoreShell coreShell) {
            _services = coreShell.Services;

            _completionBroker = _services.GetService<ICompletionBroker>();
            _signatureBroker = _services.GetService<ISignatureHelpBroker>();
            _quickInfoBroker = _services.GetService<IQuickInfoBroker>();
        }

        public IReadOnlyList<IEditorIntellisenseSession> GetSessions(IEditorView view)
            => _completionBroker.GetSessions(view.As<ITextView>())
            .Select(s => new EditorIntellisenseSession(s, _services))
            .ToList();

        public void TriggerCompletionSession(IEditorView view) 
            => _completionBroker.TriggerCompletion(view.As<ITextView>());
        public void DismissCompletionSession(IEditorView view) 
            => _completionBroker.DismissAllSessions(view.As<ITextView>());

        public void TriggerSignatureSession(IEditorView view) 
            => _signatureBroker.TriggerSignatureHelp(view.As<ITextView>());
        public void DismissSignatureSession(IEditorView view) 
            => _signatureBroker.DismissAllSessions(view.As<ITextView>());

        public void TriggerQuickInfoSession(IEditorView view) 
            => _quickInfoBroker.TriggerQuickInfo(view.As<ITextView>());

        public void DismissQuickInfoSession(IEditorView view) {
            foreach (var s in _quickInfoBroker.GetSessions(view.As<ITextView>())) {
                s.Dismiss();
            }
        }
    }
}
