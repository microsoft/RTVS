// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.R.Package.Repl;
using Microsoft.VisualStudio.R.Package.Shell;
using static System.FormattableString;

namespace Microsoft.VisualStudio.R.Package.DataInspect.DataSource {
    public class GridDataSource {
        /// <summary>
        /// Evaluatoin R expression and return grid data
        /// </summary>
        /// <param name="expression">expression for R object</param>
        /// <param name="environment">R environment where R object is attached</param>
        /// <param name="gridRange">range of data to return</param>
        /// <param name="rSession">R session, nullable. If null, default interactive session is used</param>
        /// <returns></returns>
        public static async Task<IGridData<string>> GetGridDataAsync(string expression, string environment, GridRange gridRange, IRSession rSession = null) {
            await TaskUtilities.SwitchToBackgroundThread();

            if (rSession == null) {
                rSession = VsAppShell.Current.ExportProvider.GetExportedValue<IRSessionProvider>().GetInteractiveWindowRSession();
                if (rSession == null) {
                    throw new InvalidOperationException(Invariant($"{nameof(IRSessionProvider)} failed to return RSession for {nameof(EvaluationWrapper)}"));
                }
            }

            string rows = gridRange.Rows.ToRString();
            string columns = gridRange.Columns.ToRString();

            REvaluationResult? result = null;
            using (var evaluator = await rSession.BeginEvaluationAsync(false)) {
                result = await evaluator.EvaluateAsync($"rtvs:::grid.dput(rtvs:::grid.data({expression}, {environment ?? "NULL"}, {rows}, {columns}))", REvaluationKind.Normal);

                if (result.Value.ParseStatus != RParseStatus.OK || result.Value.Error != null) {
                    throw new InvalidOperationException($"Grid data evaluation failed:{result}");
                }
            }

            GridData data = null;

            if (result.HasValue) {
                data = GridParser.Parse(result.Value.StringResult.ToUnicodeQuotes());
                data.Range = gridRange;

                if ((data.ValidHeaderNames.HasFlag(GridData.HeaderNames.Row) && data.RowNames.Count != gridRange.Rows.Count)
                    || (data.ValidHeaderNames.HasFlag(GridData.HeaderNames.Column) && data.ColumnNames.Count != gridRange.Columns.Count)) {
                    throw new InvalidOperationException("Header names lengths are different from data's length");
                }
            }

            return data;
        }
    }
}
