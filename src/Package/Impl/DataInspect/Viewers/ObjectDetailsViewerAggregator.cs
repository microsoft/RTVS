// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.R.Debugger;

namespace Microsoft.VisualStudio.R.Package.DataInspect.Viewers {
    [Export(typeof(IObjectDetailsViewerAggregator))]
    internal sealed class ObjectDetailsViewerAggregator : IObjectDetailsViewerAggregator {
        [ImportMany]
        internal IEnumerable<Lazy<IObjectDetailsViewer>> Viewers { get; set; }

        [Import]
        private IDataObjectEvaluator Evaluator { get; set; }

        public async Task<IObjectDetailsViewer> GetViewer(string expression) {
            var preliminary = await Evaluator.EvaluateAsync(expression,
                                DebugEvaluationResultFields.Classes | DebugEvaluationResultFields.Dim | DebugEvaluationResultFields.Length,
                                null)
                                as DebugValueEvaluationResult;
            if (preliminary != null) {
                return GetViewer(preliminary);
            }
            return null;
        }

        public IObjectDetailsViewer GetViewer(DebugValueEvaluationResult result) {
            Lazy<IObjectDetailsViewer> lazyViewer = Viewers.FirstOrDefault(x => x.Value.CanView(result));
            return lazyViewer?.Value;
        }
    }
}
