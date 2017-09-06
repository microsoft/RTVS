// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Microsoft.VisualStudio.Shell.Mocks
{
    [ExcludeFromCodeCoverage]
    public sealed class VsCodeWindowMock : IVsCodeWindow
    {
        private Guid _clsidView;
        private IVsTextLines _textLines;

        public int Close()
        {
            return VSConstants.S_OK;
        }

        public int GetBuffer(out IVsTextLines ppBuffer)
        {
            ppBuffer = _textLines;
            return VSConstants.S_OK;
        }

        public int GetEditorCaption(READONLYSTATUS dwReadOnly, out string pbstrEditorCaption)
        {
            pbstrEditorCaption = null;
            return VSConstants.S_OK;
        }

        public int GetLastActiveView(out IVsTextView ppView)
        {
            ppView = null;
            return VSConstants.S_OK;
        }

        public int GetPrimaryView(out IVsTextView ppView)
        {
            ppView = null;
            return VSConstants.S_OK;
        }

        public int GetSecondaryView(out IVsTextView ppView)
        {
            ppView = null;
            return VSConstants.S_OK;
        }

        public int GetViewClassID(out Guid pclsidView)
        {
            pclsidView = _clsidView;
            return VSConstants.S_OK;
        }

        public int SetBaseEditorCaption(string[] pszBaseEditorCaption)
        {
            return VSConstants.S_OK;
        }

        public int SetBuffer(IVsTextLines pBuffer)
        {
            _textLines = pBuffer;
            return VSConstants.S_OK;
        }

        public int SetViewClassID(ref Guid clsidView)
        {
            _clsidView = clsidView;
            return VSConstants.S_OK;
        }
    }
}
