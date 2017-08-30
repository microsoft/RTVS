// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Logging;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.R.Package.Shell;
using static System.FormattableString;

namespace Microsoft.VisualStudio.R.Package.DataInspect.DataSource {
    internal sealed class GridDataSource {
        private readonly IRSession _session;
        private string _expression;
        private string _rows;
        private string _columns;
        private string _rowSelector;
        private GridData _gridData;

        public GridDataSource(IRSession session) {
            _session = session;
        }

        public async Task<IGridData<string>> GetGridDataAsync(string expression, GridRange? gridRange, ISortOrder sortOrder = null) {
            await TaskUtilities.SwitchToBackgroundThread();

            var rows = gridRange?.Rows.ToRString();
            var columns = gridRange?.Columns.ToRString();
            var rowSelector = (sortOrder != null && !sortOrder.IsEmpty) ? sortOrder.GetRowSelector() : "";

            if (_gridData != null && _expression == expression && _rows == rows && _columns == columns && _rowSelector == rowSelector) {
                return _gridData;
            }

            var expr = Invariant($"rtvs:::grid_data({expression}, {rows}, {columns}, {rowSelector})");
            try {
                _gridData = await _session.EvaluateAsync<GridData>(expr, REvaluationKind.Normal);
                _expression = expression;
                _rows = rows;
                _columns = columns;
                _rowSelector = rowSelector;
            } catch (RException ex) {
                var message = Invariant($"Grid data evaluation failed:{Environment.NewLine}{ex.Message}");
                VsAppShell.Current.Log().Write(LogVerbosity.Normal, MessageCategory.Error, message);
            }
            return _gridData;
        }
    }
}
