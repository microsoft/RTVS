// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.R.DataInspection;
using Microsoft.R.Debugger;

namespace Microsoft.VisualStudio.R.Package.DataInspect.Viewers {
    [Export(typeof(IObjectDetailsViewer))]
    internal sealed class Viewer1D : GridViewerBase {
        private readonly static string[] _classes = new string[] { "ts", "array" };

        [ImportingConstructor]
        public Viewer1D(IObjectDetailsViewerAggregator aggregator, IDataObjectEvaluator evaluator) :
            base(aggregator, evaluator) { }

        #region IObjectDetailsViewer
        public override bool CanView(IRValueInfo evaluation) {
            if (evaluation != null && evaluation.Classes.Any(t => _classes.Contains(t))) {
                return evaluation.Length.HasValue && evaluation.Length > 1 && (evaluation.Dim == null || evaluation.Dim.Count == 1);
            }
            return false;
        }
        #endregion
    }
}
