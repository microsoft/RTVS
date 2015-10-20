using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.R.Editor.Completion.Definitions;
using Microsoft.R.Editor.ContentType;
using Microsoft.R.Support.Help.Definitions;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.VisualStudio.R.Package.DataInspect.Completion
{
    [Export(typeof(IVariablesProvider))]
    [ContentType(RContentTypeDefinition.ContentType)]
    internal sealed class WorkspaceVariableProvider : IVariablesProvider
    {
        private Dictionary<string, REvaluation> _topLevelVariables = new Dictionary<string, REvaluation>();

        public WorkspaceVariableProvider()
        {
            VariableProvider.Current.VariableChanged += OnVariableChanged;
        }

        #region IVariablesProvider
        public IReadOnlyCollection<INamedItemInfo> Variables
        {
            get
            {
                List<INamedItemInfo> vars = new List<INamedItemInfo>();

                foreach (REvaluation ev in _topLevelVariables.Values)
                {
                    vars.Add(new VariableInfo(ev));
                }

                return vars;
            }
        }

        public IReadOnlyCollection<INamedItemInfo> GetMembers(string variableName)
        {
            string[] parts = variableName.Split(new char[] { '$', '@' });
            List<INamedItemInfo> members = new List<INamedItemInfo>();
            REvaluation eval;

            if (parts.Length > 0 && _topLevelVariables.TryGetValue(parts[0], out eval))
            {
                for (int i = 1; i < parts.Length; i++)
                {
                    if (eval.Children != null || eval.Children.Evals != null)
                    {
                        eval = eval.Children.Evals.FirstOrDefault((x) => x != null && x.Name == parts[i]);
                        if(eval == null)
                        {
                            break;
                        }
                    }
                }

                if(eval != null && eval.Children != null && eval.Children.Evals != null)
                {
                    foreach (REvaluation ev in eval.Children.Evals)
                    {
                        members.Add(new VariableInfo(ev));
                    }
                }
            }

            return members;
        }
        #endregion

        private void OnVariableChanged(object sender, VariableChangedArgs e)
        {
            UpdateList(e.NewVariable);
        }

        private void UpdateList(REvaluation e)
        {
            if (e == null)
            {
                _topLevelVariables.Clear();
                return;
            }

            if (e.Children != null && e.Children.Evals != null)
            {
                foreach (var x in e.Children.Evals)
                {
                    _topLevelVariables[x.Name] = x;
                }
            }
        }

        class VariableInfo : INamedItemInfo
        {
            public VariableInfo(REvaluation e)
            {
                this.Name = e.Name;
                if(e.TypeName == "function")
                {
                    ItemType = NamedItemType.Function;
                }
                else
                {
                    ItemType = NamedItemType.Variable;
                }
            }

            public string Description
            {
                get { return string.Empty; }
            }


            public NamedItemType ItemType { get; private set; }

            public string Name { get; set; }
        }
    }
}
