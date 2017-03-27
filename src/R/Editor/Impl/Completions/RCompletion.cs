// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.Windows.Media;

namespace Microsoft.R.Editor.Completions {
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

        public static int CompareOrdinal(Completion completion1, Completion completion2) {
            return Compare(completion1, completion2, StringComparison.Ordinal);
        }

        public static int CompareIgnoreCase(Completion completion1, Completion completion2) {
            return Compare(completion1, completion2, StringComparison.OrdinalIgnoreCase);
        }

        public static int Compare(Completion completion1, Completion completion2, StringComparison comparison) {
            if (completion1 == null || completion2 == null) {
                return -1;
            }

            int value = String.Compare(completion1.DisplayText, completion2.DisplayText, comparison);
            if (0 == value) {
                value = String.Compare(completion1.IconAutomationText, completion2.IconAutomationText, comparison);
            }

            return value;
        }

        #region IComparable<RCompletion>
        public int CompareTo(RCompletion other) {
            return CompareIgnoreCase(this, other);
        }
        #endregion
    }
}
