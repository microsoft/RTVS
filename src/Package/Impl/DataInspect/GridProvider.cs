using System;
using System.Threading.Tasks;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    /// <summary>
    /// grid data provider to control
    /// </summary>
    internal class GridProvider : IGridProvider<string> {
        private EvaluationWrapper _evaluation;

        public GridProvider(EvaluationWrapper evaluation) {
            _evaluation = evaluation;

            RowCount = evaluation.Dimensions[0];
            ColumnCount = evaluation.Dimensions[1];
        }

        public int ColumnCount { get; }

        public int RowCount { get; }

        public Task<IGridData<string>> GetAsync(GridRange gridRange) {
            return VariableProvider.Current.GetGridDataAsync(_evaluation.Expression, gridRange);
        }

        public Task<IGrid<string>> GetRangeAsync(GridRange gridRange) {
            throw new NotImplementedException();
        }
    }
}
