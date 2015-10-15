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
using Microsoft.VisualStudio.R.Package.Utilities;

namespace Microsoft.VisualStudio.R.Package.DataInspect
{
    internal class VariableVisualInfo   // TODO: remove this later
    {
        public int IndentStep { get; set; }
        public double NameWidth { get; set; }
        public double ValueWidth { get; set; }
        public double TypeWidth { get; set; }
    }

    internal class Variable : BindableBase
    {
        private Variable(VariableEvaluationContext evaluationContext)
        {
            Children = new ObservableCollection<Variable>();
            EvaluationContext = evaluationContext;
        }

        private Variable _parent;
        public Variable Parent
        {
            get { return _parent; }
            private set
            {
                _parent = value;
                if (_parent != null)
                {
                    IndentStep = _parent.IndentStep + 1;
                    NameWidth = _parent.NameWidth;
                    ValueWidth = _parent.ValueWidth;
                    TypeWidth = _parent.TypeWidth;
                }
            }
        }
        public int IndentStep { get; private set; }

        public VariableEvaluationContext EvaluationContext { get; }

        /// <summary>
        /// Create Variable instance from R evaluation
        /// </summary>
        /// <param name="evaluation"></param>
        /// <returns></returns>
        public static Variable Create(
            Variable parent,
            REvaluation evaluation,
            VariableEvaluationContext evaluationContext,
            VariableVisualInfo vvi = null)
        {
            const string DataFramePrefix = "'data.frame':";
            var instance = new Variable(evaluationContext);

            instance.Parent = parent;
            if (vvi != null)
            {
                instance.IndentStep = vvi.IndentStep;
                instance.NameWidth = vvi.NameWidth;
                instance.ValueWidth = vvi.ValueWidth;
                instance.TypeWidth = vvi.TypeWidth;
            }

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
                    instance.Children.Add(Variable.Create(instance, child, evaluationContext));
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
            set { SetProperty<string>(ref _variableName, value); }
        }

        private string _variableValue;
        /// <summary>
        /// Value of variable, represented in short string
        /// </summary>
        public string VariableValue
        {
            get { return _variableValue; }
            set { SetProperty<string>(ref _variableValue, value); }
        }

        private string _typeName;
        /// <summary>
        /// Type name of variable
        /// </summary>
        public string TypeName
        {
            get { return _typeName; }
            set { SetProperty<string>(ref _typeName, value); }
        }

        // TODO: repeating the same three times ugly!!! create column, columnheader and use binding
        private double _nameWidth;
        public double NameWidth
        {
            get { return _nameWidth; }
            set { SetProperty<double>(ref _nameWidth, value); }
        }

        private double _valueWidth;
        public double ValueWidth
        {
            get { return _valueWidth; }
            set { SetProperty<double>(ref _valueWidth, value); }
        }

        private double _typeWidth;
        public double TypeWidth
        {
            get { return _typeWidth; }
            set { SetProperty<double>(ref _typeWidth, value); }
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

            // assume Children is in order
            Children.InplaceUpdate(
                update.Children,
                (s, u) => s.VariableName == u.VariableName,
                (s, u) => s.UpdateInternal(u));
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
