using System;

namespace Microsoft.Languages.Editor.TaskList
{
    internal class TaskListSourceEvent
    {
        public TaskListSourceEventType EventType
        {
            get;
            private set;
        }

        public EventArgs EventArgs
        {
            get;
            private set;
        }

        public TaskListSourceEvent(TaskListSourceEventType eventType, EventArgs eventArgs)
        {
            EventType = eventType;
            EventArgs = eventArgs;
        }
    }
}
