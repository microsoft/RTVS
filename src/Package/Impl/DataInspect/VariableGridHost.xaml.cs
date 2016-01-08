using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Controls;
using Microsoft.Common.Core;
using Newtonsoft.Json.Linq;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    public partial class VariableGridHost : UserControl {
        public VariableGridHost() {
            InitializeComponent();
        }

        internal void SetEvaluation(EvaluationWrapper evaluation) {
            VariableGrid.Initialize(new DataProvider(evaluation));
        }
    }

    internal class DataProvider : IGridProvider<string> {
        private EvaluationWrapper _evaluation;

        public DataProvider(EvaluationWrapper evaluation) {
            _evaluation = evaluation;

            RowCount = evaluation.Dimensions[0];
            ColumnCount = evaluation.Dimensions[1];
        }

        public int ColumnCount { get; }

        public int RowCount { get; }

        public async Task<IGridData<string>> GetAsync(GridRange gridRange) {
            await TaskUtilities.SwitchToBackgroundThread();

            string rows = RangeToRString(gridRange.Rows);
            string cols = RangeToRString(gridRange.Columns);

            var result = await VariableProvider.Current.EvaluateGridDataAsync(_evaluation.Name, rows, cols);

            var data = GridParser.Parse(result);
            data.Range = gridRange;

            if (data.ColumnNames.Count != gridRange.Columns.Count
                || data.RowNames.Count != gridRange.Rows.Count) {
                throw new InvalidOperationException("The number of evaluatoin data doesn't match with what is requested");
            }

            return data;
        }

        public Task<IGrid<string>> GetRangeAsync(GridRange gridRange) {
            throw new NotImplementedException();
        }

        private static string RangeToRString(Range range) {
            return $"{range.Start + 1}:{range.Start + range.Count}";
        }
    }
}
