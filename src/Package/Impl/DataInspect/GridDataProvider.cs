using System;
using System.Diagnostics;
using System.Threading.Tasks;
using static System.FormattableString;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    /// <summary>
    /// grid data provider to control
    /// </summary>
    internal class GridDataProvider : IGridProvider<string> {
        private EvaluationWrapper _evaluation;

        public GridDataProvider(EvaluationWrapper evaluation) {
            _evaluation = evaluation;

            RowCount = evaluation.Dimensions[0];
            ColumnCount = evaluation.Dimensions[1];
        }

        public int ColumnCount { get; }

        public int RowCount { get; }

        public Task<IGridData<string>> GetAsync(GridRange gridRange) {
            var t = _evaluation.GetGridDataAsync(_evaluation.Expression, gridRange);
            if (t == null) {
                Debug.Fail(Invariant($"{nameof(EvaluationWrapper)} returned null grid data"));
                return Task.FromResult<IGridData<string>>(null);
            }

            return t;
        }

        public Task<IGrid<string>> GetRangeAsync(GridRange gridRange) {
            throw new NotImplementedException();
        }
    }
}
