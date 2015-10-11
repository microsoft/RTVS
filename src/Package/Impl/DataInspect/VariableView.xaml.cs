using System;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;

namespace Microsoft.VisualStudio.R.Package.DataInspect
{
    public partial class VariableView : UserControl
    {
        private readonly VariableProvider _variableProvider;
        private Variable _globalEnv;

        public VariableView()
        {
            InitializeComponent();

            _variableProvider = new VariableProvider();
            _variableProvider.SessionsChanged += VariableProvider_SessionsChanged;

            InitializeData();
        }

        private void VariableProvider_SessionsChanged(object sender, EventArgs e)
        {
            InitializeData();
        }

        public void InitializeData()
        {
            _globalEnv = _variableProvider.GetGlobalEnv(new VariableProvideContext());
            SetVariable(_globalEnv);
        }

        private void SetVariable(Variable variable)
        {
            if (_globalEnv != null)
            {
                _globalEnv.Children.CollectionChanged -= VariableChildren_CollectionChanged;
            }

            _globalEnv = variable;
            if (_globalEnv != null)
            {
                _globalEnv.Children.CollectionChanged += VariableChildren_CollectionChanged;
                foreach (var child in _globalEnv.Children)
                {
                    RootGrid.Items.Add(child);
                }
            }
        }

        private int itemIndexOffset = 1;
        private void VariableChildren_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        int index = e.NewStartingIndex + itemIndexOffset; // offset to Column header
                        foreach (var child in e.NewItems)
                        {
                            RootGrid.Items.Insert(index++, child);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    {
                        int index = e.OldStartingIndex + itemIndexOffset;
                        foreach (var child in e.OldItems)
                        {
                            RootGrid.Items.Insert(index++, child);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    {
                        while (RootGrid.Items.Count > itemIndexOffset)
                        {
                            RootGrid.Items.RemoveAt(RootGrid.Items.Count - itemIndexOffset);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                case NotifyCollectionChangedAction.Move:
                default:
                    throw new NotSupportedException();
            }
        }
    }
}
