// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using Microsoft.Languages.Editor.Completions;
using Microsoft.Languages.Editor.Text;

namespace Microsoft.Languages.Editor.Signatures {
    public interface IViewSignatureBroker {
        IReadOnlyList<IEditorIntellisenseSession> GetSessions(IEditorView view);
        void TriggerSignatureSession(IEditorView view);
        void DismissSignatureSession(IEditorView view);
    }
}
