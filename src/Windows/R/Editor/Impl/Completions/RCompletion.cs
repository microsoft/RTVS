// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics;
using System.Windows.Media;
using Microsoft.VisualStudio.Language.Intellisense;

namespace Microsoft.R.Editor.Completions {
    /// <summary>
    /// Completion entry in the R intellisense completion set
    /// </summary>
    [DebuggerDisplay("{" + nameof(DisplayText) + "}")]
    public sealed class RCompletion : Completion {
        public RCompletion(string displayText, string insertionText, string description, ImageSource iconSource) :
            base(displayText, insertionText, description, iconSource, string.Empty) { }

        public bool IsVisible { get; set; } = true;
    }
}
