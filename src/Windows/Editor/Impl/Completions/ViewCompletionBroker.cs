// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.Common.Core.Diagnostics;
using Microsoft.Common.Core.Services;
using Microsoft.Languages.Editor.Text;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.Languages.Editor.Completions {
    public sealed class ViewCompletionBroker : IViewCompletionBroker {
        private readonly IServiceContainer _services;
        private readonly ICompletionBroker _completionBroker;
        private readonly ISignatureHelpBroker _signatureBroker;
        private readonly IQuickInfoBroker _quickInfoBroker;
        private readonly ITextView _view;

        public ViewCompletionBroker(IServiceContainer services, IEditorView view) {
            _services = services;

            _view = view.As<ITextView>();
            Check.ArgumentNull(nameof(view), _view);

            _completionBroker = services.GetService<ICompletionBroker>();
            _signatureBroker = services.GetService<ISignatureHelpBroker>();
            _quickInfoBroker = services.GetService<IQuickInfoBroker>();

            view.AddService(this);
        }

        public IReadOnlyList<IEditorIntellisenseSession> GetSessions(IEditorView view) 
            => _completionBroker.GetSessions(view.As<ITextView>()).Select(s => new EditorIntellisenseSession(s, _services)).ToList();

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
