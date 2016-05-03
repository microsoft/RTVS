// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.R.Actions.Logging;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Session;
using Microsoft.VisualStudio.R.Package.Shell;
using static System.FormattableString;

namespace Microsoft.VisualStudio.R.Package.DataInspect.DataSource {
    public class GridDataSource {
        public static async Task<IGridData<string>> GetGridDataAsync(string expression, GridRange? gridRange) {
            await TaskUtilities.SwitchToBackgroundThread();
            var rSession = VsAppShell.Current.ExportProvider.GetExportedValue<IRSessionProvider>().GetInteractiveWindowRSession();
 
            string rows = gridRange?.Rows.ToRString();
            string columns = gridRange?.Columns.ToRString();

            REvaluationResult? result = null;
            using (var evaluator = await rSession.BeginEvaluationAsync()) {
                result = await evaluator.EvaluateAsync($"rtvs:::grid.data({expression}, {rows}, {columns})", REvaluationKind.Json);

                if (!result.HasValue || result.Value.ParseStatus != RParseStatus.OK || result.Value.Error != null) {
                    // No reason to crash, just log it
                    var message = Invariant($"Grid data evaluation failed{Environment.NewLine} {result}");
                    GeneralLog.Write(message);
                    return null;
                }
                return result.Value.JsonResult.ToObject<GridData>();
            }
        }
    }
}
