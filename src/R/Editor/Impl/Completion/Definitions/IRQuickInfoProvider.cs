
namespace Microsoft.R.Editor.Completions.Definitions
{
    /// <summary>
    /// Get something (a string or WPF element) to show in a tooltip
    /// </summary>
    public interface IRQuickInfoProvider
    {
        object GetQuickInfo(int position);
    }
}
