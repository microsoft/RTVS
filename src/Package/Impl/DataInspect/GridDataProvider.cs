// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.VisualStudio.R.Package.DataInspect.DataSource;
using static System.FormattableString;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    /// <summary>
    /// grid data provider to control
    /// </summary>
    internal class GridDataProvider : IGridProvider<string> {
        private readonly EvaluationWrapper _evaluation;

        public GridDataProvider(EvaluationWrapper evaluation) {
            _evaluation = evaluation;

            RowCount = evaluation.Dimensions[0];
            ColumnCount = evaluation.Dimensions[1];
        }

        public int ColumnCount { get; }

        public int RowCount { get; }

        public Task<IGridData<string>> GetAsync(GridRange gridRange) {
            var t = GridDataSource.GetGridDataAsync(_evaluation.Expression, _evaluation.Environment, gridRange);
            if (t == null) {
                // May happen when R host is not running
                Trace.Fail(Invariant($"{nameof(EvaluationWrapper)} returned null grid data"));
                return Task.FromResult<IGridData<string>>(null);
            }
            return t;
        }
    }
}
