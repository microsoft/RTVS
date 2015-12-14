using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using ThreadHelper = Microsoft.VisualStudio.Shell.ThreadHelper;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    public partial class VariableView : UserControl {
        ObservableTreeNode _rootNode;

        public VariableView() {
            InitializeComponent();

            if (VariableProvider.Current.LastEvaluation == null) {
                SetRootNode(EvaluationWrapper.Ellipsis);
            } else {
                SetRootNode(VariableProvider.Current.LastEvaluation);
                EnvironmentName.Text = VariableProvider.Current.LastEvaluation.Name;
            }
            VariableProvider.Current.VariableChanged += VariableProvider_VariableChanged;
        }

        private void VariableProvider_VariableChanged(object sender, VariableChangedArgs e) {
            VariableChanged(e.NewVariable);
        }

        private void VariableChanged(EvaluationWrapper variable) {
            ThreadHelper.Generic.BeginInvoke(
                DispatcherPriority.Normal,
                () => {
                    EnvironmentName.Text = variable.Name;
                    _rootNode.Model = new VariableNode(variable);
                });
        }

        private void SetRootNode(EvaluationWrapper evaluation) {
            _rootNode = new ObservableTreeNode(new VariableNode(evaluation));
            _rootNode.IsExpanded = true;
            RootTreeGrid.ItemsSource = new TreeNodeCollection(_rootNode).ItemList;
        }
    }
}
