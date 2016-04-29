// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.R.Debugger;

namespace Microsoft.VisualStudio.R.Package.DataInspect.Viewers {
    [Export(typeof(IObjectDetailsViewer))]
    internal sealed class VectorViewer : GridViewerBase {
        private readonly static string[] _excludedClasses = new string[] { "expression", "function", "factor", "environment" };

        [ImportingConstructor]
        public VectorViewer(IObjectDetailsViewerAggregator aggregator, IDataObjectEvaluator evaluator) :
            base(aggregator, evaluator) { }

        #region IObjectDetailsViewer
        public override bool CanView(IDebugValueEvaluationResult evaluation) {
            if (evaluation != null && !evaluation.Classes.Any(t => _excludedClasses.Contains(t))) {
                return evaluation.Dim == null && evaluation.Length > 1;
            }
            return false;
        }
        #endregion
    }
}
