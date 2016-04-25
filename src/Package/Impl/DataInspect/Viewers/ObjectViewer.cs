// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Microsoft.R.Debugger;
using Microsoft.R.Host.Client;

namespace Microsoft.VisualStudio.R.Package.DataInspect.Viewers {
    [Export(typeof(IObjectViewer))]
    public sealed class ObjectViewer : IObjectViewer {
        private readonly IObjectDetailsViewerAggregator _aggregator;
        private readonly IDebugObjectEvaluator _evaluator;

        [ImportingConstructor]
        public ObjectViewer(IObjectDetailsViewerAggregator aggregator, IDebugObjectEvaluator evaluator) {
            _aggregator = aggregator;
            _evaluator = evaluator;
        }

        public Task ViewObject(string expression, string title) {
            return Task.Run(async () => {
                var classes = await _evaluator.EvaluateAsync(expression, DebugEvaluationResultFields.Classes) as DebugValueEvaluationResult;
                if (classes != null) {
                    var viewer = _aggregator.GetViewer(classes);
                    if (viewer != null) {
                        var result = await _evaluator.EvaluateAsync(expression, viewer.EvaluationFields) as DebugValueEvaluationResult;
                        if (result != null) {
                            await viewer?.ViewAsync(result);
                        }
                    }
                }
            });
        }
    }
}
