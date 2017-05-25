// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Languages.Core.Formatting;

namespace Microsoft.Languages.Editor.Settings {
    public interface IEditorSettings: IDisposable {
        event EventHandler<EventArgs> SettingsChanged;

        bool AutoFormat { get; }
        bool CompletionEnabled { get; }
        int IndentSize { get; }
        IndentType IndentType { get; }
        int TabSize { get; }
        IndentStyle IndentStyle { get; }
        bool SyntaxCheckEnabled { get; }
        bool SignatureHelpEnabled { get; }
        bool InsertMatchingBraces { get; }
    }
}
