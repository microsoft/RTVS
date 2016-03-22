// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows.Media;
using Microsoft.Languages.Editor.Imaging;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.AST.DataTypes;
using Microsoft.R.Core.AST.Scopes.Definitions;
using Microsoft.R.Editor.Completion.Definitions;
using Microsoft.VisualStudio.Language.Intellisense;

namespace Microsoft.R.Editor.Completion.Providers {
    /// <summary>
    /// Provides list of functions and variables applicable to the current scope and 
    /// the caret position. Enumerates variables and function that appear before the
    /// current caret position as well as those declared in outer scopes.
    /// </summary>
    [Export(typeof(IRCompletionListProvider))]
    public class UserVariablesCompletionProvider : IRCompletionListProvider {

        #region IRCompletionListProvider
        public bool AllowSorting { get; } = true;

        public IReadOnlyCollection<RCompletion> GetEntries(RCompletionContext context) {
            List<RCompletion> completions = new List<RCompletion>();
            ImageSource functionGlyph = GlyphService.GetGlyph(StandardGlyphGroup.GlyphGroupMethod, StandardGlyphItem.GlyphItemPublic);
            ImageSource variableGlyph = GlyphService.GetGlyph(StandardGlyphGroup.GlyphGroupVariable, StandardGlyphItem.GlyphItemPublic);

            var ast = context.AstRoot;
            var scope = ast.GetNodeOfTypeFromPosition<IScope>(context.Position);

            var variables = scope.GetApplicableVariables(context.Position);
            foreach (var v in variables) {
                RCompletion completion;
                RFunction f = v.Value as RFunction;
                completion = new RCompletion(v.Name, CompletionUtilities.BacktickName(v.Name), string.Empty, f != null ? functionGlyph : variableGlyph);
                completions.Add(completion);
            }

            return completions;
        }
        #endregion
    }
}
