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
        private readonly ICompletionEntry _entry;
        public RCompletion(ICompletionEntry entry) {
            _entry = entry;
        }

        public override string Description => _entry.Description;
        public override string DisplayText => _entry.DisplayText;
        public override string InsertionText => _entry.InsertionText;
        public override ImageSource IconSource => _entry.ImageSource as ImageSource;
        public override string IconAutomationText => _entry.AccessibleText;

        public bool IsVisible { get; set; } = true;
    }
}
