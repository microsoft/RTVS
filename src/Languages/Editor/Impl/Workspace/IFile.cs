
namespace Microsoft.Languages.Editor.Workspace
{
    public interface IFile
    {
        /// <summary>
        /// File name
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Containing folder
        /// </summary>
        IFolder Folder { get; }
    }
}
