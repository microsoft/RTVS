// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.R.DataInspection;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.R.Package.DataInspect.DataSource;
using static System.FormattableString;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    /// <summary>
    /// grid data provider to control
    /// </summary>
    internal sealed class GridDataProvider : IGridProvider<string> {
        private readonly VariableViewModel _evaluation;
        private readonly IRSession _session;

        public GridDataProvider(IRSession session, VariableViewModel evaluation) {
            _session = session;
            _evaluation = evaluation;

            RowCount = evaluation.Dimensions[0];
            ColumnCount = evaluation.Dimensions.Count >= 2 ? evaluation.Dimensions[1] : 1;
            CanSort = true;

            // Lists cannot be sorted, except when the list is a dataframe.
            if (evaluation.TypeName == "list") {
                var er = evaluation.DebugEvaluation as IRValueInfo;
                CanSort = er?.Classes.Contains("data.frame") == true;
            }
        }

        public long ColumnCount { get; }

        public long RowCount { get; }

        public bool CanSort { get; }

        public Task<IGridData<string>> GetAsync(GridRange gridRange, ISortOrder sortOrder = null) {
            var t = _session.GetGridDataAsync(_evaluation.Expression, gridRange, sortOrder);
            if (t == null) {
                // May happen when R host is not running
                Trace.Fail(Invariant($"{nameof(VariableViewModel)} returned null grid data"));
                return Task.FromResult<IGridData<string>>(null);
            }
            return t;
        }
    }
}
