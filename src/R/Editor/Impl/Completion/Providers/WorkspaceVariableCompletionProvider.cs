// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Windows.Media;
using Microsoft.Common.Core.Shell;
using Microsoft.Languages.Editor.Imaging;
using Microsoft.Languages.Editor.Shell;
using Microsoft.R.Editor.Completion.Definitions;
using Microsoft.R.Support.Help.Definitions;
using Microsoft.VisualStudio.Language.Intellisense;

namespace Microsoft.R.Editor.Completion.Providers {
    /// <summary>
    /// Provides list of installed packages for completion inside 
    /// library(...) statement. List of packages is  obtained from 
    /// ~\Program Files\R and from ~\Documents\R folders
    /// </summary>
    public sealed class WorkspaceVariableCompletionProvider : IRCompletionListProvider {
        private readonly ICoreShell _shell;

        [Import]
        private IVariablesProvider VariablesProvider { get; set; }

        public WorkspaceVariableCompletionProvider(ICoreShell shell) {
            _shell = shell;
            _shell.CompositionService.SatisfyImportsOnce(this);
        }

        #region IRCompletionListProvider
        public bool AllowSorting { get; } = true;

        public IReadOnlyCollection<RCompletion> GetEntries(RCompletionContext context) {
            List<RCompletion> completions = new List<RCompletion>();
            ImageSource functionGlyph = GlyphService.GetGlyph(StandardGlyphGroup.GlyphGroupMethod, StandardGlyphItem.GlyphItemPublic, _shell);
            ImageSource variableGlyph = GlyphService.GetGlyph(StandardGlyphGroup.GlyphGroupVariable, StandardGlyphItem.GlyphItemPublic, _shell);

            string variableName = context.Session.TextView.GetVariableNameBeforeCaret();

            VariablesProvider.Initialize();
            int memberCount = VariablesProvider.GetMemberCount(variableName);
            IReadOnlyCollection<INamedItemInfo> members = VariablesProvider.GetMembers(variableName, 200);
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
