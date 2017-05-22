// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using Microsoft.Languages.Editor.Text;

namespace Microsoft.Languages.Editor.Completions {
    public interface IViewCompletionBroker {
        IReadOnlyList<IEditorIntellisenseSession> GetSessions(IEditorView view);
         void TriggerCompletionSession(IEditorView view);
        void DismissCompletionSession(IEditorView view);
    }
}
