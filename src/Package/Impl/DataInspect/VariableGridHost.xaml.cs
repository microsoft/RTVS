using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.R.Debugger;
using Microsoft.VisualStudio.R.Package.Shell;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    public partial class VariableGridHost : UserControl {
        private EvaluationWrapper _evaluation;
        private VariableSubscription _subscription;

        public VariableGridHost() {
            InitializeComponent();
        }
        
        internal void SetEvaluation(EvaluationWrapper evaluation) {
            VariableGrid.Initialize(new DataProvider(evaluation));

            _evaluation = evaluation;

            if (_subscription != null) {
                VariableProvider.Current.Unsubscribe(_subscription);
                _subscription = null;
            }

            _subscription = VariableProvider.Current.Subscribe(
                evaluation.FrameIndex,
                evaluation.Expression,
                SubscribeAction);
        }

        private void SubscribeAction(DebugEvaluationResult evaluation) {
            VsAppShell.Current.DispatchOnUIThread(
                () => {
                    if (evaluation is DebugErrorEvaluationResult) {
                        var error = (DebugErrorEvaluationResult)evaluation;
                        SetError(error.ErrorText);
                        return;
                    }

                    var wrapper = new EvaluationWrapper(-1, evaluation, true);

                    if (wrapper.Dimensions.Count != 2) {
                        // the same evaluation changed to non-matrix
                        SetError($"object '{evaluation.Expression}' is not two dimensional.");
                    } else if (wrapper.Dimensions[0] != _evaluation.Dimensions[0]
                        || wrapper.Dimensions[1] != _evaluation.Dimensions[1]) {
                        ClearError();

                        // matrix size changed. Reset the evaluation
                        SetEvaluation(wrapper);
                    } else {
                        ClearError();
                        
                        // size stays same. Refresh
                        VariableGrid.Refresh();
                    }
                });
        }

        private void SetError(string text) {
            ErrorTextBlock.Text = text;
            ErrorTextBlock.Visibility = Visibility.Visible;

            VariableGrid.Visibility = Visibility.Collapsed;
        }

        private void ClearError() {
            ErrorTextBlock.Visibility = Visibility.Collapsed;

            VariableGrid.Visibility = Visibility.Visible;
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
