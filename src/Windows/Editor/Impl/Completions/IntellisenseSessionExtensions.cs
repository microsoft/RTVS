// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.VisualStudio.Language.Intellisense;

namespace Microsoft.Languages.Editor.Completions {
    public static class IntellisenseSessionExtensions {
        public static IEditorIntellisenseSession ToEditorIntellisenseSession(this IIntellisenseSession session)
            => session.Properties.TryGetProperty(EditorIntellisenseSession.SessionKey, out IEditorIntellisenseSession es) ? es : null;
    }
}
