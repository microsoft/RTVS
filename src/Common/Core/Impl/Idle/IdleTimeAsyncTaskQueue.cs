// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Shell;

namespace Microsoft.Common.Core.Idle {
    /// <summary>
    /// A queue of asynchronous tasks that are processed in the order they were added. As opposed to thread
    /// pool tasks only start on idle and queued task may be canceled if needed.
    /// There may be one or more tasks running in parallel depending on number of CPUs avaialable.
    /// </summary>
    public class IdleTimeAsyncTaskQueue {
        class TaskQueueEntry {
            public Func<object> TaskAction { get; private set; }
            public Action<object> CallbackAction { get; private set; }
            public Action<object> CancelAction { get; private set; }
            public object Tag { get; private set; }

            public TaskQueueEntry(Func<object> taskAction, Action<object> callbackAction, Action<object> cancelAction, object tag) {
                TaskAction = taskAction;
                CallbackAction = callbackAction;
                CancelAction = cancelAction;
                Tag = tag;
            }
        }

        private readonly IServiceContainer _services;
        private readonly IIdleTimeService _idleTime;
        private readonly List<TaskQueueEntry> _taskQueue = new List<TaskQueueEntry>();
        private readonly IdleTimeAsyncTask[] _workerTasks;
        private bool _connectedToIdle = false;

        public IdleTimeAsyncTaskQueue(IServiceContainer services) {
            _services = services;
            _idleTime = services.GetService<IIdleTimeService>();

            var logicalCpuCount = Environment.ProcessorCount;
            var taskCount = logicalCpuCount / 4;
            if (taskCount == 0) {
                taskCount = 1;
            }

            _workerTasks = new IdleTimeAsyncTask[taskCount];

            for (var i = 0; i < _workerTasks.Length; i++) {
                _workerTasks[i] = new IdleTimeAsyncTask(_services);
            }
        }

        /// <summary>
        /// Add task to the idle time queue. Tasks are executed asynchronously in the order they were added.
        /// On next idle time if thread is available it will take task from the head of the queue and execute it. 
        /// There may be one or more tasks running in parallel depending on number of CPUs avaialable.
        /// </summary>
        public void Enqueue(Func<object> taskAction, Action<object> callbackAction, Action<object> cancelAction, object tag) {
            _taskQueue.Add(new TaskQueueEntry(taskAction, callbackAction, cancelAction, tag));
            ConnectToIdle();
        }

        /// <summary>
        /// Add task to the idle time queue. Tasks are executed asynchronously in the order they were added.
        /// On next idle time if thread is available it will take task from the head of the queue and execute it. 
        /// There may be one or more tasks running in parallel depending on number of CPUs avaialable.
        /// </summary>
        public void Enqueue(Func<object> taskAction, Action<object> callbackAction, object tag)
            => Enqueue(taskAction, callbackAction, null, tag);

        /// <summary>
        /// Removes tasks associated with a give callback
        /// </summary>
        /// <param name="tag">Object uniquely indentifying the task</param>
        public void CancelTasks(object tag) {
            if (_taskQueue.Count > 0) {
                for (var i = _taskQueue.Count - 1; i >= 0; i--) {
                    if (_taskQueue[i].Tag == tag) {
                        _taskQueue.RemoveAt(i);
                    }
                }

                if (_taskQueue.Count == 0) {
                    DisconnectFromIdle();
                }
            }
        }

        public void IncreasePriority(object tag) {
            for (var i = 0; i < _taskQueue.Count; i++) {
                var task = _taskQueue[i];

                if (task.Tag == tag) {
                    _taskQueue.RemoveAt(i);
                    _taskQueue.Insert(0, task);
                }
            }
        }

        private void ConnectToIdle() {
            if (!_connectedToIdle) {
                _connectedToIdle = true;
                _idleTime.Idle += OnIdle;
            }
        }

        private void DisconnectFromIdle() {
            if (_connectedToIdle) {
                _connectedToIdle = false;

                // We're holding onto these tasks in a static, let's clean them up
                //   Otherwise, they could be pointing to closed documents/views
                //   or other stale data that the Tag or callbacks hold onto.
                for (var i = 0; i < _workerTasks.Length; i++) {
                    _workerTasks[i] = new IdleTimeAsyncTask(_services);
                }
                _idleTime.Idle -= OnIdle;
            }
        }

        private void OnIdle(object sender, EventArgs e) {
            for (var i = 0; i < _taskQueue.Count; i++) {
                var taskEntry = _taskQueue[i];

                if (!GetAvailableTask(taskEntry.Tag, out var worker)) {
                    return; // all worker threads are busy
                }

                if (worker != null) {
                    _taskQueue.RemoveAt(i);

                    worker.DoTaskOnIdle(taskEntry.TaskAction, taskEntry.CallbackAction, taskEntry.CancelAction, taskEntry.Tag);
                    if (_taskQueue.Count == 0) {
                        DisconnectFromIdle();
                    }

                    break;
                }
            }
        }

        private bool GetAvailableTask(object tag, out IdleTimeAsyncTask worker) {
            // Ensure no current workers are processing work items with the same tag.
            // This ensures object thread affinity so no two HTML validators 
            // will run in the same document.

            worker = null;
            var thisTagIsRunning = false;

            foreach (var candidate in _workerTasks) {
                if (candidate.TaskRunning && candidate.Tag == tag) {
                    // Task with this tag is already running, try another task maybe
                    thisTagIsRunning = true;
                } else if (!candidate.TaskRunning) {
                    worker = candidate;
                }
            }

            var workerAvailable = worker != null;
            if (thisTagIsRunning) {
                worker = null; // worker is available but not for this task
            }
            return workerAvailable; // some task is available
        }
    }
}
