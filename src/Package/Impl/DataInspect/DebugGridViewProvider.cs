// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel.Composition;
using Microsoft.R.Debugger;
using Microsoft.R.Debugger.Engine;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    [Export(typeof(IDebugGridViewProvider))]
    internal class DebugGridViewProvider : IDebugGridViewProvider {
        private readonly IObjectDetailsViewerAggregator _aggregator;

        [ImportingConstructor]
        public DebugGridViewProvider(IObjectDetailsViewerAggregator aggregator) {
            _aggregator = aggregator;
        }

        public bool CanShowDataGrid(DebugEvaluationResult evaluationResult) {
            var wrapper = new VariableViewModel(evaluationResult, _aggregator);
            return wrapper.CanShowDetail;
        }

        public void ShowDataGrid(DebugEvaluationResult evaluationResult) {
            var wrapper = new VariableViewModel(evaluationResult, _aggregator);
            if (!wrapper.CanShowDetail) {
                throw new InvalidOperationException("Cannot show data grid on evaluation result " + evaluationResult);
            }
            wrapper.ShowDetailCommand.Execute(null);
        }
    }
}
