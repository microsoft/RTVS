// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.R.Actions.Logging;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Session;
using Microsoft.VisualStudio.R.Package.Shell;
using static System.FormattableString;

namespace Microsoft.VisualStudio.R.Package.DataInspect.DataSource {
    public class GridDataSource {
        public static async Task<IGridData<string>> GetGridDataAsync(string expression, GridRange? gridRange, IEnumerable<string> sortOrder = null) {
            await TaskUtilities.SwitchToBackgroundThread();
            var rSession = VsAppShell.Current.ExportProvider.GetExportedValue<IRSessionProvider>().GetInteractiveWindowRSession();

            string rows = gridRange?.Rows.ToRString();
            string columns = gridRange?.Columns.ToRString();

            using (var evaluator = await rSession.BeginEvaluationAsync()) {
                try {
                    string exp;
                    if (sortOrder != null) {
                        var sb = new StringBuilder("c(");
                        bool first = true;
                        foreach (var s in sortOrder) {
                            if (!first) {
                                sb.Append(',');
                            } else {
                                first = false;
                            }
                            sb.Append(Invariant($"'{s}'"));
                        }
                        sb.Append(')');

                        exp = Invariant($"rtvs:::grid.data({expression}, {rows}, {columns}, {sb.ToString()})");
                    } else {
                        exp = Invariant($"rtvs:::grid.data({expression}, {rows}, {columns})");
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
