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
    /// Provides completion for variables in the current workspace.
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

        public IReadOnlyCollection<ICompletionEntry> GetEntries(IRIntellisenseContext context, string prefixFilter = null) {
            var completions = new List<ICompletionEntry>();
            var start = DateTime.Now;

            _variablesProvider.Initialize();
            var names = GetFieldProvidingVariableNames(context);

            foreach (var variableName in names) {
                var members = _variablesProvider.GetMembers(variableName, 200);
                foreach (var v in members) {
                    Debug.Assert(v != null);
                    if (v.Name.Length > 0 && v.Name[0] != '[') {
                        var glyph = v.ItemType == NamedItemType.Variable ? _variableGlyph : _functionGlyph;
                        var completion = new EditorCompletionEntry(v.Name.RemoveBackticks(), v.Name.BacktickName(), v.Description, glyph);
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
        private static IEnumerable<string> GetFieldProvidingVariableNames(IRIntellisenseContext context) {
            var list = new List<string>();
            // Traverse AST up to the nearest expression which parent is a scope
            // (i.e. not nested in other expressions) collecting names of indexed
            // variables. Example: a[2, b[3, x, |

            var indexer = context.AstRoot.GetNodeOfTypeFromPosition<Indexer>(context.Position, includeEnd: true);
            while (indexer != null) {
                if (indexer.RightOperand is Variable variable) {
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

            var name = context.Session.View.GetVariableNameBeforeCaret();
            if(!string.IsNullOrEmpty(name)) {
                list.Add(name);
            }
            return list;
        }
    }
}
