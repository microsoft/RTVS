// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Microsoft.R.Host.Client;

namespace Microsoft.VisualStudio.R.Package.DataInspect.Viewers {
    [Export(typeof(IObjectViewer))]
    public sealed class ObjectDetailsViewerProvider : IObjectViewer {
        private readonly IObjectDetailsViewerAggregator _aggregator;
        private readonly IDebugObjectEvaluator _evaluator;

        [ImportingConstructor]
        public ObjectDetailsViewerProvider(IObjectDetailsViewerAggregator aggregator, IDebugObjectEvaluator evaluator) {
            _aggregator = aggregator;
            _evaluator = evaluator;
        }

        public async Task ViewObjectDetails(string expression, string title) {
            var viewer = await _aggregator.GetViewer(expression);
            if (viewer != null) {
                await viewer?.ViewAsync(expression, title);
            }
        }
    }
}
