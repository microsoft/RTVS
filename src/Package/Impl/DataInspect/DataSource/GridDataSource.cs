using System;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.R.Package.Repl;
using Microsoft.VisualStudio.R.Package.Shell;
using static System.FormattableString;

namespace Microsoft.VisualStudio.R.Package.DataInspect.DataSource {
    public class GridDataSource {
        public static async Task<IGridData<string>> GetGridDataAsync(string expression, GridRange gridRange, IRSession rSession = null) {
            await TaskUtilities.SwitchToBackgroundThread();

            if (rSession == null) {
                rSession = VsAppShell.Current.ExportProvider.GetExportedValue<IRSessionProvider>().GetInteractiveWindowRSession();
                if (rSession == null) {
                    throw new InvalidOperationException(Invariant($"{nameof(IRSessionProvider)} failed to return RSession for {nameof(EvaluationWrapper)}"));
                }
            }

            string rows = gridRange.Rows.ToRString();
            string columns = gridRange.Columns.ToRString();

            using (var evaluator = await rSession.BeginEvaluationAsync(false)) {
                var result = await evaluator.EvaluateAsync($"rtvs:::grid.dput(rtvs:::grid.data({expression}, {rows}, {columns}))", REvaluationKind.Normal);

                if (result.ParseStatus != RParseStatus.OK || result.Error != null) {
                    throw new InvalidOperationException($"Grid data evaluation failed:{result}");
                }

                GridData data = GridParser.Parse(result.StringResult);
                data.Range = gridRange;

                if ((data.ValidHeaderNames.HasFlag(GridData.HeaderNames.Row) && data.RowNames.Count != gridRange.Rows.Count)
                    || (data.ValidHeaderNames.HasFlag(GridData.HeaderNames.Column) && data.ColumnNames.Count != gridRange.Columns.Count)) {
                    throw new InvalidOperationException("Header names lengths are different from data's length");
                }

                return data;
            }
        }
    }
}
