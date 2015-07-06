using Microsoft.VisualStudio.Text;

namespace Microsoft.Languages.Editor.Text
{
    public interface ITextSnapshotProvider
    {
        ITextSnapshot Snapshot { get; }
    }
}
