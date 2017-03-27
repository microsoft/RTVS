// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Windows;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.UI.Commands;
using Microsoft.R.Components.View;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Package.Shell {
    public sealed class VisualComponentToolWindowAdapter<T> : IVisualComponentContainer<T> where T : IVisualComponent {
        private readonly ToolWindowPane _toolWindowPane;
        private readonly IServiceContainer _services;
        private IVsWindowFrame _vsWindowFrame;

        public VisualComponentToolWindowAdapter(ToolWindowPane toolWindowPane, IServiceContainer services) {
            _toolWindowPane = toolWindowPane;
            _services = services;
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
                _services.MainThread().Assert();
                string text;
                var statusBar = _services.GetService<IVsStatusbar>(typeof(SVsStatusbar));
                ErrorHandler.ThrowOnFailure(statusBar.GetText(out text));
                return text;
            }
            set {
                _services.MainThread().Assert();
                var statusBar = _services.GetService<IVsStatusbar>(typeof(SVsStatusbar));
                statusBar.SetText(value);
            }
        }

        public void ShowContextMenu(CommandId commandId, Point position) {
            _services.MainThread().Post(() => {
                var point = Component.Control.PointToScreen(position);
                _services.ShowContextMenu(commandId, (int)point.X, (int)point.Y);
            });
        }

        public void UpdateCommandStatus(bool immediate) {
            _services.MainThread().Post(() => {
                var shell = _services.GetService<IVsUIShell>(typeof (SVsUIShell));
                shell.UpdateCommandUI(immediate ? 1 : 0);
            });
        }

        public void Hide() {
            if (VsWindowFrame == null) {
                return;
            }

            _services.MainThread().Post(() => {
                ErrorHandler.ThrowOnFailure(VsWindowFrame.Hide());
            });
        }

        public void Show(bool focus, bool immediate) {
            if (VsWindowFrame == null) {
                return;
            }

            if (immediate) {
                _services.MainThread().Assert();
                if (focus) {
                    ErrorHandler.ThrowOnFailure(VsWindowFrame.Show());
                    Component.Control?.Focus();
                } else {
                    ErrorHandler.ThrowOnFailure(VsWindowFrame.ShowNoActivate());
                }
            } else {
                _services.MainThread().Post(() => {
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