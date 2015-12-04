using System;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using System.Windows.Threading;
using Microsoft.Languages.Editor.Shell;
using Microsoft.R.Editor.ContentType;
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
        private readonly Lazy<IRHistoryProvider> _historyProviderExport;
        private IServiceProvider _oleServiceProvider;
        private IWpfTextViewHost _wpfTextViewHost;
        private IVsTextView _vsTextViewAdapter;
        private IOleCommandTarget _commandTarget;

        public IWpfTextView TextView { get; private set; }

        public HistoryWindowPane() {
            Caption = Resources.HistoryWindowCaption;
            _contentTypeRegistryService = AppShell.Current.ExportProvider.GetExportedValue<IContentTypeRegistryService>();
            _historyProviderExport = AppShell.Current.ExportProvider.GetExport<IRHistoryProvider>();
            _componentModel = AppShell.Current.GetGlobalService<IComponentModel>(typeof(SComponentModel));
            _oleServiceProvider = AppShell.Current.GetGlobalService<IServiceProvider>();

            ToolBar = new CommandID(RGuidList.RCmdSetGuid, RPackageCommandId.historyWindowToolBarId);
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
            var history = _historyProviderExport.Value.GetAssociatedRHistory(TextView);
            return new HistorySearchTask(dwCookie, history, pSearchQuery, pSearchCallback);
        }

        public override void ClearSearch() {
            var history = _historyProviderExport.Value.GetAssociatedRHistory(TextView);
            EditorShell.Current.DispatchOnUIThread(() => history.ClearFilter(), DispatcherPriority.Normal);
            base.ClearSearch();
        }

        protected override void Dispose(bool disposing) {
            if (disposing) {
                _vsTextViewAdapter = null;
                _oleServiceProvider = null;
                _commandTarget = null;
            }
            base.Dispose(disposing);
        }

        private sealed class HistorySearchTask : VsSearchTask {
            private readonly IRHistory _history;

            public HistorySearchTask(uint dwCookie, IRHistory history, IVsSearchQuery pSearchQuery, IVsSearchCallback pSearchCallback)
                : base(dwCookie, pSearchQuery, pSearchCallback) {
                _history = history;
            }

            protected override void OnStartSearch() {
                base.OnStartSearch();
                EditorShell.Current.DispatchOnUIThread(() => _history.Filter(SearchQuery.SearchString), DispatcherPriority.Normal);
            }
        }
    }
}
