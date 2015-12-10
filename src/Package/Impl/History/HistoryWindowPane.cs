using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.R.Editor.ContentType;
using Microsoft.R.Support.Settings;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Packages.R;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;
using DefGuidList = Microsoft.VisualStudio.Editor.DefGuidList;
using IServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

namespace Microsoft.VisualStudio.R.Package.History {
    [Guid(WindowGuid)]
    internal class HistoryWindowPane : ToolWindowPane, IOleCommandTarget {
        public const string WindowGuid = "62ACEA29-91C7-4BFC-B76F-550E7B3DE234";

        private readonly IContentTypeRegistryService _contentTypeRegistryService;
        private readonly IComponentModel _componentModel;
        private readonly Lazy<IRHistory> _historyProvider;
        private IServiceProvider _oleServiceProvider;
        private IWpfTextViewHost _wpfTextViewHost;
        private IVsTextView _vsTextViewAdapter;
        private IOleCommandTarget _commandTarget;

        public IWpfTextView TextView { get; private set; }

        public HistoryWindowPane() {
            Caption = Resources.HistoryWindowCaption;
            _contentTypeRegistryService = VsAppShell.Current.ExportProvider.GetExportedValue<IContentTypeRegistryService>();
            _componentModel = VsAppShell.Current.GetGlobalService<IComponentModel>(typeof(SComponentModel));
            _oleServiceProvider = VsAppShell.Current.GetGlobalService<IServiceProvider>();

            ToolBar = new CommandID(RGuidList.RCmdSetGuid, RPackageCommandId.historyWindowToolBarId);

            var historyProviderExport = VsAppShell.Current.ExportProvider.GetExport<IRHistoryProvider>();
            Debug.Assert(historyProviderExport != null);

            _historyProvider = new Lazy<IRHistory>(() => {
                var history = historyProviderExport.Value.GetAssociatedRHistory(TextView);
                history.HistoryChanged += OnHistoryChanged;
                return history;
            });
        }

        protected override void OnCreate() {
            CreateWpfTextViewHost();
            Content = _wpfTextViewHost;
            base.OnCreate();
        }

        public override void OnToolWindowCreated() {
            Guid commandUiGuid = VSConstants.GUID_TextEditorFactory;
            ((IVsWindowFrame)Frame).SetGuidProperty((int)__VSFPROPID.VSFPROPID_InheritKeyBindings, ref commandUiGuid);
            base.OnToolWindowCreated();
        }

        private void CreateWpfTextViewHost() {
            var editorAdapterFactory = _componentModel.GetService<IVsEditorAdaptersFactoryService>();
            var contentType = _contentTypeRegistryService.GetContentType(RHistoryContentTypeDefinition.ContentType);
            var textBufferAdapter = (IVsTextLines)editorAdapterFactory.CreateVsTextBufferAdapter(_oleServiceProvider, contentType);

            textBufferAdapter.InitializeContent(string.Empty, 0);

            _vsTextViewAdapter = editorAdapterFactory.CreateVsTextViewAdapter(_oleServiceProvider);
            _commandTarget = (IOleCommandTarget)_vsTextViewAdapter;

            IVsTextEditorPropertyContainer propContainer;
            ErrorHandler.ThrowOnFailure(((IVsTextEditorPropertyCategoryContainer)_vsTextViewAdapter).GetPropertyCategory(DefGuidList.guidEditPropCategoryViewMasterSettings, out propContainer));
            propContainer.SetProperty(VSEDITPROPID.VSEDITPROPID_ViewComposite_AllCodeWindowDefaults, true);
            propContainer.SetProperty(VSEDITPROPID.VSEDITPROPID_ViewGlobalOpt_AutoScrollCaretOnTextEntry, true);

            var initFlags = (uint)TextViewInitFlags.VIF_HSCROLL | (uint)TextViewInitFlags.VIF_VSCROLL | (uint)TextViewInitFlags3.VIF_NO_HWND_SUPPORT;
            var initView = new[] {
                new INITVIEW {
                    fSelectionMargin = 0U,
                    fWidgetMargin = 0U,
                    fVirtualSpace = 0U,
                    fDragDropMove = 0U
                }
            };

            _vsTextViewAdapter.Initialize(textBufferAdapter, IntPtr.Zero, initFlags, initView);
            _wpfTextViewHost = editorAdapterFactory.GetWpfTextViewHost(_vsTextViewAdapter);

            TextView = _wpfTextViewHost.TextView;

            TextView.Options.SetOptionValue(DefaultTextViewHostOptions.SelectionMarginId, false);
            TextView.Options.SetOptionValue(DefaultTextViewHostOptions.GlyphMarginId, false);
            TextView.Options.SetOptionValue(DefaultTextViewHostOptions.ZoomControlId, false);
            TextView.Options.SetOptionValue(DefaultWpfViewOptions.EnableMouseWheelZoomId, false);
            TextView.Options.SetOptionValue(DefaultWpfViewOptions.EnableHighlightCurrentLineId, false);
            TextView.Options.SetOptionValue(DefaultTextViewOptions.BraceCompletionEnabledOptionId, false);
            TextView.Options.SetOptionValue(DefaultTextViewOptions.DragDropEditingId, false);
            TextView.Caret.IsHidden = true;
            TextView.TextBuffer.ChangeContentType(contentType, null);
        }

        public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText) {
            return _commandTarget.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);
        }

        public int Exec(ref Guid pguidCmdGroup, uint nCmdId, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut) {
            return _commandTarget.Exec(ref pguidCmdGroup, nCmdId, nCmdexecopt, pvaIn, pvaOut);
        }

        public override bool SearchEnabled => true;

        public override void ProvideSearchSettings(IVsUIDataSource pSearchSettings) {
            var settings = (SearchSettingsDataSource)pSearchSettings;
            settings.SearchStartType = VSSEARCHSTARTTYPE.SST_INSTANT;
            base.ProvideSearchSettings(pSearchSettings);
        }

        public override IVsSearchTask CreateSearch(uint dwCookie, IVsSearchQuery pSearchQuery, IVsSearchCallback pSearchCallback) {
            return new HistorySearchTask(dwCookie, _historyProvider.Value, pSearchQuery, pSearchCallback);
        }

        public override void ClearSearch() {
            VsAppShell.Current.DispatchOnUIThread(() => _historyProvider.Value.ClearFilter());
            base.ClearSearch();
        }

        protected override void Dispose(bool disposing) {
            if (disposing) {
                _vsTextViewAdapter = null;
                _oleServiceProvider = null;
                _commandTarget = null;
                _historyProvider.Value.HistoryChanged -= OnHistoryChanged;
            }
            base.Dispose(disposing);
        }

        private void OnHistoryChanged(object sender, EventArgs e) {
            if (RToolsSettings.Current.ClearFilterOnAddHistory) {
                SearchHost.SearchAsync(null);
            }
        }

        private sealed class HistorySearchTask : VsSearchTask {
            private readonly IRHistory _history;

            public HistorySearchTask(uint dwCookie, IRHistory history, IVsSearchQuery pSearchQuery, IVsSearchCallback pSearchCallback)
                : base(dwCookie, pSearchQuery, pSearchCallback) {
                _history = history;
            }

            protected override void OnStartSearch() {
                base.OnStartSearch();
                VsAppShell.Current.DispatchOnUIThread(() => _history.Filter(SearchQuery.SearchString));
            }
        }
    }
}
