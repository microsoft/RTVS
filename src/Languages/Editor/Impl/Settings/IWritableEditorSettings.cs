// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Languages.Core.Formatting;

namespace Microsoft.Languages.Editor.Settings {
    public interface IWritableEditorSettings {
        event EventHandler<EventArgs> SettingsChanged;
        void ResetSettings();
        bool AutoFormat { get; set; }
        bool CompletionEnabled { get; set; }
        int IndentSize { get; set; }
        IndentType IndentType { get; set; }
        int TabSize { get; set; }
        IndentStyle IndentStyle { get; set; }
        bool SyntaxCheckEnabled { get; set; }
        bool SignatureHelpEnabled { get; }
        bool InsertMatchingBraces { get; }
    }
}
