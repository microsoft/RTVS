// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using Microsoft.Common.Core.Shell;
using Microsoft.R.DataInspection;

namespace Microsoft.VisualStudio.R.Package.DataInspect.Viewers {
    [Export(typeof(IObjectDetailsViewer))]
    internal sealed class GridViewer : GridViewerBase {
        private const long MaxDim = 10000000;

        [ImportingConstructor]
        public GridViewer(ICoreShell coreShell, IDataObjectEvaluator evaluator) :
            base(coreShell.Services, evaluator) { }

        #region IObjectDetailsViewer

        public override bool CanView(IRValueInfo value) {
            // We can only view collections that have elements.
            var length = value?.Length ?? 0;
            if (length == 0) {
                return false;
            }

            // We can only view atomic vectors or lists.
            // Note that data.frame is always a list, and matrix and array can be either vector or list.
            if (!value.IsAtomic() && value.TypeName != "list") {
                return false;
            }

            // We can only view dimensionless (treated as 1D), 1D, or 2D collections. 
            // For 1D collections, only view if there's more than one element (for 2D, we still want
            // to enable grid in that case to expose row & column names).
            var dimCount = value.Dim?.Count ?? 1;
            if (dimCount > 2 || (dimCount == 1 && length == 1)) {
                return false;
            }

            // Grid viewer allocates arrays of doubles of the size of each dimension, to precompute cell coordinates.
            // If a given dimension is too large, the corresponding array can easily OOM on allocation. So, restrict
            // dimensions to reasonable sizes.
            foreach (var dim in value.Dim ?? new[] { length }) {
                if (dim > MaxDim) {
                    return false;
                }
            }

            return true;
        }

        #endregion
    }
}
