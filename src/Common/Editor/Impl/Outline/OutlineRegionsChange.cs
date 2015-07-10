using Microsoft.Languages.Core.Text;

namespace Microsoft.Languages.Editor.Outline
{
    public sealed class OutlineRegionsChange
    {
        public ITextRange ChangedRange { get; private set; }
        public OutlineRegionCollection NewRegions { get; private set; }

        public OutlineRegionsChange(ITextRange changedRange, OutlineRegionCollection newRegions)
        {
            ChangedRange = changedRange;
            NewRegions = newRegions;
        }
    }
}
