// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Common.Core.Diagnostics;
using Microsoft.Common.Core.Shell;
using Microsoft.Languages.Editor.Text;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.Languages.Editor.Completions {
    internal sealed class ViewCompletionBroker : IViewCompletionBroker {
        private readonly ICoreShell _coreShell;
        private readonly ICompletionBroker _completionBroker;
        private readonly ISignatureHelpBroker _signatureBroker;
        private readonly IQuickInfoBroker _quickInfoBroker;
        private readonly ITextView _view;

        public ViewCompletionBroker(ICoreShell coreShell, IEditorView view) {
            _view = view.As<ITextView>();
            Check.ArgumentNull(nameof(view), _view);

            _coreShell = coreShell;
            _completionBroker = _coreShell.GetService<ICompletionBroker>();
            _signatureBroker = _coreShell.GetService<ISignatureHelpBroker>();
            _quickInfoBroker = _coreShell.GetService<IQuickInfoBroker>();

            view.AddService(this);
        }

        public void TriggerCompletionSession() => _completionBroker.TriggerCompletion(_view);
        public void DismissCompletionSession() => _completionBroker.DismissAllSessions(_view);

        public void TriggerSignatureSession() => _signatureBroker.TriggerSignatureHelp(_view);
        public void DismissSignatureSession() => _signatureBroker.DismissAllSessions(_view);

        public void TriggerQuickInfoSession() => _quickInfoBroker.TriggerQuickInfo(_view);
        public void DismissQuickInfoSession() {
            foreach (var s in _quickInfoBroker.GetSessions(_view)) {
                s.Dismiss();
            }
        }
    }
}
