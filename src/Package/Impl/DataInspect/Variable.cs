using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Microsoft.VisualStudio.R.Package.DataInspect
{
    public class Variable
    {
        private Variable(Variable parent, VariableView view = null)
        {
            Children = new ObservableCollection<Variable>();
            View = view;
            Parent = parent;
        }

        public static Variable CreateEmpty() {
            return new Variable(null);
        }

        public VariableEvaluationContext EvaluationContext { get; private set; }

        public static Variable Create(REvaluation evaluation, VariableEvaluationContext context)
        {
            var instance = Create(evaluation);
            instance.EvaluationContext = context;

            return instance;
        }

        /// <summary>
        /// Create Variable instance from R evaluation
        /// </summary>
        /// <param name="evaluation"></param>
        /// <returns></returns>
        public static Variable Create(REvaluation evaluation)
        {
            const string DataFramePrefix = "'data.frame':";
            var instance = new Variable(null, null);

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

            if (evaluation.Children != null)
            {
                foreach (var child in evaluation.Children)
                {
                    instance.Children.Add(Variable.Create(child));
                }
            }

            return instance;
        }

        /// <summary>
        /// <see cref="VariableView"/> that owns this
        /// </summary>
        public VariableView View { get; set; }

        /// <summary>
        /// Name of variable
        /// </summary>
        public string VariableName { get; set; }

        /// <summary>
        /// Value of variable, represented in short string
        /// </summary>
        public string VariableValue { get; set; }

        /// <summary>
        /// Type name of variable
        /// </summary>
        public string TypeName { get; set; }

        public bool HasChildren { get; private set; }

        public ObservableCollection<Variable> Children
        {
            get;
        }

        Variable _parent;
        public Variable Parent
        {
            get { return _parent; }
            private set
            {
                _parent = value;
                if (_parent != null)
                {
                    Level = _parent.Level + 1;
                }
            }
        }

        public int Level { get; set; }
    }
}
