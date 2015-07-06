using System;
using System.Diagnostics;
using System.Windows.Media;

namespace Microsoft.R.Editor.Completion
{
    using Completion = Microsoft.VisualStudio.Language.Intellisense.Completion;

    [DebuggerDisplay("{DisplayText}")]
    public class RCompletion : Completion
    {
        private Func<string> _descriptionCallback;
        public bool RetriggerIntellisense { get; private set; }

        public RCompletion(
            string displayText,
            string insertionText,
            Func<string> descriptionCallback,
            ImageSource iconSource,
            string iconAutomationText) :
            base(displayText, insertionText, String.Empty, iconSource, iconAutomationText)
        {
            _descriptionCallback = descriptionCallback;
        }

        public RCompletion(
            string displayText,
            string insertionText,
            string description,
            ImageSource iconSource,
            string iconAutomationText,
            int charactersBeforeCaret,
            bool retriggerIntellisense) :
            base(displayText, insertionText, description, iconSource, iconAutomationText)
        {
            this.CharactersBeforeCaret = charactersBeforeCaret;
            this.RetriggerIntellisense = retriggerIntellisense;
        }

        public override string Description
        {
            get
            {
                if (_descriptionCallback != null)
                {
                    return _descriptionCallback();
                }

                return base.Description;
            }

            set
            {
                base.Description = value;
            }
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
