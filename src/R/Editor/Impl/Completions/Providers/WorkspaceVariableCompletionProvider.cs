// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Common.Core.Imaging;
using Microsoft.Languages.Editor.Completions;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.AST.Scopes;
using Microsoft.R.Core.AST.Variables;
using Microsoft.R.Editor.Functions;

namespace Microsoft.R.Editor.Completions.Providers {
    /// <summary>
    /// Provides list of installed packages for completion inside 
    /// library(...) statement. List of packages is  obtained from 
    /// ~\Program Files\R and from ~\Documents\R folders
    /// </summary>
    public sealed class WorkspaceVariableCompletionProvider : IRCompletionListProvider {
        private readonly IVariablesProvider _variablesProvider;
        private readonly object _functionGlyph;
        private readonly object _variableGlyph;

        public WorkspaceVariableCompletionProvider(IVariablesProvider provider, IImageService imageService) {
            _variablesProvider = provider;
            _functionGlyph = imageService.GetImage(ImageType.Method);
            _variableGlyph = imageService.GetImage(ImageType.Variable);
        }

        #region IRCompletionListProvider
        public bool AllowSorting { get; } = true;

        public IReadOnlyCollection<ICompletionEntry> GetEntries(IRCompletionContext context) {
            var completions = new List<ICompletionEntry>();

            var start = DateTime.Now;

            _variablesProvider.Initialize();
            var names = GetFieldProvidingVariableNames(context);

            foreach (var variableName in names) {
                int memberCount = _variablesProvider.GetMemberCount(variableName);
                IReadOnlyCollection<INamedItemInfo> members = _variablesProvider.GetMembers(variableName, 200);

                foreach (var v in members) {
                    Debug.Assert(v != null);
                    if (v.Name.Length > 0 && v.Name[0] != '[') {
                        var glyph = v.ItemType == NamedItemType.Variable ? _variableGlyph : _functionGlyph;
                        var completion = new EditorCompletionEntry(v.Name, v.Name.WrapInBacktics(), v.Description, glyph);
                        completions.Add(completion);
                    }
                }
            }
            Debug.WriteLine("Variable members fetch: " + (DateTime.Now - start).TotalMilliseconds);
            return completions;
        }
        #endregion

        /// <summary>
        /// Retrieves names of workspace variables that may be supplying fields to the completion list.
        /// </summary>
        /// <remarks>
        /// For example, in
        ///     dt &lt;- data.table(mtcars)
        ///     dt[c|
        /// we want to complete for 'cyl'. This expressions can be nested.
        /// </remarks>
        private static IEnumerable<string> GetFieldProvidingVariableNames(IRCompletionContext context) {
            var list = new List<string>();
            // Traverse AST up to the nearest expression which parent is a scope
            // (i.e. not nested in other expressions) collecting names of indexed
            // variables. Example: a[2, b[3, x, |

            var indexer = context.AstRoot.GetNodeOfTypeFromPosition<Indexer>(context.Position, includeEnd: true);
            while (indexer != null) {
                var variable = indexer.RightOperand as Variable;
                if (variable != null) {
                    list.Add(variable.Name + "$");
                } else {
                    break;
                }

                var node = (IAstNode)indexer;
                indexer = null;
                while (indexer == null && !(node is IScope)) {
                    node = node.Parent;
                    indexer = node as Indexer;
                }
            }

            if (list.Count > 0) {
                return list;
            }

            var name = context.Session.View.GetVariableNameBeforeCaret();
            return !string.IsNullOrEmpty(name) ? new string[] { name } : Enumerable.Empty<string>();
        }
    }
}
