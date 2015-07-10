using System;
using System.Collections.Concurrent;
using Microsoft.Languages.Editor.TaskList.Definitions;

namespace Microsoft.Languages.Editor.TaskList
{
    /// <summary>
    /// Wrapper around task list item source that records events fired by the source
    /// and stores them for later processing in a background thread.
    /// </summary>
    internal class TaskSourceWrapper : IDisposable
    {
        private TaskList _errorList;

        public TaskSourceWrapper(TaskList errorList, IEditorTaskListItemSource source)
        {
            _errorList = errorList;

            TaskSource = source;
            Events = new ConcurrentQueue<TaskListSourceEvent>();

            TaskSource.TasksAdded += TaskSource_TasksAdded;
            TaskSource.TasksRemoved += TaskSource_TasksRemoved;
            TaskSource.TasksCleared += TaskSource_TasksCleared;
            TaskSource.TasksUpdated += TaskSource_TasksUpdated;
        }

        public event EventHandler<EventArgs> Changed;

        public IEditorTaskListItemSource TaskSource
        {
            get;
            private set;
        }

        public ConcurrentQueue<TaskListSourceEvent> Events
        {
            get;
            private set;
        }

        public bool IsDirty
        {
            get
            {
                return Events.Count > 0;
            }
        }

        private void TaskSource_TasksCleared(object sender, EventArgs e)
        {
            Enqueue(new TaskListSourceEvent(TaskListSourceEventType.ClearTasks, EventArgs.Empty));
        }

        private void TaskSource_TasksRemoved(object sender, TasksListItemsChangedEventArgs e)
        {
            Enqueue(new TaskListSourceEvent(TaskListSourceEventType.RemoveTasks, e));
        }

        private void TaskSource_TasksAdded(object sender, TasksListItemsChangedEventArgs e)
        {
            Enqueue(new TaskListSourceEvent(TaskListSourceEventType.AddTasks, e));
        }

        private void TaskSource_TasksUpdated(object sender, EventArgs e)
        {
            Enqueue(new TaskListSourceEvent(TaskListSourceEventType.UpdateTasks, e));
        }

        private void Enqueue(TaskListSourceEvent eventRecord)
        {
            if (Changed != null && Events != null)
            {
                Events.Enqueue(eventRecord);
                Changed(TaskSource, EventArgs.Empty);
            }
        }

        #region IDisposable
        public void Dispose()
        {
            if (TaskSource != null)
            {
                TaskSource.TasksAdded -= TaskSource_TasksAdded;
                TaskSource.TasksRemoved -= TaskSource_TasksRemoved;
                TaskSource.TasksCleared -= TaskSource_TasksCleared;
                TaskSource.TasksUpdated -= TaskSource_TasksUpdated;

                TaskSource = null;
            }

            if (Events != null)
            {
                while(!Events.IsEmpty)
                {
                    TaskListSourceEvent result;
                    Events.TryDequeue(out result);
                }

                Events = null;
            }
        }

        #endregion
    }
}
