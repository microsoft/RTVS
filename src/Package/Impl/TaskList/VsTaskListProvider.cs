// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Languages.Editor.TaskList;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Packages.R;
using Microsoft.VisualStudio.Shell;

namespace Microsoft.VisualStudio.R.Package.TaskList {
    /// <summary>
    /// Provider of items for VS error list
    /// </summary>
    public sealed class VsTaskListProvider : ErrorListProvider {
        private static Guid _taskListProviderGuid = new Guid("FE76E4B5-D946-4508-B0BA-B59CA995AAC0");

        private Dictionary<IEditorTaskListItem, VsTaskItem> _itemMap = new Dictionary<IEditorTaskListItem, VsTaskItem>();
        private IEditorTaskListItemSource _source;
        private bool _dirty;

        public VsTaskListProvider(IEditorTaskListItemSource source)
            : base(RPackage.Current) {
            // Registration of the provider in VS is done by the base class.

            _source = source;
            _source.TasksAdded += OnTasksAdded;
            _source.TasksRemoved += OnTasksRemoved;
            _source.TasksCleared += OnTasksCleared;
            _source.TasksUpdated += OnTasksUpdated;
            _source.BeginUpdatingTasks += OnBeginUpdatingTasks;
            _source.EndUpdatingTasks += OnEndUpdatingTasks;

            Vsshell.Current.Idle += OnIdle;

            ProviderName = "R Language Service";
            ProviderGuid = _taskListProviderGuid;
        }

        private void OnTasksAdded(object sender, TasksListItemsChangedEventArgs e) {
            SuspendRefresh();
            foreach (var task in e.Tasks) {
                var vsTask = new VsTaskItem(task, _source);
                _itemMap[task] = vsTask;

                this.Tasks.Add(vsTask);
            }

            _dirty = true;
            ResumeRefresh();
        }

        private void OnTasksRemoved(object sender, TasksListItemsChangedEventArgs e) {
            SuspendRefresh();
            foreach (var task in e.Tasks) {
                VsTaskItem vsTask;
                if (_itemMap.TryGetValue(task, out vsTask)) {
                    this.Tasks.Remove(vsTask);
                    _itemMap.Remove(task);
                }
            }

            _dirty = true;
            ResumeRefresh();
        }

        private void OnTasksCleared(object sender, EventArgs e) {
            Clear();
        }

        private void OnTasksUpdated(object sender, EventArgs e) {
            _dirty = true;
        }

        private void OnBeginUpdatingTasks(object sender, EventArgs e) {
            SuspendRefresh();
        }

        private void OnEndUpdatingTasks(object sender, EventArgs e) {
            ResumeRefresh();
        }

        private void Clear() {
            Tasks.Clear();
            _itemMap.Clear();
            _dirty = true;
        }

        protected override void Dispose(bool disposing) {
            if (_source != null) {
                Vsshell.Current.Idle -= OnIdle;

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

        private void OnIdle(object sender, EventArgs eventArgs) {
            FlushTasks();
        }

        internal void FlushTasks() {
            if (_dirty) {
                _dirty = false;

                SuspendRefresh();

                foreach (VsTaskItem vsTaskItem in this.Tasks) {
                    vsTaskItem.Update();
                }

                ResumeRefresh();
            }
        }
    }
}
