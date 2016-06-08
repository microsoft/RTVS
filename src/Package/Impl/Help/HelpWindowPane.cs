// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using Microsoft.Common.Core;
using Microsoft.Languages.Editor.Composition;
using Microsoft.R.Components.Help;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Editor.Completion.Definitions;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.Interop;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Packages.R;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Package.Help {
    [Guid(WindowGuidString)]
    internal class HelpWindowPane : VisualComponentToolWindow<IHelpVisualComponent> {
        public const string WindowGuidString = "9E909526-A616-43B2-A82B-FD639DCD40CB";
        public static Guid WindowGuid { get; } = new Guid(WindowGuidString);

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

        public override bool SearchEnabled => true;

        public override void ProvideSearchSettings(IVsUIDataSource pSearchSettings) {
            dynamic settings = pSearchSettings;
            settings.SearchWatermark = Resources.HelpSearchWatermark;
            settings.SearchTooltip = Resources.HelpSearchTooltip;
            settings.SearchStartType = VSSEARCHSTARTTYPE.SST_ONDEMAND;
            base.ProvideSearchSettings(pSearchSettings);
        }

        public override IVsSearchTask CreateSearch(uint dwCookie, IVsSearchQuery pSearchQuery, IVsSearchCallback pSearchCallback) {
            return new HelpSearchTask(dwCookie, pSearchQuery, pSearchCallback);
        }

        private sealed class HelpSearchTask : VsSearchTask {
            //private static IEnumerable<Lazy<IRHelpSearchTermProvider>> _termProviders;
            //private readonly List<string> _terms = new List<string>();
            private readonly IVsSearchCallback _callback;
            private IRInteractiveWorkflowProvider _workflowProvider;

            public HelpSearchTask(uint dwCookie, IVsSearchQuery pSearchQuery, IVsSearchCallback pSearchCallback)
                : base(dwCookie, pSearchQuery, pSearchCallback) {
                _callback = pSearchCallback;
                _workflowProvider = VsAppShell.Current.ExportProvider.GetExportedValue<IRInteractiveWorkflowProvider>();
            }

            protected override async void OnStartSearch() {
                base.OnStartSearch();
                _callback.ReportComplete(this, 1);

                var searchString = SearchQuery.SearchString;
                if (!string.IsNullOrWhiteSpace(searchString)) {
                    try {
                        var session = _workflowProvider.GetOrCreate().RSession;
                        await session.ExecuteAsync($"rtvs:::show_help({searchString.ToRStringLiteral()})");
                    } catch (OperationCanceledException) {
                    } catch (RException) {
                    } catch (MessageTransportException) {
                    }
                }
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
