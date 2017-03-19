// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel.Composition;
using Microsoft.Common.Core.Shell;
using Microsoft.R.DataInspection;
using Microsoft.R.Debugger;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    [Export(typeof(IDebugGridViewProvider))]
    internal class DebugGridViewProvider : IDebugGridViewProvider {
        private readonly IObjectDetailsViewerAggregator _aggregator;
        private readonly ICoreShell _coreShell;

        [ImportingConstructor]
        public DebugGridViewProvider(ICoreShell coreShell, IObjectDetailsViewerAggregator aggregator) {
            _aggregator = aggregator;
            _coreShell = coreShell;
        }

        public bool CanShowDataGrid(IREvaluationResultInfo evaluationResult) {
            var wrapper = new VariableViewModel(evaluationResult, _aggregator, _coreShell);
            return wrapper.CanShowDetail;
        }

        public void ShowDataGrid(IREvaluationResultInfo evaluationResult) {
            var wrapper = new VariableViewModel(evaluationResult, _aggregator, _coreShell);
            if (!wrapper.CanShowDetail) {
                throw new InvalidOperationException("Cannot show data grid on evaluation result " + evaluationResult);
            }
            wrapper.ShowDetailCommand.Execute(null);
        }
    }
}
