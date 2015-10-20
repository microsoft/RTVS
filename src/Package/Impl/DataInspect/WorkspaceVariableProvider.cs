using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.R.Editor.Completion.Definitions;
using Microsoft.R.Editor.ContentType;
using Microsoft.R.Support.Help.Definitions;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.VisualStudio.R.Package.DataInspect
{
    /// <summary>
    /// Provides name of variables and members declared in REPL workspace
    /// </summary>
    [Export(typeof(IVariablesProvider))]
    [ContentType(RContentTypeDefinition.ContentType)]
    internal sealed class WorkspaceVariableProvider : IVariablesProvider
    {
        /// <summary>
        /// Collection of top-level variables
        /// </summary>
        private Dictionary<string, REvaluation> _topLevelVariables = new Dictionary<string, REvaluation>();

        public WorkspaceVariableProvider()
        {
            VariableProvider.Current.VariableChanged += OnVariableChanged;
        }

        #region IVariablesProvider
        /// <summary>
        /// Given variable name determines number of members
        /// </summary>
        /// <param name="variableName">Variable name or null if global scope</param>
        public int GetMemberCount(string variableName)
        {
            if (string.IsNullOrEmpty(variableName))
            {
                // Global scope
                return _topLevelVariables.Values.Count;
            }

            // TODO: do estimate
            return 100;
        }

        /// <summary>
        /// Given variable name returns variable members
        /// adhering to specified criteria. Last member name
        /// may be partial such as abc$def$g
        /// </summary>
        /// <param name="variableName">
        /// Variable name such as abc$def$g. 'g' may be partially typed
        /// in which case providers returns members of 'def' filtered to 'g' prefix.
        /// </param>
        /// <param name="maxCount">Max number of members to return</param>
        public IReadOnlyCollection<INamedItemInfo> GetMembers(string variableName, int maxCount)
        {
            // Split abc$def$g into parts. String may also be empty or end with $ or @.
            string[] parts = variableName.Split(new char[] { '$', '@' });

            if (parts.Length == 0 || parts[0].Length == 0)
            {
                // Global scope
                return _topLevelVariables.Values.Select((m, index) =>
                {
                    return index < maxCount ? new VariableInfo(m) : null;
                }).ToArray();
            }

            // Last member name may be empty or partially typed
            string lastMemberName = parts[parts.Length - 1];
            // in abc$def$ or in abc$def$g we need to retrieve members of 'def' 
            // so the last part doesn't matter.
            int partCount = parts.Length - 1;
            REvaluation eval;

            // Go by parts and drill into members
            if (_topLevelVariables.TryGetValue(parts[0], out eval))
            {
                for (int i = 1; i < partCount; i++)
                {
                    string part = parts[i];

                    if (string.IsNullOrEmpty(part))
                    {
                        // Something looks like abc$$def
                        break;
                    }

                    if (eval.Children != null || eval.Children.Evals != null)
                    {
                        eval = eval.Children.Evals.FirstOrDefault((x) => x != null && x.Name == part);
                        if (eval == null)
                        {
                            break;
                        }
                    }
                }

                if (eval != null && eval.Children != null && eval.Children.Evals != null)
                {
                    return eval.Children.Evals.Select((m, index) =>
                    {
                        return index < maxCount ? new VariableInfo(m) : null;
                    }).ToArray();
                }
            }

            return new VariableInfo[0];
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
                if (e.TypeName == "function")
                {
                    ItemType = NamedItemType.Function;
                }
                else
                {
                    ItemType = NamedItemType.Variable;
                }
            }

            public string Description { get; } = string.Empty;

            public NamedItemType ItemType { get; private set; }

            public string Name { get; set; }
        }
    }
}
