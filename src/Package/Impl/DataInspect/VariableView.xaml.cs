using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Controls;
using Microsoft.R.Debugger;
using Microsoft.R.Support.Settings.Definitions;
using Microsoft.VisualStudio.R.Package.DataInspect.Definitions;
using Microsoft.VisualStudio.R.Package.Shell;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    public partial class VariableView : UserControl, IDisposable {
        private readonly IRToolsSettings _settings;
        ObservableTreeNode _rootNode;
        VariableSubscription _globalEnvSubscription;

        const string GlobalEnvironmentName = "Global Environment";

        public VariableView() : this(null) { }

        public VariableView(IRToolsSettings settings) {
            _settings = settings;

            InitializeComponent();

            SortDirection = ListSortDirection.Ascending;

            var variableProvider = VsAppShell.Current.ExportProvider.GetExportedValue<IVariableDataProvider>();

            SetRootNode(EvaluationWrapper.Ellipsis);

            _globalEnvSubscription = variableProvider.Subscribe(0, VariableProvider.GlobalEnvironmentExpression, OnGlobalEnvironmentEvaluation);

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
            var wrapper = new EvaluationWrapper(result);

            var rootNodeModel = new VariableNode(_settings, wrapper);

            VsAppShell.Current.DispatchOnUIThread(
                () => {
                    EnvironmentName.Text = GlobalEnvironmentName;
                    _rootNode.Model = rootNodeModel;
                });
        }

        private void SetRootNode(EvaluationWrapper evaluation) {
            _rootNode = new ObservableTreeNode(
                new VariableNode(_settings, evaluation),
                Comparer<ITreeNode>.Create(Comparison));

            RootTreeGrid.ItemsSource = new TreeNodeCollection(_rootNode).ItemList;
        }

        private ListSortDirection SortDirection { get; set; }

        private int Comparison(ITreeNode left, ITreeNode right) {
            return VariableNode.Comparison((VariableNode)left, (VariableNode)right, SortDirection);
        }
    }
}
