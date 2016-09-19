// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.Windows.Media;

namespace Microsoft.R.Editor.Completion {
    using Completion = Microsoft.VisualStudio.Language.Intellisense.Completion;

    /// <summary>
    /// Completion entry in the R intellisense completion set
    /// </summary>
    [DebuggerDisplay("{DisplayText}")]
    public class RCompletion : Completion, IComparable<RCompletion> {
        public RCompletion(
            string displayText,
            string insertionText,
            string description,
            ImageSource iconSource) :
            base(displayText, insertionText, description, iconSource, string.Empty) {
        }

        public bool IsVisible { get; set; } = true;

        public static int Compare(Completion completion1, Completion completion2) {
            if (completion1 == null || completion2 == null)
                return -1;

            int value = String.Compare(completion1.DisplayText, completion2.DisplayText, StringComparison.OrdinalIgnoreCase);
            if (0 == value)
                value = String.Compare(completion1.IconAutomationText, completion2.IconAutomationText, StringComparison.OrdinalIgnoreCase);

            return value;
        }

        #region IComparable<RCompletion>
        public int CompareTo(RCompletion other) {
            return string.Compare(this.DisplayText, other.DisplayText, StringComparison.OrdinalIgnoreCase);
        }
        #endregion
    }
}
