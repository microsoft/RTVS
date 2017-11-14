// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Microsoft.VisualStudio.Shell.Mocks {
    public sealed class VsTextStreamMarker : IVsTextStreamMarker {
        private readonly IVsTextStream _buffer;
        private int _start;
        private int _length;
        private int _type;
        private uint _behavior;

        public VsTextStreamMarker(IVsTextStream buffer, int start, int length) {
            _buffer = buffer;
            _start = start;
            _length = length;
        }

        public int DrawGlyph(IntPtr hdc, RECT[] pRect) {
            throw new NotImplementedException();
        }

        public int ExecMarkerCommand(int iItem) {
            return VSConstants.S_OK;
        }

        public int GetBehavior(out uint pdwBehavior) {
            pdwBehavior = _behavior;
            throw new NotImplementedException();
        }

        public int GetCurrentSpan(out int piPos, out int piLen) {
            piPos = _start;
            piLen = _length;
            return VSConstants.S_OK;
        }

        public int GetMarkerCommandInfo(int iItem, string[] pbstrText, uint[] pcmdf) {
            throw new NotImplementedException();
        }

        public int GetPriorityIndex(out int piPriorityIndex) {
            piPriorityIndex = 0;
            return VSConstants.S_OK;
        }

        public int GetStreamBuffer(out IVsTextStream ppBuffer) {
            ppBuffer = _buffer;
            return VSConstants.S_OK;
        }

        public int GetTipText(string[] pbstrText) {
            pbstrText[0] = string.Empty;
            return VSConstants.S_OK;
        }

        public int GetType(out int piMarkerType) {
            piMarkerType = _type;
            return VSConstants.S_OK;
        }

        public int GetVisualStyle(out uint pdwFlags) {
            throw new NotImplementedException();
        }

        public int Invalidate() {
            return VSConstants.S_OK;
        }

        public int ResetSpan(int iNewPos, int iNewLen) {
            _start = iNewPos;
            _length = iNewLen;
            return VSConstants.S_OK;
        }

        public int SetBehavior(uint dwBehavior) {
            _behavior = dwBehavior;
            return VSConstants.S_OK;
        }

        public int SetType(int iMarkerType) {
            _type = iMarkerType;
            return VSConstants.S_OK;
        }

        public int SetVisualStyle(uint dwFlags) {
            throw new NotImplementedException();
        }

        public int UnadviseClient() {
            return VSConstants.S_OK;
        }
    }
}
