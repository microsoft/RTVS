//#define PRINTTHUMB
using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;
using ThreadHelper = Microsoft.VisualStudio.Shell.ThreadHelper;

namespace Microsoft.VisualStudio.R.Package.DataInspect
{
    public partial class VariableView : UserControl
    {
        private Variable _globalEnv;

        public VariableView()
        {
            InitializeComponent();

            VariableProvider.Current.VariableChanged += VariableProvider_VariableChanged;

            Loaded += VariableView_Loaded;
            Unloaded += VariableView_Unloaded;
            SizeChanged += VariableView_SizeChanged;
        }

        private void VariableView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ResetWidth();
        }

        private void VariableView_Loaded(object sender, RoutedEventArgs e)
        {
            RegisterThumbEvents();

            ResetWidth();
        }

        private void VariableView_Unloaded(object sender, RoutedEventArgs e)
        {
            UnregisterThumbEvents();
        }

        private void VariableProvider_VariableChanged(object sender, VariableChangedArgs e)
        {
            if (e.NewVariable != null
                && e.NewVariable.Name == VariableProvider.Current.GlobalEnvContext.VariableName)
            {
                var newVariable = Variable.Create(null, e.NewVariable, VariableProvider.Current.GlobalEnvContext,
                    new VariableVisualInfo() {
                        IndentStep = -1,  // indent -1, as childrent is root level.
                        NameWidth = NameColumn.ActualWidth,
                        ValueWidth = ValueColumn.ActualWidth,
                        TypeWidth = TypeColumn.ActualWidth });

                ThreadHelper.Generic.BeginInvoke(
                    DispatcherPriority.Normal,
                    () =>
                {
                    if (_globalEnv == null)
                    {
                        SetVariable(newVariable);
                    }
                    else
                    {
                        _globalEnv.Update(newVariable);
                    }
                    });
            }
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
                EnvironmentTextBlock.Text = _globalEnv.VariableValue;

                _globalEnv.Children.CollectionChanged += VariableChildren_CollectionChanged;
                foreach (var child in _globalEnv.Children)
                {
                    RootTreeView.Items.Add(child);
                }
            }
        }

        private int itemIndexOffset = 0;
        private void VariableChildren_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        int index = e.NewStartingIndex + itemIndexOffset; // offset to Column header
                        foreach (var child in e.NewItems)
                        {
                            RootTreeView.Items.Insert(index++, child);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    {
                        int index = e.OldStartingIndex + itemIndexOffset;
                        foreach (var child in e.OldItems)
                        {
                            RootTreeView.Items.RemoveAt(index);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    {
                        while (RootTreeView.Items.Count > itemIndexOffset)
                        {
                            RootTreeView.Items.RemoveAt(RootTreeView.Items.Count - itemIndexOffset);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                case NotifyCollectionChangedAction.Move:
                default:
                    throw new NotSupportedException();
            }
        }

        #region Column Resizing

        private void RegisterThumbEvents()
        {
            UnregisterThumbEvents();

            LeftThumb.DragStarted += Thumb_DragStarted;
            LeftThumb.DragDelta += Thumb_DragDelta;
            LeftThumb.DragCompleted += Thumb_DragCompleted;
            LeftThumb.MouseDoubleClick += Thumb_MouseDoubleClick;

            RightThumb.DragStarted += Thumb_DragStarted;
            RightThumb.DragDelta += Thumb_DragDelta;
            RightThumb.DragCompleted += Thumb_DragCompleted;
            RightThumb.MouseDoubleClick += Thumb_MouseDoubleClick;
        }

        private void UnregisterThumbEvents()
        {
            LeftThumb.DragStarted -= Thumb_DragStarted;
            LeftThumb.DragDelta -= Thumb_DragDelta;
            LeftThumb.DragCompleted -= Thumb_DragCompleted;
            LeftThumb.MouseDoubleClick -= Thumb_MouseDoubleClick;

            RightThumb.DragStarted -= Thumb_DragStarted;
            RightThumb.DragDelta -= Thumb_DragDelta;
            RightThumb.DragCompleted -= Thumb_DragCompleted;
            RightThumb.MouseDoubleClick -= Thumb_MouseDoubleClick;
        }

        private double _nameWidth;
        private double _typeWidth;
        private void Thumb_DragStarted(object sender, DragStartedEventArgs e)
        {
            _nameWidth = NameColumn.ActualWidth;
            _typeWidth = TypeColumn.ActualWidth;
        }

        private void Thumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            bool reset = false;
            if (sender == LeftThumb)
            {
                var newNameWidth = _nameWidth + e.HorizontalChange;
#if PRINTTHUMB
                Debug.WriteLine("LeftThumb:{0} {1} {2} {3}", HeaderRowGrid.ActualWidth, _nameWidth, _typeWidth, e.HorizontalChange);
#endif
                if (newNameWidth > 5
                    && (newNameWidth + _typeWidth + 10) < (HeaderRowGrid.ActualWidth))
                {
                    _nameWidth = newNameWidth;
                    NameColumn.Width = newNameWidth;
                    reset = true;
                }
            }
            else if (sender == RightThumb)
            {
                var newTypeWidth = _typeWidth - e.HorizontalChange;
#if PRINTTHUMB
                Debug.WriteLine("RightThumb:{0} {1} {2} {3}", HeaderRowGrid.ActualWidth, _nameWidth, _typeWidth, e.HorizontalChange);
#endif
                if (newTypeWidth > 5
                    && (_nameWidth + newTypeWidth + 10) < (HeaderRowGrid.ActualWidth))
                {
                    _typeWidth = newTypeWidth;
                    TypeColumn.Width = newTypeWidth;
                    reset = true;
                }
            }

            if (reset)
            {
                ResetWidth();
            }

            e.Handled = true;
        }

        private void Thumb_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            _nameWidth = 0;
            _typeWidth = 0;
        }

        private void Thumb_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
        }

        private void ResetWidth()
        {
            if (_globalEnv != null)
            {
                SetWidth(_globalEnv);
            }
        }

        // Depth first tree traverse
        private void SetWidth(Variable v)
        {
            v.NameWidth = NameColumn.ActualWidth;
            v.ValueWidth = ValueColumn.ActualWidth;
            v.TypeWidth = TypeColumn.ActualWidth;

            if (v.Children != null)
            {
                foreach (var child in v.Children)
                {
                    SetWidth(child);
                }
            }
        }

#endregion
    }
}
