// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using Microsoft.Common.Core.Shell;
using Microsoft.Languages.Editor.TaskList;

namespace Microsoft.VisualStudio.R.Package.TaskList {
    /// <summary>
    /// Implements tsk list in VS environment. Exported via MEF
    /// and imported as IEditorTaskList in Microsoft.R.Editor 
    /// </summary>
    [Export(typeof(IEditorTaskList))]
    public sealed class VsTaskList : IEditorTaskList {
        private readonly ICoreShell _coreShell;
        private readonly Dictionary<IEditorTaskListItemSource, VsTaskListProvider> 
            _providerMap = new Dictionary<IEditorTaskListItemSource, VsTaskListProvider>();

        [ImportingConstructor]
        public VsTaskList(ICoreShell coreShell) {
            _coreShell = coreShell;
        }

        public void AddTaskSource(IEditorTaskListItemSource source) {
            Debug.Assert(!_providerMap.ContainsKey(source));

            var provider = new VsTaskListProvider(source, _coreShell.Services);
            _providerMap[source] = provider;
        }

        public void RemoveTaskSource(IEditorTaskListItemSource source) {
            if (_providerMap.ContainsKey(source)) {
                var provider = _providerMap[source];
                _providerMap.Remove(source);

                provider.Dispose();
            }
        }

        public void FlushTaskList() {
            foreach (var provider in _providerMap.Values) {
                provider.FlushTasks();
                provider.Refresh();
            }
        }

#pragma warning disable 67
        public event EventHandler<EventArgs> TaskListClosing;
    }
}