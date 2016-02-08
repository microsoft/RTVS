using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Controls;
using Microsoft.R.Editor.Data;
using Microsoft.VisualStudio.R.Package.Shell;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    public partial class VariableView : UserControl, IDisposable {
        ObservableTreeNode _rootNode;

        public VariableView() {
            InitializeComponent();

            SortDirection = ListSortDirection.Ascending;

            if (VariableProvider.Current.LastEvaluation == null) {
                SetRootNode(EvaluationWrapper.Ellipsis);
            } else {
                SetRootNode(VariableProvider.Current.LastEvaluation);
                EnvironmentName.Text = VariableProvider.Current.LastEvaluation.Name;
            }
            VariableProvider.Current.VariableChanged += VariableProvider_VariableChanged;

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

        private void SetRootNode(IRSessionDataObject evaluation) {
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
