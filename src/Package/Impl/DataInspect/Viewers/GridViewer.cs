// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using Microsoft.R.DataInspection;

namespace Microsoft.VisualStudio.R.Package.DataInspect.Viewers {
    [Export(typeof(IObjectDetailsViewer))]
    internal sealed class GridViewer : GridViewerBase {
        [ImportingConstructor]
        public GridViewer(IObjectDetailsViewerAggregator aggregator, IDataObjectEvaluator evaluator) :
            base(aggregator, evaluator) { }

        #region IObjectDetailsViewer

        public override bool CanView(IRValueInfo value) {
            // We can only view collections that have elements.
            if (value?.Length == null || value.Length == 0) {
                return false;
            }

            // We can only view atomic vectors or lists.
            // Note that data.frame is always a list, and matrix and array can be either vector or list.
            if (!value.IsAtomic() && value.TypeName != "list") {
                return false;
            }

            // We can only view dimensionless, 1D, or 2D collections. 
            if (value.Dim != null && value.Dim.Count > 2) {
                return false;
            }

            return true;
        }

        #endregion
    }
}
