using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.Shell.Mocks {
    public sealed class VsUiShellMock : IVsUIShell {
        public int AddNewBFNavigationItem(IVsWindowFrame pWindowFrame, string bstrData, object punk, int fReplaceCurrent) {
            throw new NotImplementedException();
        }

        public int CenterDialogOnWindow(IntPtr hwndDialog, IntPtr hwndParent) {
            throw new NotImplementedException();
        }

        public int CreateDocumentWindow(uint grfCDW, string pszMkDocument, IVsUIHierarchy pUIH, uint itemid, IntPtr punkDocView, IntPtr punkDocData, ref Guid rguidEditorType, string pszPhysicalView, ref Guid rguidCmdUI, OLE.Interop.IServiceProvider psp, string pszOwnerCaption, string pszEditorCaption, int[] pfDefaultPosition, out IVsWindowFrame ppWindowFrame) {
            throw new NotImplementedException();
        }

        public int CreateToolWindow(uint grfCTW, uint dwToolWindowId, object punkTool, ref Guid rclsidTool, ref Guid rguidPersistenceSlot, ref Guid rguidAutoActivate, OLE.Interop.IServiceProvider psp, string pszCaption, int[] pfDefaultPosition, out IVsWindowFrame ppWindowFrame) {
            throw new NotImplementedException();
        }

        public int EnableModeless(int fEnable) {
            throw new NotImplementedException();
        }

        public int FindToolWindow(uint grfFTW, ref Guid rguidPersistenceSlot, out IVsWindowFrame ppWindowFrame) {
            throw new NotImplementedException();
        }

        public int FindToolWindowEx(uint grfFTW, ref Guid rguidPersistenceSlot, uint dwToolWinId, out IVsWindowFrame ppWindowFrame) {
            throw new NotImplementedException();
        }

        public int GetAppName(out string pbstrAppName) {
            throw new NotImplementedException();
        }

        public int GetCurrentBFNavigationItem(out IVsWindowFrame ppWindowFrame, out string pbstrData, out object ppunk) {
            throw new NotImplementedException();
        }

        public int GetDialogOwnerHwnd(out IntPtr phwnd) {
            throw new NotImplementedException();
        }

        public int GetDirectoryViaBrowseDlg(VSBROWSEINFOW[] pBrowse) {
            throw new NotImplementedException();
        }

        public int GetDocumentWindowEnum(out IEnumWindowFrames ppenum) {
            throw new NotImplementedException();
        }

        public int GetErrorInfo(out string pbstrErrText) {
            throw new NotImplementedException();
        }

        public int GetNextBFNavigationItem(out IVsWindowFrame ppWindowFrame, out string pbstrData, out object ppunk) {
            throw new NotImplementedException();
        }

        public int GetOpenFileNameViaDlg(VSOPENFILENAMEW[] pOpenFileName) {
            throw new NotImplementedException();
        }

        public int GetPreviousBFNavigationItem(out IVsWindowFrame ppWindowFrame, out string pbstrData, out object ppunk) {
            throw new NotImplementedException();
        }

        public int GetSaveFileNameViaDlg(VSSAVEFILENAMEW[] pSaveFileName) {
            throw new NotImplementedException();
        }

        public int GetToolWindowEnum(out IEnumWindowFrames ppenum) {
            throw new NotImplementedException();
        }

        public int GetURLViaDlg(string pszDlgTitle, string pszStaticLabel, string pszHelpTopic, out string pbstrURL) {
            throw new NotImplementedException();
        }

        public int GetVSSysColor(VSSYSCOLOR dwSysColIndex, out uint pdwRGBval) {
            throw new NotImplementedException();
        }

        public int OnModeChange(DBGMODE dbgmodeNew) {
            throw new NotImplementedException();
        }

        public int PostExecCommand(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, ref object pvaIn) {
            throw new NotImplementedException();
        }

        public int PostSetFocusMenuCommand(ref Guid pguidCmdGroup, uint nCmdID) {
            throw new NotImplementedException();
        }

        public int RefreshPropertyBrowser(int dispid) {
            throw new NotImplementedException();
        }

        public int RemoveAdjacentBFNavigationItem(RemoveBFDirection rdDir) {
            throw new NotImplementedException();
        }

        public int RemoveCurrentNavigationDupes(RemoveBFDirection rdDir) {
            throw new NotImplementedException();
        }

        public int ReportErrorInfo(int hr) {
            throw new NotImplementedException();
        }

        public int SaveDocDataToFile(VSSAVEFLAGS grfSave, object pPersistFile, string pszUntitledPath, out string pbstrDocumentNew, out int pfCanceled) {
            throw new NotImplementedException();
        }

        public int SetErrorInfo(int hr, string pszDescription, uint dwReserved, string pszHelpKeyword, string pszSource) {
            throw new NotImplementedException();
        }

        public int SetForegroundWindow() {
            throw new NotImplementedException();
        }

        public int SetMRUComboText(ref Guid pguidCmdGroup, uint dwCmdID, string lpszText, int fAddToList) {
            throw new NotImplementedException();
        }

        public int SetMRUComboTextW(Guid[] pguidCmdGroup, uint dwCmdID, string pwszText, int fAddToList) {
            throw new NotImplementedException();
        }

        public int SetToolbarVisibleInFullScreen(Guid[] pguidCmdGroup, uint dwToolbarId, int fVisibleInFullScreen) {
            throw new NotImplementedException();
        }

        public int SetupToolbar(IntPtr hwnd, IVsToolWindowToolbar ptwt, out IVsToolWindowToolbarHost pptwth) {
            throw new NotImplementedException();
        }

        public int SetWaitCursor() {
            throw new NotImplementedException();
        }

        public int ShowContextMenu(uint dwCompRole, ref Guid rclsidActive, int nMenuId, POINTS[] pos, IOleCommandTarget pCmdTrgtActive) {
            throw new NotImplementedException();
        }

        public int ShowMessageBox(uint dwCompRole, ref Guid rclsidComp, string pszTitle, string pszText, string pszHelpFile, uint dwHelpContextID, OLEMSGBUTTON msgbtn, OLEMSGDEFBUTTON msgdefbtn, OLEMSGICON msgicon, int fSysAlert, out int pnResult) {
            throw new NotImplementedException();
        }

        public int TranslateAcceleratorAsACmd(MSG[] pMsg) {
            throw new NotImplementedException();
        }

        public int UpdateCommandUI(int fImmediateUpdate) {
            throw new NotImplementedException();
        }

        public int UpdateDocDataIsDirtyFeedback(uint docCookie, int fDirty) {
            throw new NotImplementedException();
        }
    }
}
