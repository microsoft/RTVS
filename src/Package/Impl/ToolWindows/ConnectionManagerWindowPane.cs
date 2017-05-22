// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Runtime.InteropServices;
using Microsoft.Common.Core.Services;
using Microsoft.R.Components.ConnectionManager;
using Microsoft.R.Components.ConnectionManager.Implementation;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Constants = Microsoft.VisualStudio.OLE.Interop.Constants;

namespace Microsoft.VisualStudio.R.Package.ToolWindows {
    [Guid(WindowGuidString)]
    internal class ConnectionManagerWindowPane : VisualComponentToolWindow<IConnectionManagerVisualComponent>, IOleCommandTarget {
        private readonly IConnectionManager _connectionManager;
        public const string WindowGuidString = "75753398-BE0E-442E-900C-E775EAC1FAC2";
        public static Guid WindowGuid { get; } = new Guid(WindowGuidString);

        private IOleCommandTarget _commandTarget;

        public ConnectionManagerWindowPane(IConnectionManager connectionManager, IServiceContainer services): base(services) {
            _connectionManager = connectionManager;
            BitmapImageMoniker = KnownMonikers.ImmediateWindow;
            Caption = Resources.WorkspacesWindowCaption;
        }

        protected override void OnCreate() {
            Component = new ConnectionManagerVisualComponent(_connectionManager, this, Services);
            base.OnCreate();
        }

        public override void OnToolWindowCreated() {
            Guid commandUiGuid = VSConstants.GUID_TextEditorFactory;
            ((IVsWindowFrame)Frame).SetGuidProperty((int)__VSFPROPID.VSFPROPID_InheritKeyBindings, ref commandUiGuid);
            base.OnToolWindowCreated();
        }

        protected override void Dispose(bool disposing) {
            if (disposing && Component != null) {
                Component.Dispose();
                Component = null;
                _commandTarget = null;
            }
            base.Dispose(disposing);
        }

        public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText) {
            return _commandTarget?.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText) ?? (int)Constants.OLECMDERR_E_NOTSUPPORTED;
        }

        public int Exec(ref Guid pguidCmdGroup, uint nCmdId, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut) {
            return _commandTarget?.Exec(ref pguidCmdGroup, nCmdId, nCmdexecopt, pvaIn, pvaOut) ?? (int)Constants.OLECMDERR_E_NOTSUPPORTED;
        }
    }
}
