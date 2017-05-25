// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Runtime.InteropServices;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Services;
using Microsoft.R.Components.Help;
using Microsoft.R.Components.Help.Commands;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.Interop;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Packages.R;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;
using static System.FormattableString;

namespace Microsoft.VisualStudio.R.Package.Help {
    [Guid(WindowGuidString)]
    internal class HelpWindowPane : VisualComponentToolWindow<IHelpVisualComponent> {
        public const string WindowGuidString = "9E909526-A616-43B2-A82B-FD639DCD40CB";
        public static Guid WindowGuid { get; } = new Guid(WindowGuidString);

        public HelpWindowPane(IServiceContainer services): base(services) {
            Caption = Resources.HelpWindowCaption;
            BitmapImageMoniker = KnownMonikers.StatusHelp;
            ToolBar = new System.ComponentModel.Design.CommandID(RGuidList.RCmdSetGuid, RPackageCommandId.helpWindowToolBarId);
        }

        protected override void OnCreate() {
            Component = new HelpVisualComponent(Services) { Container = this };
            var controller = new AsyncCommandController()
                .AddCommand(RGuidList.RCmdSetGuid, RPackageCommandId.icmdHelpHome, new HelpHomeCommand(Services))
                .AddCommand(RGuidList.RCmdSetGuid, RPackageCommandId.icmdHelpNext, new HelpNextCommand(Component))
                .AddCommand(RGuidList.RCmdSetGuid, RPackageCommandId.icmdHelpPrevious, new HelpPreviousCommand(Component))
                .AddCommand(RGuidList.RCmdSetGuid, RPackageCommandId.icmdHelpRefresh, new HelpRefreshCommand(Component));
            ToolBarCommandTarget = new CommandTargetToOleShim(null, controller);
            base.OnCreate();
        }

        public override bool SearchEnabled => true;

        public override void ProvideSearchSettings(IVsUIDataSource pSearchSettings) {
            dynamic settings = pSearchSettings;
            settings.SearchWatermark = Resources.HelpSearchWatermark;
            settings.SearchTooltip = Resources.HelpSearchTooltip;
            settings.SearchStartType = VSSEARCHSTARTTYPE.SST_ONDEMAND;
            settings.RestartSearchIfUnchanged = true;
            base.ProvideSearchSettings(pSearchSettings);
        }

        public override IVsSearchTask CreateSearch(uint dwCookie, IVsSearchQuery pSearchQuery, IVsSearchCallback pSearchCallback) {
            return new HelpSearchTask(dwCookie, pSearchQuery, pSearchCallback, Services);
        }

        private sealed class HelpSearchTask : VsSearchTask {
            //private static IEnumerable<Lazy<IRHelpSearchTermProvider>> _termProviders;
            //private readonly List<string> _terms = new List<string>();
            private readonly IVsSearchCallback _callback;
            private readonly IRInteractiveWorkflowProvider _workflowProvider;

            public HelpSearchTask(uint dwCookie, IVsSearchQuery pSearchQuery, IVsSearchCallback pSearchCallback, IServiceContainer services)
                : base(dwCookie, pSearchQuery, pSearchCallback) {
                _callback = pSearchCallback;
                _workflowProvider = services.GetService<IRInteractiveWorkflowProvider>();
            }

            protected override void OnStartSearch() {
                base.OnStartSearch();
                _callback.ReportComplete(this, 1);

                var searchString = SearchQuery.SearchString;
                if (!string.IsNullOrWhiteSpace(searchString)) {
                    SearchAsync(searchString).DoNotWait();
                }
            }

            private Task SearchAsync(string searchString) {
                var session = _workflowProvider.GetOrCreate().RSession;
                return session.ExecuteAsync(Invariant($"rtvs:::show_help({searchString.ToRStringLiteral()})"));
            }


            // TODO: activate when adding completion to the search box
            //private void GetSearchTerms() {
            //    if (_terms.Count == 0) { }
            //    foreach (var p in TermProviders) {
            //        _terms.AddRange(p.Value.GetEntries());
            //    }
            //    _terms.Sort();
            //}

            //private static IEnumerable<Lazy<IRHelpSearchTermProvider>> TermProviders {
            //    get {
            //        if (_termProviders == null) {
            //            _termProviders = ComponentLocator<IRHelpSearchTermProvider>.ImportMany();
            //        }
            //        return _termProviders;
            //    }
            //}
        }
    }
}
