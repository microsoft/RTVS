using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Microsoft.VisualStudio.R.Package.DataInspect
{
    internal class Variable : INotifyPropertyChanged  // TODO: BindableBase
    {
        private Variable(VariableEvaluationContext evaluationContext)
        {
            Children = new ObservableCollection<Variable>();
            EvaluationContext = evaluationContext;
        }

        public VariableEvaluationContext EvaluationContext { get; }

        public event PropertyChangedEventHandler PropertyChanged;


        /// <summary>
        /// Create Variable instance from R evaluation
        /// </summary>
        /// <param name="evaluation"></param>
        /// <returns></returns>
        public static Variable Create(REvaluation evaluation, VariableEvaluationContext evaluationContext)
        {
            const string DataFramePrefix = "'data.frame':";
            var instance = new Variable(evaluationContext);

            // Name
            instance.VariableName = evaluation.Name;

            // Type
            instance.TypeName = evaluation.ClassName;
            if (instance.TypeName == "ordered factor")
            {
                instance.TypeName = "ordered";
            }

            // Value
            string variableValue = evaluation.Value.Trim();
            if (evaluation.Value.Trim().StartsWith(DataFramePrefix))
            {
                instance.VariableValue = variableValue.Substring(DataFramePrefix.Length).Trim();
            }
            else
            {
                instance.VariableValue = variableValue;
            }

            if ((instance.TypeName == "data.frame")
                || (instance.TypeName == "matrix")
                || (instance.TypeName == "environment"))
            {
                instance.HasChildren = true;
            }

            if (evaluation.Length > 1)
            {
                instance.HasChildren = true;
            }

            if (evaluation.Children != null)
            {
                foreach (var child in evaluation.Children.Evals)
                {
                    instance.Children.Add(Variable.Create(child, evaluationContext));
                }
            }

            return instance;
        }


        private string _variableName;
        /// <summary>
        /// Name of variable
        /// </summary>
        public string VariableName
        {
            get { return _variableName; }
            set
            {
                if (_variableName != value)
                {
                    _variableName = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("VariableName"));
                    }
                }
            }
        }

        private string _variableValue;
        /// <summary>
        /// Value of variable, represented in short string
        /// </summary>
        public string VariableValue
        {
            get { return _variableValue; }
            set
            {
                if (_variableValue != value)
                {
                    _variableValue = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("VariableValue"));
                    }
                }
            }
        }

        private string _typeName;
        /// <summary>
        /// Type name of variable
        /// </summary>
        public string TypeName
        {
            get { return _typeName; }
            set
            {
                if (_typeName != value)
                {
                    _typeName = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("TypeName"));
                    }
                }
            }
        }

        public bool HasChildren { get; private set; }

        public ObservableCollection<Variable> Children { get; }

        public void Update(Variable update)
        {
            DispatchInvoke(() => UpdateInternal(update), DispatcherPriority.Normal);
        }

        private void UpdateInternal(Variable update)    // TODO: optimize the iteration
        {
            if (VariableName != update.VariableName)
            {
                throw new InvalidOperationException("Can't update to different variable");
            }

            VariableValue = update.VariableValue;
            TypeName = update.TypeName;
            HasChildren = update.HasChildren;

            // remove
            var removed = (from v in Children
                          where !update.Children.Any((u) => (v.VariableName == u.VariableName))
                          select v).ToList();
            var nonRemoved = (from v in update.Children
                           where !removed.Any((u) => (v.VariableName == u.VariableName))
                           select v).ToList();

            foreach (var item in removed)
            {
                Children.Remove(item);
            }

            List<Variable> newVariables  = new List<Variable>();
            foreach (var newitem in nonRemoved)
            {
                var old = Children.FirstOrDefault((u) => (u.VariableName == newitem.VariableName));
                if (old == null)
                {
                    newVariables.Add(newitem);
                }
                else
                {
                    old.UpdateInternal(newitem);
                }
            }

            foreach (var item in newVariables)
            {
                Children.Add(item);
            }
        }

        private static void DispatchInvoke(Action toInvoke, DispatcherPriority priority)
        {
            Action guardedAction =
                () =>
                {
                    try
                    {
                        toInvoke();
                    }
                    catch (Exception e)
                    {
                        Debug.Assert(false, "Guarded invoke caught exception", e.Message);
                    }
                };

            Application.Current.Dispatcher.BeginInvoke(guardedAction, priority);    // TODO: acquiring Application.Current.Dispatcher, create utility class for UI thread and use it
        }
    }
}
