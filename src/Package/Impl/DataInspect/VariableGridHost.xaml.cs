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

            Loaded += VariableGridHost_Loaded;
            Unloaded += VariableGridHost_Unloaded;
        }
        
        internal void SetEvaluation(EvaluationWrapper evaluation) {
            VariableGrid.Initialize(new DataProvider(evaluation));
        }

        private void VariableGridHost_Loaded(object sender, System.Windows.RoutedEventArgs e) {
            VariableProvider.Current.VariableChanged += VariableProvider_VariableChanged;
        }

        private void VariableGridHost_Unloaded(object sender, System.Windows.RoutedEventArgs e) {
            VariableProvider.Current.VariableChanged -= VariableProvider_VariableChanged;
        }

        private void VariableProvider_VariableChanged(object sender, VariableChangedArgs e) {
            VariableGrid.Refresh();
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

        public Task<IGridData<string>> GetAsync(GridRange gridRange) {
            return VariableProvider.Current.GetGridDataAsync(_evaluation.Expression, gridRange);
        }

        public Task<IGrid<string>> GetRangeAsync(GridRange gridRange) {
            throw new NotImplementedException();
        }
    }
}
