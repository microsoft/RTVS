using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.Common.Core;
using ThreadHelper = Microsoft.VisualStudio.Shell.ThreadHelper;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    public partial class VariableView : UserControl {
        ObservableTreeNode _rootNode;

        public VariableView() {
            InitializeComponent();

            VariableProvider.Current.VariableChanged += VariableProvider_VariableChanged;
            if (VariableProvider.Current.LastEvaluation != null) {
                SetRootNode(VariableProvider.Current.LastEvaluation);
            }

            Loaded += VariableView_Loaded;
            Unloaded += VariableView_Unloaded;
            SizeChanged += VariableView_SizeChanged;
        }

        private void VariableView_SizeChanged(object sender, SizeChangedEventArgs e) {
        }

        private void VariableView_Loaded(object sender, RoutedEventArgs e) {
        }

        private void VariableView_Unloaded(object sender, RoutedEventArgs e) {
        }

        private void VariableProvider_VariableChanged(object sender, VariableChangedArgs e) {
            VariableChanged(e.NewVariable);
        }

        private void VariableChanged(EvaluationWrapper variable) {
            if (_rootNode == null) {
                ThreadHelper.Generic.BeginInvoke(
                    DispatcherPriority.Normal,
                    () => SetRootNode(variable));
            }
            else {
                _rootNode.Model = new VariableNode(variable);
            }
        }

        private void SetRootNode(EvaluationWrapper evaluation) {
            _rootNode = ObservableTreeNode.CreateAsRoot(new VariableNode(evaluation), false);
            var items = new TreeNodeCollection(_rootNode);
            RootTreeGrid.ItemsSource = items.View;

            EnvironmentName.Text = evaluation.Name;
        }
    }
}
