// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.R.Components.Extensions;
using Microsoft.R.DataInspection;

namespace Microsoft.VisualStudio.R.Package.DataInspect.Viewers {
    [Export(typeof(IObjectDetailsViewer))]
    internal sealed class TableViewer : GridViewerBase {
        private readonly static string[] _tableClasses = new string[] { "matrix", "data.frame", "table", "array" };

        [ImportingConstructor]
        public TableViewer(IObjectDetailsViewerAggregator aggregator, IDataObjectEvaluator evaluator) :
            base(aggregator, evaluator) { }

        #region IObjectDetailsViewer
        public override bool CanView(IRValueInfo evaluation) {
            if (evaluation != null && evaluation.Classes.Any(t => _tableClasses.Contains(t))) {
                return evaluation.Dim != null && evaluation.Dim.Count == 2;
            }
            return false;
        }
        #endregion
    }
}
