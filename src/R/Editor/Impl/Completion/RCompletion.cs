using System;
using System.Diagnostics;
using System.Windows.Media;

namespace Microsoft.R.Editor.Completion
{
    using Completion = Microsoft.VisualStudio.Language.Intellisense.Completion;

    [DebuggerDisplay("{DisplayText}")]
    public class RCompletion : Completion
    {
        public bool RetriggerIntellisense { get; private set; }

        public RCompletion(
            string displayText,
            string insertionText,
            string description,
            ImageSource iconSource) :
            base(displayText, insertionText, String.Empty, iconSource, string.Empty)
        {
        }

        public RCompletion(
            string displayText,
            string insertionText,
            string description,
            ImageSource iconSource,
            int charactersBeforeCaret,
            bool retriggerIntellisense) :
            this(displayText, insertionText, description, iconSource)
        {
            this.CharactersBeforeCaret = charactersBeforeCaret;
            this.RetriggerIntellisense = retriggerIntellisense;
        }

        public int CharactersBeforeCaret { get; private set; }

        public static int Compare(Completion completion1, Completion completion2)
        {
            if (completion1 == null || completion2 == null)
                return -1;

            int value = String.Compare(completion1.DisplayText, completion2.DisplayText, StringComparison.OrdinalIgnoreCase);
            if (0 == value)
                value = String.Compare(completion1.IconAutomationText, completion2.IconAutomationText, StringComparison.OrdinalIgnoreCase);

            return value;
        }
    }
}
