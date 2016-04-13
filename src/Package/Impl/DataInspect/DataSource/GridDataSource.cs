// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.R.Package.Repl;
using Microsoft.VisualStudio.R.Package.Shell;
using Newtonsoft.Json;
using static System.FormattableString;

namespace Microsoft.VisualStudio.R.Package.DataInspect.DataSource {
    public class GridDataSource {
        public static async Task<IGridData<string>> GetGridDataAsync(string expression, GridRange? gridRange, IRSession rSession = null) {
            await TaskUtilities.SwitchToBackgroundThread();

            if (rSession == null) {
                rSession = VsAppShell.Current.ExportProvider.GetExportedValue<IRSessionProvider>().GetInteractiveWindowRSession();
                if (rSession == null) {
                    throw new InvalidOperationException(Invariant($"{nameof(IRSessionProvider)} failed to return RSession for {nameof(EvaluationWrapper)}"));
                }
            }

            string rows = gridRange?.Rows.ToRString();
            string columns = gridRange?.Columns.ToRString();

            REvaluationResult? result = null;
            using (var evaluator = await rSession.BeginEvaluationAsync()) {
                result = await evaluator.EvaluateAsync($"rtvs:::toJSON(rtvs:::grid.data({expression}, {rows}, {columns}))", REvaluationKind.Normal);

                if (!result.HasValue || result.Value.ParseStatus != RParseStatus.OK || result.Value.Error != null) {
                    throw new InvalidOperationException($"Grid data evaluation failed{Environment.NewLine} {result}");
                }
                return JsonConvert.DeserializeObject<GridData>(result.Value.StringResult.ConvertCharacterCodes());
            }
        }
    }
}
