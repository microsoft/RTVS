using System;

namespace Microsoft.Languages.Editor.Workspace
{
    /// <summary>
    /// Abstraction of an item in a project/solution
    /// </summary>
    public interface IWorkspaceItem : IDisposable
    {
        /// <summary>
        /// Item moniker. For a disk-based document the same as PhysicalPath.
        /// May be something else for workspace items that are not disk items.
        /// </summary>
        string Moniker { get; }

        /// <summary>
        /// Physical path to the item on disk
        /// </summary>
        string Path { get; }
    }
}
