// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Runtime.InteropServices;
using System.Windows;

namespace Microsoft.Markdown.Editor.Preview {
    internal static class NativeMethods {
        [ComImport, Guid("BD3F23C0-D43E-11CF-893B-00AA00BDCE1A"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IDocHostUIHandler {
            [PreserveSig]
            int ShowContextMenu(int dwID, POINT pt, [MarshalAs(UnmanagedType.Interface)] object pcmdtReserved,
                [MarshalAs(UnmanagedType.Interface)] object pdispReserved);

            [PreserveSig]
            int GetHostInfo(ref DOCHOSTUIINFO info);

            [PreserveSig]
            int ShowUI(int dwID, [MarshalAs(UnmanagedType.Interface)] object activeObject,
                [MarshalAs(UnmanagedType.Interface)] object commandTarget,
                [MarshalAs(UnmanagedType.Interface)] object frame, [MarshalAs(UnmanagedType.Interface)] object doc);

            [PreserveSig] int HideUI();
            [PreserveSig] int UpdateUI();
            [PreserveSig] int EnableModeless(bool fEnable);
            [PreserveSig] int OnDocWindowActivate(bool fActivate);
            [PreserveSig] int OnFrameWindowActivate(bool fActivate);
            [PreserveSig] int ResizeBorder(COMRECT rect, [MarshalAs(UnmanagedType.Interface)] object doc, bool fFrameWindow);
            [PreserveSig] int TranslateAccelerator(ref System.Windows.Forms.Message msg, ref Guid group, int nCmdID);
            [PreserveSig] int GetOptionKeyPath([Out, MarshalAs(UnmanagedType.LPArray)] string[] pbstrKey, int dw);
            [PreserveSig] int GetDropTarget([In, MarshalAs(UnmanagedType.Interface)] object pDropTarget, [MarshalAs(UnmanagedType.Interface)] out object ppDropTarget);
            [PreserveSig] int GetExternal([MarshalAs(UnmanagedType.IDispatch)] out object ppDispatch);
            [PreserveSig] int TranslateUrl(int dwTranslate, [MarshalAs(UnmanagedType.LPWStr)] string strURLIn, [MarshalAs(UnmanagedType.LPWStr)] out string pstrURLOut);
            [PreserveSig] int FilterDataObject(IDataObject pDO, out IDataObject ppDORet);
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct DOCHOSTUIINFO {
            public int cbSize;
            public int dwFlags;
            public int dwDoubleClick;
            public IntPtr dwReserved1;
            public IntPtr dwReserved2;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct COMRECT {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal class POINT {
            public int x;
            public int y;
        }

        [ComImport, Guid("3050F3F0-98B5-11CF-BB82-00AA00BDCE0B"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface ICustomDoc {
            [PreserveSig]
            int SetUIHandler(IDocHostUIHandler pUIHandler);
        }

        [ComImport, Guid("6D5140C1-7436-11CE-8034-00AA006009FA"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IOleServiceProvider {
            [PreserveSig]
            uint QueryService([In] ref Guid guidService, [In] ref Guid riid, [MarshalAs(UnmanagedType.IDispatch)] out object ppvObject);
        }

        [Flags]
        public enum HostUIFlags {
            DIALOG = 0x00000001,
            DISABLE_HELP_MENU = 0x00000002,
            NO3DBORDER = 0x00000004,
            SCROLL_NO = 0x00000008,
            DISABLE_SCRIPT_INACTIVE = 0x00000010,
            OPENNEWWIN = 0x00000020,
            DISABLE_OFFSCREEN = 0x00000040,
            FLAT_SCROLLBAR = 0x00000080,
            DIV_BLOCKDEFAULT = 0x00000100,
            ACTIVATE_CLIENTHIT_ONLY = 0x00000200,
            OVERRIDEBEHAVIORFACTORY = 0x00000400,
            CODEPAGELINKEDFONTS = 0x00000800,
            URL_ENCODING_DISABLE_UTF8 = 0x00001000,
            URL_ENCODING_ENABLE_UTF8 = 0x00002000,
            ENABLE_FORMS_AUTOCOMPLETE = 0x00004000,
            ENABLE_INPLACE_NAVIGATION = 0x00010000,
            IME_ENABLE_RECONVERSION = 0x00020000,
            THEME = 0x00040000,
            NOTHEME = 0x00080000,
            NOPICS = 0x00100000,
            NO3DOUTERBORDER = 0x00200000,
            DISABLE_EDIT_NS_FIXUP = 0x00400000,
            LOCAL_MACHINE_ACCESS_CHECK = 0x00800000,
            DISABLE_UNTRUSTEDPROTOCOL = 0x01000000,
            HOST_NAVIGATES = 0x02000000,
            ENABLE_REDIRECT_NOTIFICATION = 0x04000000,
            USE_WINDOWLESS_SELECTCONTROL = 0x08000000,
            USE_WINDOWED_SELECTCONTROL = 0x10000000,
            ENABLE_ACTIVEX_INACTIVATE_MODE = 0x20000000,
            DPI_AWARE = 0x40000000
        }
    }
}
