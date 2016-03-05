// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Packages.R;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Package.Utilities {
    public static class ToolWindowUtilities {
        public static T FindWindowPane<T>(int id) where T : ToolWindowPane {
            if (RPackage.Current != null) {
                return RPackage.Current.FindWindowPane<T>(typeof(T), id, true) as T;
            }
            return null;
        }

        public static IVsWindowFrame FindToolWindow(Guid guid) {
            var uiShell = VsAppShell.Current.GetGlobalService<IVsUIShell>(typeof(SVsUIShell));
            IVsWindowFrame frame = null;
            uiShell.FindToolWindow((uint)__VSFINDTOOLWIN.FTW_fFindFirst, ref guid, out frame);
            return frame;
        }

        public static T ShowWindowPane<T>(int id, bool focus) where T : ToolWindowPane {
            T window = RPackage.Current.FindWindowPane<T>(typeof(T), id, true) as T;
            if (window == null) {
                return null;
            }

            var frame = window.Frame as IVsWindowFrame;
            if (frame == null) {
                return window;
            }

            if (focus) {
                ErrorHandler.ThrowOnFailure(frame.Show());
                var content = window.Content as System.Windows.UIElement;
                content?.Focus();
            } else {
                ErrorHandler.ThrowOnFailure(frame.ShowNoActivate());
            }
            return window;
        }

        public static void CreateToolWindow(IVsUIShell vsUiShell, ToolWindowPane toolWindow, int instanceId) {
            var clsId = toolWindow.ToolClsid;
            var typeId = toolWindow.GetType().GUID;
            var guidAutoActivate = Guid.Empty;
            var caption = toolWindow.Caption;
            IVsWindowFrame frame;

            toolWindow.Package = RPackage.Current;

            ErrorHandler.ThrowOnFailure(
                vsUiShell.CreateToolWindow(
                    (uint)(__VSCREATETOOLWIN.CTW_fInitNew | __VSCREATETOOLWIN.CTW_fToolbarHost),
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
            var toolBarHost = GetToolbarHost(frame);
            if (toolBarHost == null) {
                return;
            }

            var toolBarCommandSet = toolWindow.ToolBar.Guid;
            ErrorHandler.ThrowOnFailure(toolBarHost.AddToolbar3((VSTWT_LOCATION)toolWindow.ToolBarLocation, ref toolBarCommandSet, (uint)toolWindow.ToolBar.ID, toolWindow.ToolBarDropTarget, toolWindow.ToolBarCommandTarget));
            toolWindow.OnToolBarAdded();
        }

        public static IVsToolWindowToolbarHost3 GetToolbarHost(IVsWindowFrame frame) {
            object result;
            ErrorHandler.ThrowOnFailure(frame.GetProperty((int)__VSFPROPID.VSFPROPID_ToolbarHost, out result));
            return (IVsToolWindowToolbarHost3)result;
        }
    }
}
