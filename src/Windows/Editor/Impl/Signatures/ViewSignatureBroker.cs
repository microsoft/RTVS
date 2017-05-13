// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Shell;
using Microsoft.Languages.Editor.Completions;
using Microsoft.Languages.Editor.Text;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.Languages.Editor.Signatures {
    [Export(typeof(IViewSignatureBroker))]
    public sealed class ViewsignatureBroker : IViewSignatureBroker {
        private readonly ISignatureHelpBroker _signatureBroker;

        [ImportingConstructor]
        public ViewsignatureBroker(ISignatureHelpBroker signatureBroker) {
            _signatureBroker = signatureBroker;
        }

        public IReadOnlyList<IEditorIntellisenseSession> GetSessions(IEditorView view)
            => _signatureBroker.GetSessions(view.As<ITextView>())
            .Select(s => s.ToEditorIntellisenseSession())
            .ExcludeDefault()
            .ToList();

        public void TriggerSignatureSession(IEditorView view) 
            => _signatureBroker.TriggerSignatureHelp(view.As<ITextView>());
        public void DismissSignatureSession(IEditorView view) 
            => _signatureBroker.DismissAllSessions(view.As<ITextView>());
    }
}
