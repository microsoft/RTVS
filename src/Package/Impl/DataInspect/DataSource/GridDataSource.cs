// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Logging;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Session;
using Microsoft.VisualStudio.R.Package.Shell;
using static System.FormattableString;

namespace Microsoft.VisualStudio.R.Package.DataInspect.DataSource {
    internal sealed class GridDataSource {
        public static async Task<IGridData<string>> GetGridDataAsync(string expression, GridRange? gridRange, ISortOrder sortOrder = null) {
            await TaskUtilities.SwitchToBackgroundThread();
            var rSession = VsAppShell.Current.ExportProvider.GetExportedValue<IRSessionProvider>().GetInteractiveWindowRSession();

            string rows = gridRange?.Rows.ToRString();
            string columns = gridRange?.Columns.ToRString();

            using (var evaluator = await rSession.BeginEvaluationAsync()) {
                try {
                    string exp;
                    if (sortOrder != null && !sortOrder.IsEmpty) {
                        if (gridRange.Value.Columns.Count > 1) {
                            string dataFrameSortExpression = sortOrder.GetDataFrameSortFunction();
                            exp = Invariant($"rtvs:::grid_data({expression}, {rows}, {columns}, {dataFrameSortExpression.ToRStringLiteral()})");
                        } else {
                            int sortType = sortOrder.IsPrimaryDescending ? 2 : 1;
                            exp = Invariant($"rtvs:::grid_data({expression}, {rows}, {columns}, NULL, {sortType})");
                        }
                    } else {
                        exp = Invariant($"rtvs:::grid_data({expression}, {rows}, {columns})");
                    }
                    return await evaluator.EvaluateAsync<GridData>(exp, REvaluationKind.Normal);
                } catch (RException ex) {
                    var message = Invariant($"Grid data evaluation failed:{Environment.NewLine}{ex.Message}");
                    GeneralLog.Write(message);
                    return null;
                }
            }
        }
    }
}
