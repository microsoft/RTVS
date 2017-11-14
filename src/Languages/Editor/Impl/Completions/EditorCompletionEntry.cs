// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using Microsoft.Languages.Editor.Completions;

namespace Microsoft.R.Editor.Completions {
    /// <summary>
    /// Completion entry in the R intellisense completion set
    /// </summary>
    [DebuggerDisplay("{" + nameof(DisplayText) + "}")]
    public class EditorCompletionEntry : ICompletionEntry, IComparable<ICompletionEntry> {
        public EditorCompletionEntry(string displayText, string insertionText, string description, object imageSource, string accessibleText = null) {
            DisplayText = displayText;
            InsertionText = insertionText;
            Description = description;
            ImageSource = imageSource;
            AccessibleText = accessibleText;
        }

        #region ICompletionEntry
        public bool IsVisible { get; set; } = true;
        public virtual string DisplayText { get; protected set; }
        public virtual string InsertionText { get; protected set; }
        public virtual string Description { get; protected set; }
        public virtual string AccessibleText { get; protected set; }
        public virtual object ImageSource { get; protected set; }
        public object Data { get; set; }
        #endregion

        public static int CompareOrdinal(ICompletionEntry completion1, ICompletionEntry completion2) 
            => Compare(completion1, completion2, StringComparison.Ordinal);

        public static int CompareIgnoreCase(ICompletionEntry completion1, ICompletionEntry completion2)
            => Compare(completion1, completion2, StringComparison.OrdinalIgnoreCase);

        public static int Compare(ICompletionEntry completion1, ICompletionEntry completion2, StringComparison comparison) {
            if (completion1 == null || completion2 == null) {
                return -1;
            }

            var value = string.Compare(completion1.DisplayText, completion2.DisplayText, comparison);
            if (0 == value) {
                value = string.Compare(completion1.AccessibleText, completion2.AccessibleText, comparison);
            }
            return value;
        }

        #region IComparable<ICompletionEntry>
        public int CompareTo(ICompletionEntry other) => CompareIgnoreCase(this, other);
        #endregion
    }
}
