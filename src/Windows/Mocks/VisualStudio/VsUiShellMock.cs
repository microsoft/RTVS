// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.Shell.Mocks {
    [ExcludeFromCodeCoverage]
    public sealed class VsUiShellMock : IVsUIShell {
        private Dictionary<Guid, VsWindowFrameMock> _frames = new Dictionary<Guid, VsWindowFrameMock>();

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
            var mock = new VsWindowFrameMock(pszCaption);
            _frames[rguidPersistenceSlot] = mock;
            ppWindowFrame = mock;
            return VSConstants.S_OK;
        }

        public int EnableModeless(int fEnable) {
            return VSConstants.S_OK;
        }

        public int FindToolWindow(uint grfFTW, ref Guid rguidPersistenceSlot, out IVsWindowFrame ppWindowFrame) {
            VsWindowFrameMock mock;
            _frames.TryGetValue(rguidPersistenceSlot, out mock);
            if (mock == null && grfFTW == (uint)__VSFINDTOOLWIN.FTW_fForceCreate) {
                Guid g = Guid.Empty;
                CreateToolWindow(0, 1, null, ref g, ref rguidPersistenceSlot, ref g, null, string.Empty, null, out ppWindowFrame);
            } else {
                ppWindowFrame = mock;
            }
            return ppWindowFrame != null ? VSConstants.S_OK : VSConstants.E_FAIL;
        }

        public int FindToolWindowEx(uint grfFTW, ref Guid rguidPersistenceSlot, uint dwToolWinId, out IVsWindowFrame ppWindowFrame) {
            return FindToolWindow(grfFTW, ref rguidPersistenceSlot, out ppWindowFrame);
        }

        public int GetAppName(out string pbstrAppName) {
            pbstrAppName = "RTVS";
            return VSConstants.S_OK;
        }

        public int GetCurrentBFNavigationItem(out IVsWindowFrame ppWindowFrame, out string pbstrData, out object ppunk) {
            throw new NotImplementedException();
        }

        public int GetDialogOwnerHwnd(out IntPtr phwnd) {
            phwnd = IntPtr.Zero;
            return VSConstants.S_OK;
        }

        public int GetDirectoryViaBrowseDlg(VSBROWSEINFOW[] pBrowse) {
            throw new NotImplementedException();
        }

        public int GetDocumentWindowEnum(out IEnumWindowFrames ppenum) {
            throw new NotImplementedException();
        }

        public int GetErrorInfo(out string pbstrErrText) {
            pbstrErrText = string.Empty;
            return VSConstants.S_OK;
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
            ppenum = new EnumWindowFramesMock(new List<IVsWindowFrame>(_frames.Values));
            return VSConstants.S_OK;
        }

        public int GetURLViaDlg(string pszDlgTitle, string pszStaticLabel, string pszHelpTopic, out string pbstrURL) {
            throw new NotImplementedException();
        }

        public int GetVSSysColor(VSSYSCOLOR dwSysColIndex, out uint pdwRGBval) {
            pdwRGBval = 0;
            return VSConstants.S_OK;
        }

        public int OnModeChange(DBGMODE dbgmodeNew) {
            return VSConstants.S_OK;
        }

        public int PostExecCommand(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, ref object pvaIn) {
            return VSConstants.S_OK;
        }

        public int PostSetFocusMenuCommand(ref Guid pguidCmdGroup, uint nCmdID) {
            return VSConstants.S_OK;
        }

        public int RefreshPropertyBrowser(int dispid) {
            return VSConstants.S_OK;
        }

        public int RemoveAdjacentBFNavigationItem(RemoveBFDirection rdDir) {
            throw new NotImplementedException();
        }

        public int RemoveCurrentNavigationDupes(RemoveBFDirection rdDir) {
            throw new NotImplementedException();
        }

        public int ReportErrorInfo(int hr) {
            return VSConstants.S_OK;
        }

        public int SaveDocDataToFile(VSSAVEFLAGS grfSave, object pPersistFile, string pszUntitledPath, out string pbstrDocumentNew, out int pfCanceled) {
            throw new NotImplementedException();
        }

        public int SetErrorInfo(int hr, string pszDescription, uint dwReserved, string pszHelpKeyword, string pszSource) {
            return VSConstants.S_OK;
        }

        public int SetForegroundWindow() {
            return VSConstants.S_OK;
        }

        public int SetMRUComboText(ref Guid pguidCmdGroup, uint dwCmdID, string lpszText, int fAddToList) {
            return VSConstants.S_OK;
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
            return VSConstants.S_OK;
        }

        public int ShowContextMenu(uint dwCompRole, ref Guid rclsidActive, int nMenuId, POINTS[] pos, IOleCommandTarget pCmdTrgtActive) {
            return VSConstants.S_OK;
        }

        public int ShowMessageBox(uint dwCompRole, ref Guid rclsidComp, string pszTitle, string pszText, string pszHelpFile, uint dwHelpContextID, OLEMSGBUTTON msgbtn, OLEMSGDEFBUTTON msgdefbtn, OLEMSGICON msgicon, int fSysAlert, out int pnResult) {
            pnResult = 0;
            return VSConstants.S_OK;
        }

        public int TranslateAcceleratorAsACmd(MSG[] pMsg) {
            return VSConstants.S_FALSE;
        }

        public int UpdateCommandUI(int fImmediateUpdate) {
            return VSConstants.S_OK;
        }

        public int UpdateDocDataIsDirtyFeedback(uint docCookie, int fDirty) {
            return VSConstants.S_OK;
        }
    }
}
