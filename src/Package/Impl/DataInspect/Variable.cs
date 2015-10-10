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
            //IsExpanded = false;
            Parent = parent;
        }

        public static Variable CreateEmpty() {
            return new Variable(null);
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

            // TODP: HasChildren
            /*
            if ((instance.TypeName == "data.frame")
                || (instance.TypeName == "matrix"))
            {
                instance.HasChildren = true;
            }
            */

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

        //bool _isExpanded;
        //public bool IsExpanded
        //{
        //    get { return _isExpanded; }
        //    set
        //    {
        //        if (_isExpanded != value)
        //        {
        //            _isExpanded = value;
        //            if (_isExpanded)
        //            {
        //                Expand();
        //            }
        //            else
        //            {
        //                Collapse();
        //            }

        //            //View?.RefreshView();
        //        }
        //    }
        //}

        public bool HasChildren { get; private set; }

        public ObservableCollection<Variable> Children { get; }

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

        public bool IsVisible { get; set; }

        /// <summary>
        /// simple Depth first traverse of Variable tree, and take action (Recursive)
        /// </summary>
        /// <param name="variables">variables to recurse</param>
        //public static void TraverseDepthFirst(IEnumerable<Variable> variables, Func<Variable, bool> action)
        //{
        //    foreach (var variable in variables)
        //    {
        //        if (action(variable))
        //        {
        //            if (variable.HasChildren)
        //            {
        //                TraverseDepthFirst(variable.Children, action);
        //            }
        //        }
        //    }
        //}

        #region Private

        //private void Expand()
        //{
        //    TraverseDepthFirst(this.Children,
        //        (v) => { v.IsVisible = true; return v.IsExpanded; });
        //}

        //private void Collapse()
        //{
        //    TraverseDepthFirst(this.Children,
        //        (v) => { v.IsVisible = false; return v.IsExpanded; });
        //}

        #endregion
    }
}
