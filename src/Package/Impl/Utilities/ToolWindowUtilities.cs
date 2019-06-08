// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Windows.Threading;
using Microsoft.Common.Core.Threading;
using Microsoft.VisualStudio.R.Packages.R;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Package.Utilities {
    public static class ToolWindowUtilities {
        /// <summary>
        /// Locates the specified window pane (tool window). Does not create one.
        /// </summary>
        public static T FindWindowPane<T>(int id) where T : ToolWindowPane {
            if (RPackage.Current != null) {
                return RPackage.Current.FindWindowPane<T>(typeof(T), id, false) as T;
            }
            return null;
        }

        public static T ShowWindowPane<T>(int id, bool focus) where T : ToolWindowPane {
            T window = RPackage.Current.FindWindowPane<T>(typeof(T), id, true) as T;
            if (window == null) {
                return null; 
            }

            return TryShowToolWindow(window, focus) ? null : window;
        }

        public static void ShowToolWindow(ToolWindowPane toolWindow, IMainThread mainThread, bool focus, bool immediate) {
            if (immediate) {
                mainThread.Assert();
                TryShowToolWindow(toolWindow, focus);
            } else {
                mainThread.Post(() => TryShowToolWindow(toolWindow, focus));
            }
        }

        private static bool TryShowToolWindow(ToolWindowPane toolWindow, bool focus) {
            Dispatcher.CurrentDispatcher.VerifyAccess();
            var frame = toolWindow.Frame as IVsWindowFrame;
            if (frame == null) {
                return true;
            }

            if (focus) {
                ErrorHandler.ThrowOnFailure(frame.Show());
                var content = toolWindow.Content as System.Windows.UIElement;
                content?.Focus();
            } else {
                ErrorHandler.ThrowOnFailure(frame.ShowNoActivate());
            }

            return false;
        }

        public static void CreateToolWindow(IVsUIShell vsUiShell, ToolWindowPane toolWindow, int instanceId) {
            Dispatcher.CurrentDispatcher.VerifyAccess();
            var clsId = toolWindow.ToolClsid;
            var typeId = toolWindow.GetType().GUID;
            var guidAutoActivate = Guid.Empty;
            var caption = toolWindow.Caption;
            IVsWindowFrame frame;

            toolWindow.Package = RPackage.Current;

            ErrorHandler.ThrowOnFailure(
                vsUiShell.CreateToolWindow(
                    (uint)(__VSCREATETOOLWIN.CTW_fInitNew | __VSCREATETOOLWIN.CTW_fToolbarHost | __VSCREATETOOLWIN.CTW_fForceCreate),
                    (uint)instanceId,
                    toolWindow,
                    ref clsId,
                    ref typeId,
                    ref guidAutoActivate,
                    null,
                    caption,
                    null,
                    out frame
                )
            );

            toolWindow.Frame = frame;
            SetToolbarToHost(frame, toolWindow);
        }

        public static void SetToolbarToHost(IVsWindowFrame frame, ToolWindowPane toolWindow) {
            Dispatcher.CurrentDispatcher.VerifyAccess();
            if (toolWindow.ToolBar == null) {
                return;
            }

            var toolBarHost = GetToolbarHost(frame);
            if (toolBarHost == null) {
                return;
            }

            var toolBarCommandSet = toolWindow.ToolBar.Guid;
            ErrorHandler.ThrowOnFailure(toolBarHost.AddToolbar3((VSTWT_LOCATION)toolWindow.ToolBarLocation, ref toolBarCommandSet, (uint)toolWindow.ToolBar.ID, toolWindow.ToolBarDropTarget, toolWindow.ToolBarCommandTarget));
            toolWindow.OnToolBarAdded();
        }

        public static IVsToolWindowToolbarHost3 GetToolbarHost(IVsWindowFrame frame) {
            Dispatcher.CurrentDispatcher.VerifyAccess();
            object result;
            ErrorHandler.ThrowOnFailure(frame.GetProperty((int)__VSFPROPID.VSFPROPID_ToolbarHost, out result));
            return (IVsToolWindowToolbarHost3)result;
        }
    }
}
