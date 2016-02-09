namespace Microsoft.VisualStudio.R.Package.Snippets.Definitions
{
    public interface ISnippetInfo
    {
        string Title { get; }
        string Description { get; }
        string Key { get; }
        string Path { get; }
        string Shortcut { get; }
        bool ShouldFormat { get; }
    }
}