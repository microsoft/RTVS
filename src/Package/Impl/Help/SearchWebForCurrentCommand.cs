// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Text;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Components.Settings;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.Repl;
using Microsoft.VisualStudio.R.Packages.R;
using static System.FormattableString;

namespace Microsoft.VisualStudio.R.Package.Help {
    /// <summary>
    /// 'Search Web for ...' command that appears in the editor context menu.
    /// </summary>
    /// <remarks>
    /// Since command changes its name we have to make it package command
    /// since VS IDE no longer handles changing command names via OLE
    /// command target - it never calls IOlecommandTarget::QueryStatus
    /// with OLECMDTEXTF_NAME requesting changing names.
    /// </remarks>
    internal sealed class SearchWebForCurrentCommand : HelpOnCurrentCommandBase {
        private readonly IRSettings _settings;

        public SearchWebForCurrentCommand(
            IRInteractiveWorkflow workflow,
            IActiveWpfTextViewTracker textViewTracker,
            IActiveRInteractiveWindowTracker activeReplTracker) :
            base(RGuidList.RCmdSetGuid, RPackageCommandId.icmdSearchWebForCurrent,
                workflow, textViewTracker, activeReplTracker, Resources.SearchWebFor) {
            _settings = workflow.Shell.GetService<IRSettings>();
        }

        protected override void Handle(string item) {
            // Bing: search?q=item+site%3Astackoverflow.com
            var tokens = _settings.WebHelpSearchString.Split(new [] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

            var sb = new StringBuilder("https://" + Invariant($"www.bing.com/search?q={Uri.EscapeUriString(item)}"));
            foreach (var t in tokens) {
                sb.Append('+');
                sb.Append(Uri.EscapeUriString(t));
            }

            Workflow.Shell.Services.Process().Start(sb.ToString());
        }
    }
}
