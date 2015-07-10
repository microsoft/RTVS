using System.Collections.Generic;

namespace Microsoft.Languages.Editor.TaskList
{
    /// <summary>
    /// Describes operations that need to be performed
    /// on observable colletction of task list items
    /// </summary>
    internal sealed class TaskListItemCollectionOperation
    {
        /// <summary>
        /// A collection of identifiers of items
        /// that should be removed from the collection.
        /// </summary>
        public List<int> RemoveItems
        {
            get;
            private set;
        }

        /// <summary>
        /// New items that are to be added to the observable
        /// collection feeding task list control.
        /// </summary>
        public List<TaskListEntry> AddItems
        {
            get;
            private set;
        }

        /// <summary>
        /// Identifiers of items that have some of their
        /// properties (typically lines and columns) updated.
        /// </summary>
        public List<int> UpdateItems
        {
            get;
            private set;
        }

        public List<TaskSourceWrapper> RemoveSources
        {
            get;
            private set;
        }

        public TaskListItemCollectionOperation()
        {
            RemoveItems = new List<int>();
            AddItems = new List<TaskListEntry>();
            UpdateItems = new List<int>();
            RemoveSources = new List<TaskSourceWrapper>();
        }
    }
}
