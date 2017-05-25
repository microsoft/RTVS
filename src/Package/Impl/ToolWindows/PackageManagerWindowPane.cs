// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Runtime.InteropServices;
using Microsoft.Common.Core.Services;
using Microsoft.R.Components.PackageManager;
using Microsoft.R.Components.PackageManager.Implementation;
using Microsoft.R.Components.Search;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Constants = Microsoft.VisualStudio.OLE.Interop.Constants;

namespace Microsoft.VisualStudio.R.Package.ToolWindows {
    [Guid(WindowGuidString)]
    internal class PackageManagerWindowPane : VisualComponentToolWindow<IRPackageManagerVisualComponent>, IOleCommandTarget {
        public const string WindowGuidString = "363F84AD-3397-4FDE-97EA-1ABD73C64BB3";

        private readonly IRPackageManager _packageManager;
        private readonly ISearchControlProvider _searchControlProvider;
        public static Guid WindowGuid { get; } = new Guid(WindowGuidString);

        private IOleCommandTarget _commandTarget;

        public PackageManagerWindowPane(IRPackageManager packageManager, ISearchControlProvider searchControlProvider, IServiceContainer services): base(services) {
            _packageManager = packageManager;
            _searchControlProvider = searchControlProvider;
            BitmapImageMoniker = KnownMonikers.Package;
            Caption = Resources.PackageManagerWindowCaption;
        }

        protected override void OnCreate() {
            Component = new RPackageManagerVisualComponent(_packageManager, this, _searchControlProvider, Services);
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

        public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
            => _commandTarget?.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText) ?? (int)Constants.OLECMDERR_E_NOTSUPPORTED;

        public int Exec(ref Guid pguidCmdGroup, uint nCmdId, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
            => _commandTarget?.Exec(ref pguidCmdGroup, nCmdId, nCmdexecopt, pvaIn, pvaOut) ?? (int)Constants.OLECMDERR_E_NOTSUPPORTED;
    }
}
