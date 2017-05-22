// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core.Services;
using Microsoft.Languages.Editor.Text;

namespace Microsoft.Languages.Editor.Completions {
    /// <summary>
    /// Represents active completion session in the editor.
    /// </summary>
    public interface IEditorIntellisenseSession : IPlatformSpecificObject, IPropertyHolder {
        /// <summary>
        /// Application global services
        /// </summary>
        IServiceContainer Services { get; }

        /// <summary>
        /// Associated editor view
        /// </summary>
        IEditorView View { get; }

        /// <summary>
        /// If true, the session was dismissed
        /// </summary>
        bool IsDismissed { get; }

        /// <summary>
        /// Fires when session is dismissed
        /// </summary>
        event EventHandler Dismissed;
    }
}
