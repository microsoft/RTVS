
namespace Microsoft.Languages.Editor.Workspace
{
    public interface IFileWatchServiceProvider
    {
        IFileWatchService CreateFileWatchService(string rootDirectory);
    }
}
