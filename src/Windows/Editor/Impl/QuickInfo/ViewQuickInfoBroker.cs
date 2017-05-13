// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.Common.Core;
using Microsoft.Languages.Editor.Completions;
using Microsoft.Languages.Editor.Text;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.Languages.Editor.QuickInfo {
    [Export(typeof(IViewQuickInfoBroker))]
    public sealed class ViewQuickInfoBroker : IViewQuickInfoBroker {
        private readonly IQuickInfoBroker _quickInfoBroker;

        [ImportingConstructor]
        public ViewQuickInfoBroker(IQuickInfoBroker quickInfoBroker) {
            _quickInfoBroker = quickInfoBroker;
        }

        public IReadOnlyList<IEditorIntellisenseSession> GetSessions(IEditorView view)
            => _quickInfoBroker.GetSessions(view.As<ITextView>())
            .Select(s => s.ToEditorIntellisenseSession())
            .ExcludeDefault()
            .ToList();

        public void TriggerQuickInfoSession(IEditorView view)
            => _quickInfoBroker.TriggerQuickInfo(view.As<ITextView>());

        public void DismissQuickInfoSession(IEditorView view) {
            foreach (var s in _quickInfoBroker.GetSessions(view.As<ITextView>())) {
                s.Dismiss();
            }
        }
    }
}
