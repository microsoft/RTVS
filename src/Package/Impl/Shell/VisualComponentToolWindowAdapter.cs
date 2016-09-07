// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Design;
using System.Windows;
using Microsoft.R.Components.View;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.R.Components.Extensions;

namespace Microsoft.VisualStudio.R.Package.Shell {
    public class VisualComponentToolWindowAdapter<T> : IVisualComponentContainer<T> where T : IVisualComponent {
        private readonly ToolWindowPane _toolWindowPane;
        private IVsWindowFrame _vsWindowFrame;

        public VisualComponentToolWindowAdapter(ToolWindowPane toolWindowPane) {
            _toolWindowPane = toolWindowPane;
        }

        public T Component { get; set; }

        public bool IsOnScreen {
            get {
                if (VsWindowFrame == null) {
                    return false;
                }

                int onScreen;
                return VsWindowFrame.IsOnScreen(out onScreen) == VSConstants.S_OK && onScreen != 0;
            }
        }

        private IVsWindowFrame VsWindowFrame => _vsWindowFrame ?? (_vsWindowFrame = _toolWindowPane.Frame as IVsWindowFrame);

        public string CaptionText
        {
            get { return _toolWindowPane.Caption; }
            set { _toolWindowPane.Caption = value; }
        }

        public string StatusText
        {
            get {
                VsAppShell.Current.AssertIsOnMainThread();
                string text = string.Empty;
                var statusBar = VsAppShell.Current.GetGlobalService<IVsStatusbar>(typeof(SVsStatusbar));
                ErrorHandler.ThrowOnFailure(statusBar.GetText(out text));
                return text;
            }
            set {
                VsAppShell.Current.AssertIsOnMainThread();
                var statusBar = VsAppShell.Current.GetGlobalService<IVsStatusbar>(typeof(SVsStatusbar));
                statusBar.SetText(value);
            }
        }

        public void ShowContextMenu(CommandID commandId, Point position) {
            VsAppShell.Current.DispatchOnUIThread(() => {
                var point = Component.Control.PointToScreen(position);
                VsAppShell.Current.ShowContextMenu(commandId, (int)point.X, (int)point.Y);
            });
        }

        public void UpdateCommandStatus(bool immediate) {
            VsAppShell.Current.DispatchOnUIThread(() => {
                var shell = VsAppShell.Current.GetGlobalService<IVsUIShell>(typeof (SVsUIShell));
                shell.UpdateCommandUI(immediate ? 1 : 0);
            });
        }

        public void Hide() {
            if (VsWindowFrame == null) {
                return;
            }

            VsAppShell.Current.DispatchOnUIThread(() => {
                ErrorHandler.ThrowOnFailure(VsWindowFrame.Hide());
            });
        }

        public void Show(bool focus, bool immediate) {
            if (VsWindowFrame == null) {
                return;
            }

            if (immediate) {
                VsAppShell.Current.AssertIsOnMainThread();
                if (focus) {
                    ErrorHandler.ThrowOnFailure(VsWindowFrame.Show());
                    Component.Control?.Focus();
                } else {
                    ErrorHandler.ThrowOnFailure(VsWindowFrame.ShowNoActivate());
                }
            } else {
                VsAppShell.Current.DispatchOnUIThread(() => {
                    if (focus) {
                        ErrorHandler.ThrowOnFailure(VsWindowFrame.Show());
                        Component.Control?.Focus();
                    } else {
                        ErrorHandler.ThrowOnFailure(VsWindowFrame.ShowNoActivate());
                    }
                });
            }
        }
    }
}