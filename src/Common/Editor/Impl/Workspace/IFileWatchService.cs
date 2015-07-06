using System;
using System.IO;

namespace Microsoft.Languages.Editor.Workspace
{
    public interface IFileWatchService
    {
        /// <summary>
        /// Begins watching file for changes. When file changes, gets renamed or deleted
        /// then callback method is called with the file system event arguments.
        /// </summary>
        /// <param name="fileName">Full path to the file to watch</param>
        /// <param name="callback">Callback to invoke when file changes</param>
        void RegisterFileWatch(string filePath, Action<FileSystemEventArgs> callback);

        /// <summary>
        /// Removes callback/file pair from the notification list
        /// </summary>
        /// <param name="fileName">File full path</param>
        /// <param name="callback">Callback action to remove</param>
        void UnregisterFileWatch(string filePath, Action<FileSystemEventArgs> callback);
    }
}
