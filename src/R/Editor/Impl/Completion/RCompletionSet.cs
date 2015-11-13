using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;

namespace Microsoft.R.Editor.Completion {
    using Core.Tokens;
    using Completion = VisualStudio.Language.Intellisense.Completion;

    internal sealed class RCompletionSet : CompletionSet {
        private ITextBuffer _textBuffer;
        private List<Completion> _completions = new List<Completion>();
        private IReadOnlyList<Completion> _filteredCompletions = new List<Completion>();

        public RCompletionSet(ITextBuffer textBuffer, ITrackingSpan trackingSpan, List<RCompletion> completions) :
            base("R Completion", "R Completion", trackingSpan, OrderList(completions), Enumerable.Empty<RCompletion>()) {
            _textBuffer = textBuffer;
            _completions.AddRange(completions);
            _filteredCompletions = OrderList(_completions);
        }

        public override void Filter() {
            this.Filter(CompletionMatchType.MatchDisplayText, caseSensitive: true);
        }

        private static IReadOnlyList<Completion> OrderList(IReadOnlyCollection<Completion> completions) {
            // Place 'name =' at the top prioritizing argument names
            // Place items starting with non-alpha characters like .Call and &&
            // at the end of the list.
            var argumentNames = completions.Where(x => RTokenizer.IsIdentifierCharacter(x.DisplayText[0]) && x.DisplayText.EndsWith("=", StringComparison.Ordinal));
            var specialNames = completions.Where(x => !char.IsLetter(x.DisplayText[0]));
            var generalEntries = completions.Except(argumentNames);
            generalEntries = generalEntries.Except(specialNames);

            List<Completion> orderedCompletions = new List<Completion>();
            orderedCompletions.AddRange(argumentNames);
            orderedCompletions.AddRange(generalEntries);
            orderedCompletions.AddRange(specialNames);

            return orderedCompletions;
        }
    }
}
