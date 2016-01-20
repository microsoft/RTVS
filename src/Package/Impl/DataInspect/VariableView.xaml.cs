using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Controls;
using Microsoft.R.Debugger;
using Microsoft.R.Editor.Data;
using Microsoft.VisualStudio.R.Package.Shell;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    public partial class VariableView : UserControl, IDisposable {
        ObservableTreeNode _rootNode;

        public VariableView() {
            InitializeComponent();

            SortDirection = ListSortDirection.Ascending;

            if (VariableProvider.Current.GlobalEnvironment == null) {
                SetRootNode(EvaluationWrapper.Ellipsis);
            } else {
                SetRootNode(VariableProvider.Current.GlobalEnvironment);
                EnvironmentName.Text = VariableProvider.Current.GlobalEnvironment.Name;
            }
            //VariableProvider.Current.VariableChanged += VariableProvider_VariableChanged;

            VariableProvider.Current.Subscribe(".GlobalEnv", "environment()", SubscribeGlobalEnvironment);

            RootTreeGrid.Sorting += RootTreeGrid_Sorting;
        }

        public void Dispose() {
            // Used in tests only
            VariableProvider.Current.VariableChanged -= VariableProvider_VariableChanged;
            RootTreeGrid.Sorting -= RootTreeGrid_Sorting;
            VariableProvider.Current.Dispose();
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

        void SubscribeGlobalEnvironment(DebugEvaluationResult result) {
            var wrapper = new EvaluationWrapper(-1, result, false);
            var rootNodeModel = new VariableNode(wrapper);
            VsAppShell.Current.DispatchOnUIThread(
                () => {
                    EnvironmentName.Text = wrapper.Name;
                    _rootNode.Model = rootNodeModel;
                });
        }

        private void VariableProvider_VariableChanged(object sender, VariableChangedArgs e) {
            VariableChanged(e.NewVariable);
        }

        private void VariableChanged(EvaluationWrapper variable) {
            VsAppShell.Current.DispatchOnUIThread(
                () => {
                    EnvironmentName.Text = variable.Name;
                    _rootNode.Model = new VariableNode(variable);
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
