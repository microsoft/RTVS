// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics;
using System.Windows.Media;
using Microsoft.Languages.Editor.Completions;
using Microsoft.VisualStudio.Language.Intellisense;

namespace Microsoft.R.Editor.Completions {
    /// <summary>
    /// Completion entry in the R intellisense completion set
    /// </summary>
    [DebuggerDisplay("{" + nameof(DisplayText) + "}")]
    public sealed class RCompletion : Completion {
        public RCompletion(ICompletionEntry e) :
            base(e.DisplayText, e.InsertionText, e.Description, e.ImageSource as ImageSource, e.AccessibleText) { }

        public bool IsVisible { get; set; } = true;
    }
}
