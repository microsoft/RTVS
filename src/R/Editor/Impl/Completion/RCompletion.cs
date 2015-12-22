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

        public static int Compare(Completion completion1, Completion completion2) {
            if (completion1 == null || completion2 == null)
                return -1;

            int value = String.Compare(completion1.DisplayText, completion2.DisplayText, StringComparison.Ordinal);
            if (0 == value)
                value = String.Compare(completion1.IconAutomationText, completion2.IconAutomationText, StringComparison.Ordinal);

            return value;
        }

        #region IComparable<RCompletion>
        public int CompareTo(RCompletion other) {
            return DisplayText.CompareTo(other.DisplayText);
        }
        #endregion
    }
}
