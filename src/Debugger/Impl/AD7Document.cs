// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Debugger.Interop;

namespace Microsoft.R.Debugger {
    internal sealed class AD7Document : IDebugDocument2, IDebugDocumentText2 {
        int IDebugDocument2.GetDocumentClassId(out Guid pclsid) {
            throw new NotImplementedException();
        }

        int IDebugDocumentText2.GetDocumentClassId(out Guid pclsid) {
            throw new NotImplementedException();
        }

        int IDebugDocumentText2.GetName(enum_GETNAME_TYPE gnType, out string pbstrFileName) {
            throw new NotImplementedException();
        }

        int IDebugDocument2.GetName(enum_GETNAME_TYPE gnType, out string pbstrFileName) {
            throw new NotImplementedException();
        }

        int IDebugDocumentText2.GetSize(IntPtr pcNumLines, IntPtr pcNumChars) {
            throw new NotImplementedException();
        }

        int IDebugDocumentText2.GetText(TEXT_POSITION pos, uint cMaxChars, IntPtr pText, IntPtr pcNumChars) {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Same as <see cref="Microsoft.VisualStudio.Debugger.Interop.IDebugDocumentText2">IDebugDocumentText2</see>,
    /// but uses <c>IntPtr</c> instead of <c>ref/out int</c> for <c>pcNumChars</c>, accounting for the fact that
    /// it may actually be null.
    /// </summary>
    [Guid("4B0645AA-08EF-4CB9-ADB9-0395D6EDAD35")]
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IDebugDocumentText2 {
        [PreserveSig]
        int GetDocumentClassId(out Guid pclsid);
        [PreserveSig]
        int GetName(enum_GETNAME_TYPE gnType, out string pbstrFileName);
        [PreserveSig]
        int GetSize(IntPtr pcNumLines, IntPtr pcNumChars);
        [PreserveSig]
        int GetText(TEXT_POSITION pos, uint cMaxChars, IntPtr pText, IntPtr pcNumChars);
    }

}
