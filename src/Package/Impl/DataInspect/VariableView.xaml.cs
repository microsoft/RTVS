using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Controls;
using Microsoft.R.Debugger;
using Microsoft.VisualStudio.R.Package.Shell;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    public partial class VariableView : UserControl, IDisposable {
        ObservableTreeNode _rootNode;
        VariableSubscription _globalEnvSubscription;

        const string GlobalEnvironmentName = "Global Environment";

        public VariableView() {
            InitializeComponent();

            SortDirection = ListSortDirection.Ascending;

            if (VariableProvider.Current.GlobalEnvironment == null) {
                SetRootNode(EvaluationWrapper.Ellipsis);
            } else {
                SetRootNode(VariableProvider.Current.GlobalEnvironment);
                EnvironmentName.Text = GlobalEnvironmentName;
            }

            _globalEnvSubscription = VariableProvider.Current.Subscribe(0, VariableProvider.GlobalEnvironmentExpression, OnGlobalEnvironmentEvaluation);

            RootTreeGrid.Sorting += RootTreeGrid_Sorting;
        }

        public void Dispose() {
            if (_globalEnvSubscription != null) {
                _globalEnvSubscription.Dispose();
                _globalEnvSubscription = null;
            }

            RootTreeGrid.Sorting -= RootTreeGrid_Sorting;
        }

        private void RootTreeGrid_Sorting(object sender, DataGridSortingEventArgs e) {
            // SortDirection
            if (SortDirection == ListSortDirection.Ascending) {
                SortDirection = ListSortDirection.Descending;
            } else {
                SortDirection = ListSortDirection.Ascending;
            }

            _rootNode.Sort();
            e.Handled = true;
        }

        private void OnGlobalEnvironmentEvaluation(DebugEvaluationResult result) {
            var wrapper = new EvaluationWrapper(-1, result, false);

            var rootNodeModel = new VariableNode(wrapper);

            VsAppShell.Current.DispatchOnUIThread(
                () => {
                    EnvironmentName.Text = GlobalEnvironmentName;
                    _rootNode.Model = rootNodeModel;
                });
        }

        private void SetRootNode(EvaluationWrapper evaluation) {
            _rootNode = new ObservableTreeNode(
                new VariableNode(evaluation),
                Comparer<ITreeNode>.Create(Comparison));

            RootTreeGrid.ItemsSource = new TreeNodeCollection(_rootNode).ItemList;
        }

        private ListSortDirection SortDirection { get; set; }

        private int Comparison(ITreeNode left, ITreeNode right) {
            return VariableNode.Comparison((VariableNode)left, (VariableNode)right, SortDirection);
        }
    }
}
