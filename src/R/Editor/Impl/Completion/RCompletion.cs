using System;
using System.Diagnostics;
using System.Windows.Media;

namespace Microsoft.R.Editor.Completion
{
    using Support.Utility;
    using Completion = Microsoft.VisualStudio.Language.Intellisense.Completion;

    [DebuggerDisplay("{DisplayText}")]
    public class RCompletion : Completion
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

        public RCompletion(
            string displayText,
            string insertionText,
            AsyncDataSource<string> descriptionSource,
            ImageSource iconSource,
            bool retriggerIntellisense = false) :
            this(displayText, insertionText, string.Empty, iconSource, retriggerIntellisense)
        {
            descriptionSource.DataReady += OnDescriptionDataReady;
            this.RetriggerIntellisense = retriggerIntellisense;
        }

        private void OnDescriptionDataReady(object sender, string data)
        {
            this.Description = data;
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
    }
}
