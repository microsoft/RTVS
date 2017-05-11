// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Windows.Input;
using Microsoft.Common.Core.UI.Commands;

namespace Microsoft.Languages.Editor.Application.Controller {
    [ExcludeFromCodeCoverage]
    internal class KeyToVS2KCommandMapping {
        static private KeyToVS2KCommandMapping _instance;
        private const char DividerChar = ';';

        private Dictionary<Tuple<ModifierKeys, Key>, VSConstants.VSStd2KCmdID> _map;

        private KeyToVS2KCommandMapping() {
            _map = new Dictionary<Tuple<ModifierKeys, Key>, VSConstants.VSStd2KCmdID>();
            InstantiateDefaultValues();
        }

        public static KeyToVS2KCommandMapping GetInstance() {
            _instance = new KeyToVS2KCommandMapping();
            return _instance;
        }

        public string PersistToString() {
            var bldr = new StringBuilder();

            foreach (var kvp in _map) {
                string modifier = kvp.Key.Item1.ToString();
                string Key = kvp.Key.Item2.ToString();
                string id = kvp.Value.ToString();

                bldr.AppendLine(string.Concat(modifier, DividerChar, Key, DividerChar, id));
            }

            return bldr.ToString();
        }

        private Dictionary<Tuple<ModifierKeys, Key>, VSConstants.VSStd2KCmdID> FromString(string text) {
            var newMap = new Dictionary<Tuple<ModifierKeys, Key>, VSConstants.VSStd2KCmdID>();

            using (StringReader reader = new StringReader(text)) {
                string line;

                while ((line = reader.ReadLine()) != null) {
                    if (line.Length > 0) {
                        var splitted = line.Split(new char[] { DividerChar }, StringSplitOptions.RemoveEmptyEntries);

                        if (splitted.Length != 3) {
                            throw new ApplicationException();
                        }

                        ModifierKeys mkeys;
                        Key key;
                        VSConstants.VSStd2KCmdID cmdId;

                        if (!Enum.TryParse<ModifierKeys>(splitted[0], true, out mkeys)) {
                            throw new ApplicationException();
                        }

                        if (!Enum.TryParse<Key>(splitted[1], true, out key)) {
                            throw new ApplicationException();
                        }

                        if (!Enum.TryParse<VSConstants.VSStd2KCmdID>(splitted[2], true, out cmdId)) {
                            throw new ApplicationException();
                        }

                        newMap.Add(new Tuple<ModifierKeys, Key>(mkeys, key), cmdId);
                    }
                }
            }

            if (newMap.Count == 0) {
                return null;
            }

            return newMap;
        }

        public bool TryGetValue(ModifierKeys modifier, Key key, out VSConstants.VSStd2KCmdID commandId) {
            return _map.TryGetValue(new Tuple<ModifierKeys, Key>(modifier, key), out commandId);
        }

        private void InstantiateDefaultValues() {
            // No modifiers
            AddCommand(Key.Right, VSConstants.VSStd2KCmdID.RIGHT);
            AddCommand(Key.Left, VSConstants.VSStd2KCmdID.LEFT);
            AddCommand(Key.Up, VSConstants.VSStd2KCmdID.UP);
            AddCommand(Key.Down, VSConstants.VSStd2KCmdID.DOWN);
            AddCommand(Key.PageUp, VSConstants.VSStd2KCmdID.PAGEUP);
            AddCommand(Key.PageDown, VSConstants.VSStd2KCmdID.PAGEDN);
            AddCommand(Key.Home, VSConstants.VSStd2KCmdID.HOME);
            AddCommand(Key.End, VSConstants.VSStd2KCmdID.END);
            AddCommand(Key.Escape, VSConstants.VSStd2KCmdID.CANCEL);
            AddCommand(Key.Delete, VSConstants.VSStd2KCmdID.DELETE);
            AddCommand(Key.Back, VSConstants.VSStd2KCmdID.BACKSPACE);
            AddCommand(Key.Insert, VSConstants.VSStd2KCmdID.INSERT);
            AddCommand(Key.Enter, VSConstants.VSStd2KCmdID.RETURN);
            AddCommand(Key.Tab, VSConstants.VSStd2KCmdID.TAB);
            ////AddCommand(Key.F12, VSConstants.VSStd2KCmdID.GOTOTYPEDEF);

            // Shift Key
            AddShiftCommand(Key.Back, VSConstants.VSStd2KCmdID.BACKSPACE);
            AddShiftCommand(Key.Right, VSConstants.VSStd2KCmdID.RIGHT_EXT);
            AddShiftCommand(Key.Left, VSConstants.VSStd2KCmdID.LEFT_EXT);
            AddShiftCommand(Key.Up, VSConstants.VSStd2KCmdID.UP_EXT);
            AddShiftCommand(Key.Down, VSConstants.VSStd2KCmdID.DOWN_EXT);
            AddShiftCommand(Key.Home, VSConstants.VSStd2KCmdID.HOME_EXT);
            AddShiftCommand(Key.End, VSConstants.VSStd2KCmdID.END_EXT);
            AddShiftCommand(Key.PageUp, VSConstants.VSStd2KCmdID.PAGEUP_EXT);
            AddShiftCommand(Key.PageDown, VSConstants.VSStd2KCmdID.PAGEDN_EXT);
            AddShiftCommand(Key.Tab, VSConstants.VSStd2KCmdID.BACKTAB);
            AddShiftCommand(Key.Enter, VSConstants.VSStd2KCmdID.RETURN);

            // Ctrl-Shift Key
            AddControlShiftCommand(Key.Right, VSConstants.VSStd2KCmdID.CTLMOVERIGHT);
            AddControlShiftCommand(Key.Left, VSConstants.VSStd2KCmdID.CTLMOVELEFT);
            AddControlShiftCommand(Key.Home, VSConstants.VSStd2KCmdID.TOPLINE_EXT);
            AddControlShiftCommand(Key.End, VSConstants.VSStd2KCmdID.BOTTOMLINE_EXT);
            AddControlShiftCommand(Key.F, VSConstants.VSStd2KCmdID.FORMATDOCUMENT);

            // Ctrl Key
            AddControlCommand(Key.Space, VSConstants.VSStd2KCmdID.COMPLETEWORD);
            AddControlCommand(Key.J, VSConstants.VSStd2KCmdID.SHOWMEMBERLIST);
            AddControlCommand(Key.Back, VSConstants.VSStd2KCmdID.DELETEWORDLEFT);
            AddControlCommand(Key.Delete, VSConstants.VSStd2KCmdID.DELETEWORDRIGHT);
            AddControlCommand(Key.A, VSConstants.VSStd2KCmdID.SELECTALL);
            AddControlCommand(Key.W, VSConstants.VSStd2KCmdID.SELECTCURRENTWORD);
            AddControlCommand(Key.Right, VSConstants.VSStd2KCmdID.WORDNEXT);
            AddControlCommand(Key.Left, VSConstants.VSStd2KCmdID.WORDPREV);
            AddControlCommand(Key.Home, VSConstants.VSStd2KCmdID.TOPLINE);
            AddControlCommand(Key.End, VSConstants.VSStd2KCmdID.BOTTOMLINE);
            AddControlCommand(Key.Up, VSConstants.VSStd2KCmdID.SCROLLUP);
            AddControlCommand(Key.Down, VSConstants.VSStd2KCmdID.SCROLLDN);
            AddControlCommand(Key.C, VSConstants.VSStd2KCmdID.COPY);
            AddControlCommand(Key.X, VSConstants.VSStd2KCmdID.CUT);
            AddControlCommand(Key.V, VSConstants.VSStd2KCmdID.PASTE);
            AddControlCommand(Key.Z, VSConstants.VSStd2KCmdID.UNDO);
            AddControlCommand(Key.Y, VSConstants.VSStd2KCmdID.REDO);
        }

        private void AddCommand(Key key, VSConstants.VSStd2KCmdID id) => AddCommmand(ModifierKeys.None, key, id);
        private void AddShiftCommand(Key key, VSConstants.VSStd2KCmdID id) => AddCommmand(ModifierKeys.Shift, key, id);
        private void AddControlCommand(Key key, VSConstants.VSStd2KCmdID id) => AddCommmand(ModifierKeys.Control, key, id);
        private void AddControlShiftCommand(Key key, VSConstants.VSStd2KCmdID id) => AddCommmand(ModifierKeys.Control | ModifierKeys.Shift, key, id);
        private void AddCommmand(ModifierKeys modifiers, Key key, VSConstants.VSStd2KCmdID id) => _map.Add(new Tuple<ModifierKeys, Key>(modifiers, key), id);
    }
}
