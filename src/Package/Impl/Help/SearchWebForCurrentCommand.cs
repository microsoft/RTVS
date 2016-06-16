// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.Text;
using System.Web;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Support.Settings;
using Microsoft.R.Support.Settings.Definitions;
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
            var tokens = RToolsSettings.Current.WebHelpSearchString.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            bool siteSet = false;

            var sb = new StringBuilder("http://" + Invariant($"www.bing.com/search?q={HttpUtility.HtmlEncode(item)}"));
            foreach (var t in tokens) {
                sb.Append('+');
                if (!siteSet && t.IndexOf('.') > 0) {
                    sb.Append("site:");
                    siteSet = true;
                }
                sb.Append(t);
            }

            if (RToolsSettings.Current.WebHelpSearchBrowserType == WebHelpSearchBrowserType.Internal) {
                IVsWindowFrame frame;
                var browser = VsAppShell.Current.GetGlobalService<IVsWebBrowsingService>(typeof(SVsWebBrowsingService));
                browser.Navigate(sb.ToString(), (uint)__VSWBNAVIGATEFLAGS.VSNWB_WebURLOnly, out frame);
            } else {
                Process.Start(sb.ToString());
            }
        }
    }
}
