using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;

namespace Microsoft.R.Editor.Completion
{
    internal sealed class RCompletionSet: CompletionSet
    {
        public RCompletionSet(ITrackingSpan trackingSpan, List<RCompletion> completions) :
            base("R Completion", "R Completion", trackingSpan, completions, Enumerable.Empty<RCompletion>())
        {
        }

        public override void Filter()
        {
            this.Filter(CompletionMatchType.MatchDisplayText, caseSensitive: true);
        }
    }
}
