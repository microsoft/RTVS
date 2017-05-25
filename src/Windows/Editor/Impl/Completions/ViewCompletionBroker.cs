// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.Common.Core;
using Microsoft.Languages.Editor.Text;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.Languages.Editor.Completions {
    [Export(typeof(IViewCompletionBroker))]
    public sealed class ViewCompletionBroker : IViewCompletionBroker {
        private readonly ICompletionBroker _completionBroker;

        [ImportingConstructor]
        public ViewCompletionBroker(ICompletionBroker completionBroker) {
            _completionBroker = completionBroker;
        }

        public IReadOnlyList<IEditorIntellisenseSession> GetSessions(IEditorView view)
            => _completionBroker.GetSessions(view.As<ITextView>())
            .Select(s => s.ToEditorIntellisenseSession())
            .ExcludeDefault()
            .ToList();

        public void TriggerCompletionSession(IEditorView view) 
            => _completionBroker.TriggerCompletion(view.As<ITextView>());
        public void DismissCompletionSession(IEditorView view) 
            => _completionBroker.DismissAllSessions(view.As<ITextView>());
    }
}
