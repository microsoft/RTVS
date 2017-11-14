// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using Microsoft.Languages.Editor.Completions;
using Microsoft.Languages.Editor.Signatures;
using Microsoft.Languages.Editor.Text;

namespace Microsoft.R.LanguageServer.Services.Editor {
    internal sealed class ViewSignatureBroker : IViewSignatureBroker {
        public IReadOnlyList<IEditorIntellisenseSession> GetSessions(IEditorView view) => new List<IEditorIntellisenseSession>();
        public void TriggerSignatureSession(IEditorView view) { }
        public void DismissSignatureSession(IEditorView view) { }
    }
}
