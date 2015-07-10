
using Microsoft.Languages.Editor.TaskList.Definitions;

namespace Microsoft.Languages.Editor.TaskList
{
    public class TaskListEntry
    {
        public TaskListEntry(IEditorTaskListItemSource source, IEditorTaskListItem taskItem)
        {
            TaskItem = taskItem;
            TaskSource = source;
        }

        public IEditorTaskListItemSource TaskSource
        {
            get;
            private set;
        }

        public IEditorTaskListItem TaskItem
        {
            get;
            private set;
        }
    }
}
