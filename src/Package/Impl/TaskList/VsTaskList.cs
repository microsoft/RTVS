// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using Microsoft.Languages.Editor.Composition;
using Microsoft.Languages.Editor.TaskList.Definitions;

namespace Microsoft.VisualStudio.R.Package.TaskList {
    /// <summary>
    /// Implements tsk list in VS environment. Exported via MEF
    /// and imported as IEditorTaskList in Microsoft.R.Editor 
    /// </summary>
    [Export(typeof(IEditorTaskList))]
    public sealed class VsTaskList : IEditorTaskList {
        private Dictionary<IEditorTaskListItemSource, VsTaskListProvider> _providerMap = new Dictionary<IEditorTaskListItemSource, VsTaskListProvider>();

        public void AddTaskSource(IEditorTaskListItemSource source) {
            Debug.Assert(!_providerMap.ContainsKey(source));

            var provider = new VsTaskListProvider(source);
            _providerMap[source] = provider;
        }

        public void RemoveTaskSource(IEditorTaskListItemSource source) {
            if (_providerMap.ContainsKey(source)) {
                var provider = _providerMap[source];
                _providerMap.Remove(source);

                provider.Dispose();
            }
        }

        internal static void StaticFlushTaskList() {
            IEditorTaskList tasks = ComponentLocator<IEditorTaskList>.Import();
            tasks.FlushTaskList();
        }

        public void FlushTaskList() {
            foreach (VsTaskListProvider provider in _providerMap.Values) {
                provider.FlushTasks();
                provider.Refresh();
            }
        }

#pragma warning disable 67
        public event EventHandler<EventArgs> TaskListClosing;
    }
}