using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Languages.Editor.Tasks;
using Microsoft.VisualStudio.R.Package.Commands;

namespace Microsoft.VisualStudio.R.Package.Plots {
    internal sealed class PlotWindowMenu {
        private Dictionary<string, int> _nameToWindowsIdMap = new Dictionary<string, int>();
        private Dictionary<int, PlotCommand> _idToPlotCommandMap = new Dictionary<int, PlotCommand>();

        public PlotWindowMenu(IntPtr hWnd, IntPtr hMenu) {
            ProcessMenu(hMenu);
            ExtractcommandIds(hWnd);
        }

        public void Execute(int id) {
            PlotCommand cmd;
            if (_idToPlotCommandMap.TryGetValue(id, out cmd)) {
                cmd.Execute();
            } else {
                Debug.Assert(false, "Unable to find plot command " + id.ToString());
            }
        }

        private void ExtractcommandIds(IntPtr hWnd) {
            _idToPlotCommandMap[RPackageCommandId.icmdExportPlotAsPdf] = new PlotCommand(hWnd, _nameToWindowsIdMap, "pdf");
            _idToPlotCommandMap[RPackageCommandId.icmdExportPlotAsPng] = new PlotCommand(hWnd, _nameToWindowsIdMap, "png");
            _idToPlotCommandMap[RPackageCommandId.icmdExportPlotAsBitmap] = new PlotCommand(hWnd, _nameToWindowsIdMap, "bmp");

            _idToPlotCommandMap[RPackageCommandId.icmdCopyPlotAsMetafile] = new PlotCommand(hWnd, _nameToWindowsIdMap, "as a metafile");
            _idToPlotCommandMap[RPackageCommandId.icmdCopyPlotAsBitmap] = new PlotCommand(hWnd, _nameToWindowsIdMap, "as a bitmap");

            _idToPlotCommandMap[RPackageCommandId.icmdPrevPlot] = new PlotCommand(hWnd, _nameToWindowsIdMap, "previous");
            _idToPlotCommandMap[RPackageCommandId.icmdNextPlot] = new PlotCommand(hWnd, _nameToWindowsIdMap, "next");

            _idToPlotCommandMap[RPackageCommandId.icmdClearPlots] = new PlotCommand(hWnd, _nameToWindowsIdMap, "clear");
            _idToPlotCommandMap[RPackageCommandId.icmdPrintPlot] = new PlotCommand(hWnd, _nameToWindowsIdMap, "print");

            IdleTimeAction.Create(() => {
                var add = new PlotCommand(hWnd, _nameToWindowsIdMap, "add");
                add.Execute();
                var recording = new PlotCommand(hWnd, _nameToWindowsIdMap, "recording");
                recording.Execute();
            }, 500, typeof(PlotWindowMenu));
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
                                itemName = itemName.Replace("&", string.Empty).ToLowerInvariant();
                                _nameToWindowsIdMap[itemName] = itemId;
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

        class PlotCommand {
            private IntPtr _hwndRPlotWindow;
            public int Id { get; private set; }
            public PlotCommand(IntPtr hwndRPlotWindow, Dictionary<string, int> commands, string text) {
                _hwndRPlotWindow = hwndRPlotWindow;

                Id = FindCommandByText(commands, text);
                Debug.Assert(Id > 0);
            }

            public void Execute() {
                if (_hwndRPlotWindow != IntPtr.Zero) {
                    NativeMethods.PostMessage(_hwndRPlotWindow, NativeMethods.WM_COMMAND, new IntPtr(Id), _hwndRPlotWindow);
                }
            }

            private int FindCommandByText(Dictionary<string, int> commands, string text) {
                string key = commands.Keys.FirstOrDefault(x => x.Contains(text));
                if (key != null) {
                    return commands[key];
                }
                return 0;
            }
        }
    }
}
