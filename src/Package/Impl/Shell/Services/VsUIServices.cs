// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.UI;
using Microsoft.Common.Core.UI.Commands;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.R.Package.Interop;
using Microsoft.VisualStudio.R.Packages.R;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using static System.FormattableString;
using VsPackage = Microsoft.VisualStudio.Shell.Package;

namespace Microsoft.VisualStudio.R.Package.Shell {
    public sealed class VsUIServices : IUIService, IDisposable, IVsBroadcastMessageEvents {
        private const int WM_SYSCOLORCHANGE = 0x15;

        private readonly ICoreShell _coreShell;
        private readonly IVsShell _vsShell;
        private readonly IVsUIShell _uiShell;
        private uint _vsShellBroadcastEventsCookie;

        public VsUIServices(ICoreShell coreShell) {
            ProgressDialog = new VsProgressDialog(coreShell.Services);
            FileDialog = new VsFileDialog(coreShell);

            _coreShell = coreShell;
            _vsShell = VsPackage.GetGlobalService(typeof(SVsShell)) as IVsShell;
            _vsShell.AdviseBroadcastMessages(this, out _vsShellBroadcastEventsCookie);
            _uiShell = VsPackage.GetGlobalService(typeof(SVsUIShell)) as IVsUIShell;
        }

        #region IUIServices
        public event EventHandler<EventArgs> UIThemeChanged;
        public IProgressDialog ProgressDialog { get; }
        public IFileDialog FileDialog { get; }

        public UIColorTheme UIColorTheme {
            get {
                var defaultBackground = VSColorTheme.GetThemedColor(EnvironmentColors.ToolWindowBackgroundColorKey);
                return defaultBackground.GetBrightness() < 0.5 ? UIColorTheme.Dark : UIColorTheme.Light;
            }
        }

        /// <summary>
        /// Displays error message in a host-specific UI
        /// </summary>
        public void ShowErrorMessage(string message) {
            int result;
            _uiShell.ShowMessageBox(0, Guid.Empty, null, message, null, 0,
                OLEMSGBUTTON.OLEMSGBUTTON_OK, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST, OLEMSGICON.OLEMSGICON_CRITICAL, 0, out result);
        }

        public void ShowContextMenu(CommandId commandId, int x, int y, object commandTarget = null) {
            if (commandTarget == null) {
                var package = VsAppShell.EnsurePackageLoaded(RGuidList.RPackageGuid);
                if (package != null) {
                    var sp = (IServiceProvider)package;
                    var menuService = (System.ComponentModel.Design.IMenuCommandService)sp
                        .GetService(typeof(System.ComponentModel.Design.IMenuCommandService));
                    menuService.ShowContextMenu(new System.ComponentModel.Design.CommandID(commandId.Group, commandId.Id), x, y);
                }
            } else {
                var target = commandTarget as ICommandTarget;
                if (target == null) {
                    throw new ArgumentException(Invariant($"{nameof(commandTarget)} must implement ICommandTarget"));
                }
                var pts = new POINTS[1];
                pts[0].x = (short)x;
                pts[0].y = (short)y;
                _uiShell.ShowContextMenu(0, commandId.Group, commandId.Id, pts, new CommandTargetToOleShim(null, target));
            }
        }

        /// <summary>
        /// Displays question in a host-specific UI
        /// </summary>
        public MessageButtons ShowMessage(string message, MessageButtons buttons, MessageType messageType = MessageType.Information) {
            int result;
            var oleButtons = GetOleButtonFlags(buttons);
            OLEMSGICON oleIcon;

            switch (messageType) {
                case MessageType.Information:
                    oleIcon = buttons == MessageButtons.OK ? OLEMSGICON.OLEMSGICON_INFO : OLEMSGICON.OLEMSGICON_QUERY;
                    break;
                case MessageType.Warning:
                    oleIcon = OLEMSGICON.OLEMSGICON_WARNING;
                    break;
                default:
                    oleIcon = OLEMSGICON.OLEMSGICON_CRITICAL;
                    break;
            }

            _uiShell.ShowMessageBox(0, Guid.Empty, null, message, null, 0,
                oleButtons, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST, oleIcon, 0, out result);

            switch (result) {
                case NativeMethods.IDYES:
                    return MessageButtons.Yes;
                case NativeMethods.IDNO:
                    return MessageButtons.No;
                case NativeMethods.IDCANCEL:
                    return MessageButtons.Cancel;
            }
            return MessageButtons.OK;
        }

        public string SaveFileIfDirty(string fullPath) 
            => new RunningDocumentTable(RPackage.Current).SaveFileIfDirty(fullPath);
        public void UpdateCommandStatus(bool immediate) 
            => _coreShell.MainThread().Post(() => { _uiShell.UpdateCommandUI(immediate ? 1 : 0); });
        #endregion

        #region IVsBroadcastMessageEvents
        public int OnBroadcastMessage(uint msg, IntPtr wParam, IntPtr lParam) {
            if (msg == WM_SYSCOLORCHANGE) {
                UIThemeChanged?.Invoke(this, EventArgs.Empty);
            }
            return VSConstants.S_OK;
        }
        #endregion

        #region IDisposable
        public void Dispose() {
            if (_vsShell != null) {
                if (_vsShellBroadcastEventsCookie != 0) {
                    _vsShell.UnadviseBroadcastMessages(_vsShellBroadcastEventsCookie);
                    _vsShellBroadcastEventsCookie = 0;
                }
            }
        }
        #endregion

        private static OLEMSGBUTTON GetOleButtonFlags(MessageButtons buttons) {
            switch (buttons) {
                case MessageButtons.YesNoCancel:
                    return OLEMSGBUTTON.OLEMSGBUTTON_YESNOCANCEL;
                case MessageButtons.YesNo:
                    return OLEMSGBUTTON.OLEMSGBUTTON_YESNO;
                case MessageButtons.OKCancel:
                    return OLEMSGBUTTON.OLEMSGBUTTON_OKCANCEL;
            }
            return OLEMSGBUTTON.OLEMSGBUTTON_OK;
        }
    }
}
