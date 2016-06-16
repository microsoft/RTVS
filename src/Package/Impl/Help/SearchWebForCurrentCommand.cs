// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Web;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Support.Settings;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.Repl;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Packages.R;
using Microsoft.VisualStudio.Shell.Interop;
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
        public SearchWebForCurrentCommand(
            IRInteractiveWorkflow workflow,
            IActiveWpfTextViewTracker textViewTracker,
            IActiveRInteractiveWindowTracker activeReplTracker) :
            base(RGuidList.RCmdSetGuid, RPackageCommandId.icmdSearchWebForCurrent,
                workflow, textViewTracker, activeReplTracker, Resources.SearchWebFor) {
        }

        protected override void Handle(string item) {
            // Bing: search?q=item+site%3Astackoverflow.com
            var encoded = HttpUtility.HtmlEncode(item);
            var search = "http://" + Invariant($"www.bing.com/search?q={encoded}+R+site%3A{RToolsSettings.Current.WebHelpSearchString}");
            var browser = VsAppShell.Current.GetGlobalService<IVsWebBrowsingService>(typeof(SVsWebBrowsingService));
            IVsWindowFrame frame;
            browser.Navigate(search, (uint)__VSWBNAVIGATEFLAGS.VSNWB_WebURLOnly, out frame);
        }
    }
}
