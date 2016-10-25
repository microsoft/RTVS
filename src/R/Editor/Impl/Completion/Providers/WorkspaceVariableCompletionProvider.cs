// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Media;
using Microsoft.Languages.Editor.Imaging;
using Microsoft.R.Support.Help;
using Microsoft.VisualStudio.Language.Intellisense;

namespace Microsoft.R.Editor.Completion.Providers {
    /// <summary>
    /// Provides list of installed packages for completion inside 
    /// library(...) statement. List of packages is  obtained from 
    /// ~\Program Files\R and from ~\Documents\R folders
    /// </summary>
    public sealed class WorkspaceVariableCompletionProvider : IRCompletionListProvider {
        private readonly IVariablesProvider _variablesProvider;
        private readonly IGlyphService _glyphService;

        public WorkspaceVariableCompletionProvider(IVariablesProvider provider, IGlyphService glyphService) {
            _variablesProvider = provider;
            _glyphService = glyphService;
        }

        #region IRCompletionListProvider
        public bool AllowSorting { get; } = true;

        public IReadOnlyCollection<RCompletion> GetEntries(RCompletionContext context) {
            List<RCompletion> completions = new List<RCompletion>();
            ImageSource functionGlyph = _glyphService.GetGlyphThreadSafe(StandardGlyphGroup.GlyphGroupMethod, StandardGlyphItem.GlyphItemPublic);
            ImageSource variableGlyph = _glyphService.GetGlyphThreadSafe(StandardGlyphGroup.GlyphGroupVariable, StandardGlyphItem.GlyphItemPublic);

            string variableName = context.Session.TextView.GetVariableNameBeforeCaret();

            _variablesProvider.Initialize();
            int memberCount = _variablesProvider.GetMemberCount(variableName);
            IReadOnlyCollection<INamedItemInfo> members = _variablesProvider.GetMembers(variableName, 200);
            // Get list of functions in the package
            foreach (var v in members) {
                Debug.Assert(v != null);
                if (v.Name.Length > 0 && v.Name[0] != '[') {
                    ImageSource glyph = v.ItemType == NamedItemType.Variable ? variableGlyph : functionGlyph;
                    var completion = new RCompletion(v.Name, CompletionUtilities.BacktickName(v.Name), v.Description, glyph);
                    completions.Add(completion);
                }
            }

            return completions;
        }
        #endregion
    }
}
