using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Microsoft.Languages.Editor.TaskList.Definitions;
using Microsoft.Languages.Editor.Tasks;

namespace Microsoft.Languages.Editor.TaskList
{
    // [Export(typeof(IWebTaskList))]
    public sealed class TaskList : IEditorTaskList, IDisposable
    {
        public event EventHandler<EventArgs> BeginUpdate;
        public event EventHandler<TasksAddedEventArgs> TasksAdded;
        public event EventHandler<TasksRemovedEventArgs> TasksRemoved;
        public event EventHandler<TasksUpdatedEventArgs> TasksUpdated;
        public event EventHandler<EventArgs> EndUpdate;

        /// <summary>
        /// Task list entries
        /// </summary>
        public ReadOnlyCollection<TaskListEntry> TaskListEntries { get; private set; }

        /// <summary>
        /// Task sources - typically one per file but in some cases there
        /// may be more than one, like in HTML where script, CSS, HTML
        /// and server language may be providing separate sources.
        /// </summary>
        private List<TaskSourceWrapper> _taskSources = new List<TaskSourceWrapper>();
        private List<TaskListEntry> _taskListEntries = new List<TaskListEntry>();

        private IdleTimeAsyncTask _collectionUpdateTask;
        private object _taskListLock = new object();
        private long _disposed = 0;
        private bool _processingComplete = true;

        public TaskList()
        {
            TaskListEntries = new ReadOnlyCollection<TaskListEntry>(_taskListEntries);
            _collectionUpdateTask = new IdleTimeAsyncTask(ProcessPendingEvents, UiThreadCollectionUpdate);
        }

        #region IWebTaskList Members
        public event EventHandler<EventArgs> TaskListClosing;

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Stored in the list, Disposed when removed.")]
        public void AddTaskSource(IEditorTaskListItemSource itemSource)
        {
            if (!this.IsDisposed())
            {
                lock (_taskListLock)
                {
                    var source = new TaskSourceWrapper(this, itemSource);

                    source.Changed += OnTaskItemsChanged;
                    _taskSources.Add(source);
                }
            }
        }

        public void RemoveTaskSource(IEditorTaskListItemSource source)
        {
            if (!this.IsDisposed())
            {
                // Careful here: background thread might be accessing
                // the collection as we want to remove the source.
                lock (_taskListLock)
                {
                    for (int i = 0; i < _taskSources.Count; i++)
                    {
                        var ts = _taskSources[i];

                        if (ts.TaskSource == source)
                        {
                            ts.Events.Enqueue(new TaskListSourceEvent(TaskListSourceEventType.FinalRemove, EventArgs.Empty));
                            OnTaskItemsChanged(ts, EventArgs.Empty);
                            break;
                        }
                    }
                }
            }
        }

        public void FlushTaskList()
        {
            // Could call ProcessPendingEvents(), but this function is only used in VS tests anyway
        }

        private bool EventsPending
        {
            get
            {
                lock (_taskListLock)
                {
                    foreach (var taskSourceWrapper in _taskSources)
                    {
                        if (taskSourceWrapper.IsDirty)
                        {
                            return true;
                        }
                    }
                }

                return false;
            }
        }

        private void OnTaskItemsChanged(object sender, EventArgs e)
        {
            // Don't schedule task is it is already scheduled.
            //
            // Don't schedule task until previous set of observable
            // collection updates gets through.
            //
            if (!this.IsDisposed())
            {
                if (_processingComplete)
                {
                    _processingComplete = false;
                    _collectionUpdateTask.DoTaskOnIdle(1000);
                }
            }
        }

        /// <summary>
        /// Processes events recorded from task sources and calculates
        /// operations on the main observable collection. This method
        /// is executed on a background thread.
        /// </summary>
        /// <returns></returns>
        private object ProcessPendingEvents()
        {
            if (this.IsDisposed())
            {
                return null;
            }

            var operation = new TaskListItemCollectionOperation();

            // We need to eliminate items that are in 'add' list  that also appears in 'remove' list.
            // This may normally happen when first items are considered invalid and then get 
            // revalidated and their original tasks are marked for removal. This typically
            // happen in Razor files since first parse and validation pass knows nothing about
            // Razor syntax. When Razor background parser finishes processing second HTML parsing
            // pass is initiated and this time it knows about ranges it needs to skip over.
            // However, similar things may happen in regular files too since parsing and validation
            // are asynchronous threads.
            //
            // Also, some items to remove may not be in the observable collection yet.
            // They may be in the 'add items' list instead since this thread
            // might not have had a chance to handle queued events yet. However, these
            // items normally appear after added items in the event queue as
            // item has to be added before it can be removed.
            //
            var addedSet = new Dictionary<IEditorTaskListItem, IEditorTaskListItemSource>();
            var updatedSet = new HashSet<IEditorTaskListItem>();
            var removedSet = new HashSet<IEditorTaskListItem>();
            var removedSources = new HashSet<IEditorTaskListItemSource>();

            lock (_taskListLock)
            {
                // Process events from all task sources that are dirty
                foreach (var taskSourceWrapper in _taskSources)
                {
                    if (taskSourceWrapper.Events == null || !taskSourceWrapper.IsDirty)
                    {
                        continue;
                    }

                    bool sourceRemoved = false;

                    while (taskSourceWrapper.Events.Count > 0 && !sourceRemoved)
                    {
                        TaskListSourceEvent e;

                        if (taskSourceWrapper.Events.TryDequeue(out e))
                        {
                            var args = e.EventArgs as TasksListItemsChangedEventArgs;
                            switch (e.EventType)
                            {
                                case TaskListSourceEventType.AddTasks:
                                    if (args != null)
                                    {
                                        foreach (var task in args.Tasks)
                                        {
                                            addedSet[task] = taskSourceWrapper.TaskSource;
                                        }
                                    }

                                    break;

                                case TaskListSourceEventType.RemoveTasks:
                                    if (args != null)
                                    {
                                        foreach (var task in args.Tasks)
                                        {
                                            removedSet.Add(task);
                                        }
                                    }

                                    break;

                                case TaskListSourceEventType.UpdateTasks:
                                    // Note that queue may contain requests to update items
                                    // that are to be removed. Normally update requests
                                    // reside in the event queue between 'add' and 'remove' 
                                    // records.
                                    for (int i = 0; i < TaskListEntries.Count; i++)
                                    {
                                        var entry = TaskListEntries[i];
                                        if (entry.TaskSource == taskSourceWrapper.TaskSource)
                                        {
                                            updatedSet.Add(entry.TaskItem);
                                        }
                                    }

                                    break;

                                case TaskListSourceEventType.ClearTasks:
                                    // Mark all entries in the task list that originate
                                    // from this source for removal.
                                    for (int i = 0; i < TaskListEntries.Count; i++)
                                    {
                                        var entry = TaskListEntries[i];
                                        if (entry.TaskSource == taskSourceWrapper.TaskSource)
                                        {
                                            removedSet.Add(entry.TaskItem);
                                        }
                                    }

                                    // Now also remove any entries that are not in the task list yet
                                    // but rather are sitting in the event queue like when 'tasks added'
                                    // event is quickly followed by 'tasks cleared' event
                                    var removedTasks = new List<IEditorTaskListItem>();
                                    foreach (var item in addedSet)
                                    {
                                        if (item.Value == taskSourceWrapper.TaskSource)
                                        {
                                            removedTasks.Add(item.Key);
                                        }
                                    }

                                    foreach (var item in removedTasks)
                                    {
                                        addedSet.Remove(item);
                                    }

                                    break;

                                case TaskListSourceEventType.FinalRemove:

                                    operation.RemoveSources.Add(taskSourceWrapper);
                                    removedSources.Add(taskSourceWrapper.TaskSource);

                                    // If source is removed we don't want to continue processing events from it
                                    sourceRemoved = true;
                                    break;
                            }
                        }
                    }
                }

                var removeFromRemoved = new List<IEditorTaskListItem>();

                foreach (var removed in removedSet)
                {
                    if (addedSet.ContainsKey(removed))
                    {
                        addedSet.Remove(removed);
                        removeFromRemoved.Add(removed);
                    }

                    if (updatedSet.Contains(removed))
                    {
                        updatedSet.Remove(removed);
                    }
                }

                foreach (var r in removeFromRemoved)
                {
                    removedSet.Remove(r);
                }

                // Now generate operation on the observable collection.
                // At this point 'added' set should only contain true 
                // new items while 'updated' set contains only items
                // that stay in the collection. 'removed' set only contains
                // items that are already in the collection.

                // Now figure out how to update observable collection
                if (removedSet.Count > 0 || updatedSet.Count > 0)
                {
                    for (int i = 0; i < TaskListEntries.Count; i++)
                    {
                        var entry = TaskListEntries[i];

                        if (removedSet.Contains(entry.TaskItem))
                        {
                            operation.RemoveItems.Add(i);
                            removedSet.Remove(entry.TaskItem);
                        }
                        else
                        {
                            if (updatedSet.Contains(entry.TaskItem))
                            {
                                operation.UpdateItems.Add(i);
                                updatedSet.Remove(entry.TaskItem);
                            }
                        }
                    }
                }

                foreach (var added in addedSet)
                {
                    // Make sure we don't add items that belong to a source
                    // that is being removed
                    var addedItemSource = added.Value;
                    if (!removedSources.Contains(addedItemSource) && SourceExists(addedItemSource))
                    {
                        operation.AddItems.Add(new TaskListEntry(added.Value, added.Key));
                    }
                }
            }

            return operation;
        }

        private bool SourceExists(IEditorTaskListItemSource source)
        {
            foreach (var ts in _taskSources)
            {
                if (ts.TaskSource == source)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// This method is called on UI thread when background processing 
        /// of task source events is complete.
        /// </summary>
        /// <param name="o"></param>
        private void UiThreadCollectionUpdate(object o)
        {
            var operation = o as TaskListItemCollectionOperation;
            var removedEntries = new List<TaskListEntry>();

            if (operation != null)
            {
                lock (_taskListLock)
                {
                    //foreach (var index in operation.UpdateItems)
                    //{
                    //    TaskListEntries[index].FirePropertyChanged();
                    //}

                    operation.RemoveItems.Sort();

                    for (int i = operation.RemoveItems.Count - 1; i >= 0; i--)
                    {
                        var index = operation.RemoveItems[i];

                        removedEntries.Add(_taskListEntries[index]);
                        _taskListEntries.RemoveAt(index);
                    }

                    foreach (var item in operation.AddItems)
                    {
                        _taskListEntries.Add(item);
                    }

                    // Always process source removal last so we don't get odd items that somehow 
                    // got enqueued after source was removed. This might happen if, say, some 
                    // document validation thread keeps running while document is already closed.
                    for (int i = 0; i < operation.RemoveSources.Count; i++)
                    {
                        var sourceWrapper = operation.RemoveSources[i];

                        _taskSources.Remove(sourceWrapper);

                        for (int j = 0; j < TaskListEntries.Count; j++)
                        {
                            var item = TaskListEntries[j];

                            if (item.TaskSource == sourceWrapper.TaskSource)
                            {
                                removedEntries.Add(_taskListEntries[j]);
                                _taskListEntries.RemoveAt(j);
                                j--;
                            }
                        }

                        sourceWrapper.Changed -= OnTaskItemsChanged;
                        sourceWrapper.Dispose();
                    }

                }

                if (EventsPending)
                {
                    if (!IsDisposed())
                    {
                        _collectionUpdateTask.DoTaskOnIdle();
                    }
                }
                else
                {
                    _processingComplete = true;
                }

                if (BeginUpdate != null)
                    BeginUpdate(this, EventArgs.Empty);

                if (TasksUpdated != null)
                    TasksUpdated(this, new TasksUpdatedEventArgs(operation.UpdateItems));

                if (TasksRemoved != null)
                    TasksRemoved(this, new TasksRemovedEventArgs(operation.RemoveItems));

                if (TasksAdded != null)
                    TasksAdded(this, new TasksAddedEventArgs(operation.AddItems));

                if (EndUpdate != null)
                    EndUpdate(this, EventArgs.Empty);
            }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            Interlocked.Increment(ref _disposed);

            if (_collectionUpdateTask != null)
            {
                _collectionUpdateTask.Dispose();
                _collectionUpdateTask = null;
            }

            lock (_taskListLock)
            {
                if (_taskSources != null)
                {
                    foreach (var source in _taskSources)
                    {
                        source.Changed -= OnTaskItemsChanged;
                        source.Dispose();
                    }

                    _taskSources.Clear();
                }
            }

            if (TaskListClosing != null)
                TaskListClosing(this, EventArgs.Empty);
        }

        private bool IsDisposed()
        {
            return Interlocked.Read(ref _disposed) > 0;
        }
        #endregion
    }

    public class TasksAddedEventArgs : EventArgs
    {
        public ReadOnlyCollection<TaskListEntry> Tasks { get; private set; }

        public TasksAddedEventArgs(List<TaskListEntry> tasks)
        {
            Tasks = new ReadOnlyCollection<TaskListEntry>(tasks);
        }
    }

    public class TasksRemovedEventArgs : EventArgs
    {
        public ReadOnlyCollection<int> TaskIndices { get; private set; }

        public TasksRemovedEventArgs(List<int> tasks)
        {
            TaskIndices = new ReadOnlyCollection<int>(tasks);
        }
    }

    public class TasksUpdatedEventArgs : EventArgs
    {
        public ReadOnlyCollection<int> TaskIndices { get; private set; }

        public TasksUpdatedEventArgs(List<int> tasks)
        {
            TaskIndices = new ReadOnlyCollection<int>(tasks);
        }
    }
}
