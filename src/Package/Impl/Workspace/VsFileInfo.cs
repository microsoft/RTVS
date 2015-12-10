using Microsoft.Languages.Editor.Controller;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Packages.R;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Microsoft.VisualStudio.R.Package.Workspace {
    public sealed class VsFileInfo {
        private string _cachedCaption;
        private IVsHierarchy _hierarchy = null;
        private VSConstants.VSITEMID _hierarchyItemId = VSConstants.VSITEMID.Nil;

        private VsFileInfo(string filePath) {
            FilePath = filePath;
            UpdateRunningDocumentInfo();
        }

        public static VsFileInfo FromPath(string filePath) {
            return new VsFileInfo(filePath);
        }

        public static VsFileInfo FromTextView(ITextView textView) {
            string fileName = GetFileName(textView);
            return FromPath(fileName);
        }

        public static VsFileInfo FromTextBuffer(ITextBuffer textBuffer) {
            TextViewData data = TextViewConnectionListener.GetTextViewDataForBuffer(textBuffer);
            ITextView textView = (data != null) ? data.LastActiveView : null;
            return FromTextView(textView); // it's OK for the view to be null
        }

        public string FilePath { get; private set; }

        /// <summary>
        /// Document cookie in IVsRunningDocumentTable
        /// </summary>
        public uint RunningDocumentItemCookie { get; private set; }

        public IVsHierarchy Hierarchy {
            get {
                if (_hierarchy == null) {
                    UpdateRunningDocumentInfo();
                }

                return _hierarchy;
            }
            private set {
                _hierarchy = value;
            }
        }
        public VSConstants.VSITEMID HierarchyItemId // used with IVsHierarchy
        {
            get {
                if (_hierarchyItemId == VSConstants.VSITEMID.Nil) {
                    UpdateRunningDocumentInfo();
                }

                return _hierarchyItemId;
            }
            private set {
                _hierarchyItemId = value;
            }
        }

        public string Caption {
            get {
                if (_cachedCaption == null) {
                    // The default caption is the file path
                    _cachedCaption = FilePath ?? string.Empty;

                    if (Hierarchy != null && HierarchyItemId != 0 && !string.IsNullOrEmpty(FilePath)) {
                        object isUnsaved = null;

                        if (ErrorHandler.Succeeded(Hierarchy.GetProperty(
                                (uint)HierarchyItemId, (int)__VSHPROPID.VSHPROPID_IsNewUnsavedItem, out isUnsaved)) &&
                            isUnsaved is bool && (bool)isUnsaved) {
                            object caption = null;

                            if (ErrorHandler.Succeeded(Hierarchy.GetProperty(
                                    (uint)HierarchyItemId, (int)__VSHPROPID.VSHPROPID_Caption, out caption)) &&
                                caption is string) {
                                _cachedCaption = (string)caption;
                            }
                        }
                    }
                }

                return _cachedCaption;
            }
        }

        public static string GetFileName(ITextView textView) {
            var componentModel = ServiceProvider.GlobalProvider.GetService(typeof(SComponentModel)) as IComponentModel;
            var adapterService = componentModel.GetService<IVsEditorAdaptersFactoryService>();

            if (adapterService != null && textView != null) {
                IVsTextView vsTextView = adapterService.GetViewAdapter(textView);
                IVsTextLines vsTextLines = null;

                if (vsTextView != null && ErrorHandler.Succeeded(vsTextView.GetBuffer(out vsTextLines))) {
                    IPersistFileFormat persistFile = vsTextLines as IPersistFileFormat;

                    if (persistFile != null) {
                        string fileName = null;
                        uint formatIndex = 0;

                        if (ErrorHandler.Succeeded(persistFile.GetCurFile(out fileName, out formatIndex))) {
                            return fileName;
                        }
                    }
                }
            }

            return string.Empty;
        }

        private void UpdateRunningDocumentInfo() {
            if (!string.IsNullOrEmpty(FilePath)) {
                RunningDocumentTable rdt = new RunningDocumentTable(RPackage.Current);
                RunningDocumentInfo docInfo = rdt.GetDocumentInfo(FilePath);
                RunningDocumentItemCookie = docInfo.DocCookie;
                if (docInfo.IsHierarchyInitialized) {
                    // The doc is initialized, now it is safe to read the other attributes like item id. 
                    Hierarchy = docInfo.Hierarchy;
                    HierarchyItemId = (VSConstants.VSITEMID)docInfo.ItemId;
                }
            }
        }
    }
}
