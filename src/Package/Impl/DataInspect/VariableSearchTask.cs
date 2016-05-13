// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Windows;
using Microsoft.Common.Core;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    internal sealed class VariableSearchTask : VsSearchTask {
        private readonly IVsSearchCallback _callback;
        private readonly TreeGrid _grid;

        public VariableSearchTask(TreeGrid grid, uint dwCookie, IVsSearchQuery pSearchQuery, IVsSearchCallback pSearchCallback)
            : base(dwCookie, pSearchQuery, pSearchCallback) {
            _grid = grid;
            _callback = pSearchCallback;
        }

        protected override void OnStartSearch() {
            base.OnStartSearch();

            var searchString = SearchQuery.SearchString;
            if (!string.IsNullOrWhiteSpace(searchString)) {
                foreach(var itemControl in _grid.Items) {
                    var tn = itemControl as ObservableTreeNode;
                    var model = tn?.Model?.Content as VariableViewModel;
                    if(model != null) {
                        if (model.Name.StartsWithOrdinal(searchString)) {
                            VsAppShell.Current.DispatchOnUIThread(() => _grid.SelectedItem = itemControl);
                            _callback.ReportComplete(this, 1);
                            return;
                        }
                    }
                }
            }
            _callback.ReportComplete(this, 0);
        }
    }
}
