// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows.Media;
using Microsoft.Languages.Editor.Imaging;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.AST.DataTypes;
using Microsoft.R.Core.AST.Scopes;
using Microsoft.VisualStudio.Language.Intellisense;

namespace Microsoft.R.Editor.Completions.Providers {
    /// <summary>
    /// Provides list of functions and variables applicable to the current scope and 
    /// the caret position. Enumerates variables and function that appear before the
    /// current caret position as well as those declared in outer scopes.
    /// </summary>
    [Export(typeof(IRCompletionListProvider))]
    public class UserVariablesCompletionProvider : IRCompletionListProvider {
        private readonly ImageSource _functionGlyph;
        private readonly ImageSource _variableGlyph;

        [ImportingConstructor]
        public UserVariablesCompletionProvider(IGlyphService glyphService) {
            _functionGlyph = glyphService.GetGlyphThreadSafe(StandardGlyphGroup.GlyphGroupMethod, StandardGlyphItem.GlyphItemPublic);
            _variableGlyph = glyphService.GetGlyphThreadSafe(StandardGlyphGroup.GlyphGroupVariable, StandardGlyphItem.GlyphItemPublic);
        }

        #region IRCompletionListProvider
        public bool AllowSorting { get; } = true;

        public IReadOnlyCollection<RCompletion> GetEntries(RCompletionContext context) {
            List<RCompletion> completions = new List<RCompletion>();

            var ast = context.AstRoot;
            // First try simple scope like in 'for(x in 1:10) x|'
            IScope scope = ast.GetNodeOfTypeFromPosition<SimpleScope>(context.Position, includeEnd: true);
            // If not found, look for the regular scope
            scope = scope ?? ast.GetNodeOfTypeFromPosition<IScope>(context.Position);

            var variables = scope.GetApplicableVariables(context.Position);
            foreach (var v in variables) {
                RCompletion completion;
                RFunction f = v.Value as RFunction;
                completion = new RCompletion(v.Name, CompletionUtilities.BacktickName(v.Name), string.Empty, f != null ? _functionGlyph : _variableGlyph);
                completions.Add(completion);
            }

            return completions;
        }
        #endregion
    }
}
