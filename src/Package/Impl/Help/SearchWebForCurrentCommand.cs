// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Text;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Support.Settings;
using Microsoft.VisualStudio.R.Package.Browsers;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.Repl;
using Microsoft.VisualStudio.R.Package.Shell;
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
        private readonly IWebBrowserServices _webBrowserServices;

        public SearchWebForCurrentCommand(
            IRInteractiveWorkflow workflow,
            IActiveWpfTextViewTracker textViewTracker,
            IActiveRInteractiveWindowTracker activeReplTracker,
            IWebBrowserServices webBrowserServices) :
            base(RGuidList.RCmdSetGuid, RPackageCommandId.icmdSearchWebForCurrent,
                workflow, textViewTracker, activeReplTracker, Resources.SearchWebFor) {
            _webBrowserServices = webBrowserServices;
        }

        protected override void Handle(string item) {
            // Bing: search?q=item+site%3Astackoverflow.com
            var tokens = RToolsSettings.Current.WebHelpSearchString.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

            var sb = new StringBuilder("https://" + Invariant($"www.bing.com/search?q={Uri.EscapeUriString(item)}"));
            foreach (var t in tokens) {
                sb.Append('+');
                sb.Append(Uri.EscapeUriString(t));
            }

            var wbs = VsAppShell.Current.ExportProvider.GetExportedValue<IWebBrowserServices>();
            wbs.OpenBrowser(WebBrowserRole.Help, sb.ToString());
        }
    }
}
