// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.R.Debugger;

namespace Microsoft.VisualStudio.R.Package.DataInspect.Viewers {
    [Export(typeof(IObjectDetailsViewer))]
    internal sealed class ListViewer : GridViewerBase {
        private readonly static string[] _classes = new string[] { "list" };

        [ImportingConstructor]
        public ListViewer(IObjectDetailsViewerAggregator aggregator, IDataObjectEvaluator evaluator) :
            base(aggregator, evaluator) { }

        #region IObjectDetailsViewer
        public override bool CanView(IDebugValueEvaluationResult evaluation) {
            return evaluation != null && evaluation.Classes.Any(t => _classes.Contains(t));
        }
        #endregion
    }
}
