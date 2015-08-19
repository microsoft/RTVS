using System;
using System.Diagnostics;
using System.Windows.Media;

namespace Microsoft.R.Editor.Completion
{
    using Support.Utility.Definitions;
    using Support.Utility;
    using Completion = Microsoft.VisualStudio.Language.Intellisense.Completion;

    [DebuggerDisplay("{DisplayText}")]
    public class RCompletion : Completion
    {
        private IDataProvider<string> _descriptionProvider;

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
            IDataProvider<string> descriptionProvider,
            ImageSource iconSource,
            bool retriggerIntellisense = false) :
            this(displayText, insertionText, string.Empty, iconSource, retriggerIntellisense)
        {
            _descriptionProvider = descriptionProvider;
            this.RetriggerIntellisense = retriggerIntellisense;
        }

        public override string Description
        {
            get
            {
                string description = base.Description;

                if (string.IsNullOrEmpty(description) && _descriptionProvider != null)
                {
                    description = _descriptionProvider.Data;
                    base.Description = description;
                }

                return description;
            }

            set
            {
                base.Description = value;
            }
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
