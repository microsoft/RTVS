using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Microsoft.VisualStudio.R.Package.Plots {
    internal sealed class PlotWindowMenu {
        private Dictionary<string, int> _menuCommands = new Dictionary<string, int>();

        public PlotWindowMenu(IntPtr hMenu) {
            ProcessMenu(hMenu);
        }

        private void ProcessMenu(IntPtr hMenu) {
            Debug.Assert(hMenu != IntPtr.Zero);

            int itemCount = NativeMethods.GetMenuItemCount(hMenu);
            for (int i = 0; i < itemCount; i++) {
                NativeMethods.MENUITEMINFO mii = new NativeMethods.MENUITEMINFO();
                mii.cbSize = NativeMethods.MENUITEMINFO.sizeOf;

                // Get item type
                mii.fMask = NativeMethods.MIIM_TYPE;
                bool result = NativeMethods.GetMenuItemInfo(hMenu, (uint)i, true, ref mii);
                Debug.Assert(result);
                if (result) {
                    if (mii.fType == NativeMethods.MFT_STRING) {
                        IntPtr hSubMenu = GetItemSubmenu(hMenu, i);
                        if (hSubMenu != IntPtr.Zero) {
                            ProcessMenu(hSubMenu);
                        } else {
                            // Get item id
                            int itemId = GetItemId(hMenu, i);
                            if (itemId > 0) {
                                string itemName = GetItemName(hMenu, i);
                                Debug.Assert(itemName.Length > 0);
                                _menuCommands[itemName] = itemId;
                            }
                        }
                    }
                }
            }
        }

        private IntPtr GetItemSubmenu(IntPtr hMenu, int position) {
            NativeMethods.MENUITEMINFO mii = new NativeMethods.MENUITEMINFO();
            mii.cbSize = NativeMethods.MENUITEMINFO.sizeOf;
            mii.fMask = NativeMethods.MIIM_SUBMENU;
            bool result = NativeMethods.GetMenuItemInfo(hMenu, (uint)position, true, ref mii);
            if (result) {
                return mii.hSubMenu;
            }
            return IntPtr.Zero;
        }

        private string GetItemName(IntPtr hMenu, int position) {
            NativeMethods.MENUITEMINFO mii = new NativeMethods.MENUITEMINFO();
            mii.cbSize = NativeMethods.MENUITEMINFO.sizeOf;
            mii.fMask = NativeMethods.MIIM_STRING;
            bool result = NativeMethods.GetMenuItemInfo(hMenu, (uint)position, true, ref mii);
            if (result) {
                Debug.Assert(mii.cch > 0);
                try {
                    mii.dwTypeData = Marshal.AllocHGlobal(2 * (mii.cch + 1));
                    mii.cch++;
                    mii.fMask = NativeMethods.MIIM_STRING;
                    result = NativeMethods.GetMenuItemInfo(hMenu, (uint)position, true, ref mii);
                    Debug.Assert(result);
                    if (result && mii.cch > 0) {
                        string itemName = Marshal.PtrToStringAnsi(mii.dwTypeData, mii.cch);
                        Debug.Assert(itemName.Length > 0);
                        return itemName;
                    }
                } finally {
                    Marshal.FreeHGlobal(mii.dwItemData);
                }
            }
            return string.Empty;
        }

        private int GetItemId(IntPtr hMenu, int position) {
            NativeMethods.MENUITEMINFO mii = new NativeMethods.MENUITEMINFO();
            mii.cbSize = NativeMethods.MENUITEMINFO.sizeOf;
            mii.fMask = NativeMethods.MIIM_ID;
            bool result = NativeMethods.GetMenuItemInfo(hMenu, (uint)position, true, ref mii);
            if (result) {
                return (int)mii.wID;
            }
            return 0;
        }
    }
}
