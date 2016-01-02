
namespace Microsoft.R.Editor.Completion.Definitions
{
    /// <summary>
    /// Get something (a string or WPF element) to show in a tooltip
    /// </summary>
    public interface IRQuickInfoProvider
    {
        object GetQuickInfo(int position);
    }
}
