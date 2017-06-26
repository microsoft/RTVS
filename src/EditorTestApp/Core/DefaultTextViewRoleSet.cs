// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.Languages.Editor.Application.Core {
    [ExcludeFromCodeCoverage]
    internal class DefaultTextViewRoleSet : TextViewRoleSet {
        private static readonly string[] _predefinedRoles = {
                PredefinedTextViewRoles.Analyzable,
                PredefinedTextViewRoles.Document,
                PredefinedTextViewRoles.Editable,
                PredefinedTextViewRoles.Interactive,
                PredefinedTextViewRoles.PrimaryDocument,
                PredefinedTextViewRoles.Structured,
                PredefinedTextViewRoles.Zoomable,
                PredefinedTextViewRoles.Debuggable
            };

        public DefaultTextViewRoleSet()
            : base(_predefinedRoles) {
        }
    }
}
