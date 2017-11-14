// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using Microsoft.Common.Core.Imaging;
using Microsoft.Languages.Editor.Completions;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.AST.DataTypes;
using Microsoft.R.Core.AST.Scopes;

namespace Microsoft.R.Editor.Completions.Providers {
    /// <summary>
    /// Provides list of functions and variables applicable to the current scope and 
    /// the caret position. Enumerates variables and function that appear before the
    /// current caret position as well as those declared in outer scopes.
    /// </summary>
    public sealed class UserVariablesCompletionProvider : IRCompletionListProvider {
        private readonly object _functionGlyph;
        private readonly object _variableGlyph;

        public UserVariablesCompletionProvider(IImageService imageService) {
            _functionGlyph = imageService.GetImage(ImageType.Method);
            _variableGlyph = imageService.GetImage(ImageType.Variable);
        }

        #region IRCompletionListProvider
        public bool AllowSorting { get; } = true;

        public IReadOnlyCollection<ICompletionEntry> GetEntries(IRIntellisenseContext context, string prefixFilter = null) {
            var completions = new List<ICompletionEntry>();
            var ast = context.AstRoot;

            // First try simple scope like in 'for(x in 1:10) x|'
            IScope scope = ast.GetNodeOfTypeFromPosition<SimpleScope>(context.Position, includeEnd: true);
            // If not found, look for the regular scope
            scope = scope ?? ast.GetNodeOfTypeFromPosition<IScope>(context.Position);

            var variables = scope.GetApplicableVariables(context.Position);
            foreach (var v in variables) {
                var f = v.Value as RFunction;
                var completion = new EditorCompletionEntry(v.Name.RemoveBackticks(), v.Name.BacktickName(), string.Empty, f != null ? _functionGlyph : _variableGlyph);
                completions.Add(completion);
            }

            return completions;
        }
        #endregion
    }
}
