// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using Microsoft.Languages.Editor.Composition;
using Microsoft.R.Components.Help;
using Microsoft.R.Editor.Completion.Definitions;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.Interop;
using Microsoft.VisualStudio.R.Package.Search;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Packages.R;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Package.Help {
    [Guid(WindowGuidString)]
    internal class HelpWindowPane : VisualComponentToolWindow<IHelpVisualComponent> {
        public const string WindowGuidString = "9E909526-A616-43B2-A82B-FD639DCD40CB";
        public static Guid WindowGuid { get; } = new Guid(WindowGuidString);

        private static IEnumerable<Lazy<IRHelpSearchTermProvider>> _termProviders;

        public HelpWindowPane() {
            Caption = Resources.HelpWindowCaption;
            BitmapImageMoniker = KnownMonikers.StatusHelp;
            ToolBar = new CommandID(RGuidList.RCmdSetGuid, RPackageCommandId.helpWindowToolBarId);
        }

        protected override void OnCreate() {
            Component = new HelpVisualComponent { Container = this };
            ToolBarCommandTarget = new CommandTargetToOleShim(null, Component.Controller);
            base.OnCreate();
        }

        public override void ProvideSearchSettings(IVsUIDataSource pSearchSettings) {
            var settings = (SearchSettingsDataSource)pSearchSettings;
            settings.SearchWatermark = Resources.HelpSearchWatermark;
            settings.SearchTooltip = Resources.HelpSearchTooltip;
            settings.SearchStartType = VSSEARCHSTARTTYPE.SST_ONDEMAND;
            base.ProvideSearchSettings(pSearchSettings);
        }

        public override IVsSearchTask CreateSearch(uint dwCookie, IVsSearchQuery pSearchQuery, IVsSearchCallback pSearchCallback) {
            return new HelpSearchTask(dwCookie, pSearchQuery, pSearchCallback);
        }

        private sealed class HelpSearchTask : VsSearchTask {
            private readonly IVsSearchCallback _callback;
            public HelpSearchTask(uint dwCookie, IVsSearchQuery pSearchQuery, IVsSearchCallback pSearchCallback)
                : base(dwCookie, pSearchQuery, pSearchCallback) {
                _callback = pSearchCallback;
            }

            protected override void OnStartSearch() {
                base.OnStartSearch();
                VsAppShell.Current.DispatchOnUIThread(() => _historyFiltering.Filter(SearchQuery.SearchString));
            }
        }
        private string[] GetSearchTerms() {
            var entries = new List<string>();
            foreach (var p in TermProviders) {
                entries.AddRange(p.Value.GetEntries());
            }
            entries.Sort();
            return entries.ToArray();
        }

        private static IEnumerable<Lazy<IRHelpSearchTermProvider>> TermProviders {
            get {
                if (_termProviders == null) {
                    _termProviders = ComponentLocator<IRHelpSearchTermProvider>.ImportMany();
                }
                return _termProviders;
            }
        }
    }
}
