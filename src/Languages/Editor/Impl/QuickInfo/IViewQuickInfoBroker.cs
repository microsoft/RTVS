// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using Microsoft.Languages.Editor.Completions;
using Microsoft.Languages.Editor.Text;

namespace Microsoft.Languages.Editor.QuickInfo {
    public interface IViewQuickInfoBroker {
        IReadOnlyList<IEditorIntellisenseSession> GetSessions(IEditorView view);
        void TriggerQuickInfoSession(IEditorView view);
        void DismissQuickInfoSession(IEditorView view);
    }
}
