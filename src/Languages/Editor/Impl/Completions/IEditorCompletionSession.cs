// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core.Services;
using Microsoft.Languages.Editor.Text;

namespace Microsoft.Languages.Editor.Completions {
    /// <summary>
    /// Represents active completion session in the editor.
    /// </summary>
    public interface IEditorCompletionSession: IPlatformSpecificObject, IPropertyHolder {
        IEditorView View { get; }
        bool IsDismissed { get; }

        event EventHandler Dismissed;
    }
}
