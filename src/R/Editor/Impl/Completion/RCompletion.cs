using System;
using System.Diagnostics;
using System.Windows.Media;

namespace Microsoft.R.Editor.Completion
{
    using Completion = Microsoft.VisualStudio.Language.Intellisense.Completion;

    [DebuggerDisplay("{DisplayText}")]
    public class RCompletion : Completion, IComparable<RCompletion>
    {
        public bool RetriggerIntellisense { get; private set; }

        public RCompletion(
            string displayText,
            string insertionText,
            string description,
            ImageSource iconSource,
            bool retriggerIntellisense = false) :
            base(displayText, insertionText, description, iconSource, string.Empty)
        {
            this.RetriggerIntellisense = retriggerIntellisense;
        }

        public static int Compare(Completion completion1, Completion completion2)
        {
            if (completion1 == null || completion2 == null)
                return -1;

            int value = String.Compare(completion1.DisplayText, completion2.DisplayText, StringComparison.OrdinalIgnoreCase);
            if (0 == value)
                value = String.Compare(completion1.IconAutomationText, completion2.IconAutomationText, StringComparison.OrdinalIgnoreCase);

            return value;
        }

        #region IComparable<RCompletion>
        public int CompareTo(RCompletion other)
        {
            return DisplayText.CompareTo(other.DisplayText);
        }
        #endregion
    }
}
