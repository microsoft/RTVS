// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Shell;
using Microsoft.Languages.Editor.TaskList;
using Microsoft.VisualStudio.R.Packages.R;
using Microsoft.VisualStudio.Shell;

namespace Microsoft.VisualStudio.R.Package.TaskList {
    /// <summary>
    /// Provider of items for VS error list
    /// </summary>
    public sealed class VsTaskListProvider : ErrorListProvider {
        private static readonly Guid _taskListProviderGuid = new Guid("FE76E4B5-D946-4508-B0BA-B59CA995AAC0");

        private readonly Dictionary<IEditorTaskListItem, VsTaskItem> _itemMap = new Dictionary<IEditorTaskListItem, VsTaskItem>();
        private readonly IServiceContainer _services;
        private readonly IIdleTimeService _idleTime;
        private IEditorTaskListItemSource _source;
        private bool _dirty;

        public VsTaskListProvider(IEditorTaskListItemSource source, IServiceContainer services)
            : base(RPackage.Current) {
            // Registration of the provider in VS is done by the base class.

            _source = source;
            _source.TasksAdded += OnTasksAdded;
            _source.TasksRemoved += OnTasksRemoved;
            _source.TasksCleared += OnTasksCleared;
            _source.TasksUpdated += OnTasksUpdated;
            _source.BeginUpdatingTasks += OnBeginUpdatingTasks;
            _source.EndUpdatingTasks += OnEndUpdatingTasks;

            _services = services;
            _idleTime = services.IdleTime();
            _idleTime.Idle += OnIdle;

            ProviderName = "R Language Service";
            ProviderGuid = _taskListProviderGuid;
        }

        private void OnTasksAdded(object sender, TasksListItemsChangedEventArgs e) {
            SuspendRefresh();
            foreach (var task in e.Tasks) {
                var vsTask = new VsTaskItem(task, _source, _services);
                _itemMap[task] = vsTask;

                Tasks.Add(vsTask);
            }

            _dirty = true;
            ResumeRefresh();
        }

        private void OnTasksRemoved(object sender, TasksListItemsChangedEventArgs e) {
            SuspendRefresh();
            foreach (var task in e.Tasks) {
                if (_itemMap.TryGetValue(task, out var vsTask)) {
                    Tasks.Remove(vsTask);
                    _itemMap.Remove(task);
                }
            }

            _dirty = true;
            ResumeRefresh();
        }

        private void OnTasksCleared(object sender, EventArgs e) => Clear();
        private void OnTasksUpdated(object sender, EventArgs e) => _dirty = true;
        private void OnBeginUpdatingTasks(object sender, EventArgs e) => SuspendRefresh();
        private void OnEndUpdatingTasks(object sender, EventArgs e) => ResumeRefresh();

        private void Clear() {
            Tasks.Clear();
            _itemMap.Clear();
            _dirty = true;
        }

        protected override void Dispose(bool disposing) {
            if (_source != null) {
                _idleTime.Idle -= OnIdle;

                _source.TasksAdded -= OnTasksAdded;
                _source.TasksRemoved -= OnTasksRemoved;
                _source.TasksCleared -= OnTasksCleared;
                _source.TasksUpdated -= OnTasksUpdated;
                _source.BeginUpdatingTasks -= OnBeginUpdatingTasks;
                _source.EndUpdatingTasks -= OnEndUpdatingTasks;
                _source = null;
            }

            Clear();
            base.Dispose(disposing);
        }

        private void OnIdle(object sender, EventArgs eventArgs) => FlushTasks();

        internal void FlushTasks() {
            if (_dirty) {
                _dirty = false;

                foreach (VsTaskItem vsTaskItem in Tasks) {
                    vsTaskItem.Update();
                }
            }
        }
    }
}
