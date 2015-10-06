using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Microsoft.VisualStudio.R.Controls
{
    public partial class VariableView : UserControl
    {
        private readonly VariableProvider _variableProvider;
        private readonly ObservableCollection<Variable> _variables; // variables in flat structure

        public VariableView()
        {
            InitializeComponent();
            
            _variables = new ObservableCollection<Variable>();

            ViewSource = new CollectionViewSource();
            ViewSource.Source = _variables;
            ViewSource.Filter += CollectionViewSource_Filter;

            RootGrid.ItemsSource = ViewSource.View;

            _variableProvider = new VariableProvider(this);
            _variableProvider.SessionsChanged += VariableProvider_SessionsChanged;

            RefreshData();
        }

        private void VariableProvider_SessionsChanged(object sender, EventArgs e)
        {
            RefreshData();
        }

        public CollectionViewSource ViewSource { get; }

        public void RefreshView()
        {
            ViewSource.View.Refresh();
        }

        public void AddRange(IEnumerable<Variable> variables, bool clearAll = true)
        {
            if (clearAll)
            {
                _variables.Clear();
            }

            Variable.TraverseDepthFirst(variables,
                (v) => { _variables.Add(v); v.View = this; return true; });

            // set top level visible
            foreach (var v in variables)
            {
                v.IsVisible = true;
            }

            RefreshView();
        }

        public void RefreshData()
        {
            var newVariables = _variableProvider.Get(new VariableProvideContext());

            DispatchInvoke(
                () => AddRange(newVariables),
                DispatcherPriority.Normal);
        }

        private void CollectionViewSource_Filter(object sender, FilterEventArgs e)
        {
            var variable = e.Item as Variable;
            if (variable != null)
            {
                e.Accepted = variable.IsVisible;
            }
        }

        private static void DispatchInvoke(Action toInvoke, DispatcherPriority priority)
        {
            Action guardedAction =
                () => {
                    try
                    {
                        toInvoke();
                    }
                    catch
                    {
                        Debug.Assert(false, "Guarded invoke caught exception");
                    }
                };

            Application.Current.Dispatcher.BeginInvoke(guardedAction, priority);    // TODO: acquiring Application.Current.Dispatcher, create utility class for UI thread and use it
        }
    }
}
