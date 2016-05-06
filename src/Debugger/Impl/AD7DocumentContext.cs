// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.R.Components.ContentTypes;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;

namespace Microsoft.R.Debugger {
    internal sealed class AD7DocumentContext : IDebugDocumentContext2 {
        public string FileName { get; }

        public TEXT_POSITION Start { get; }

        public TEXT_POSITION End { get; }

        public AD7MemoryAddress Address { get; }

        public AD7DocumentContext(string fileName, TEXT_POSITION start, TEXT_POSITION end, AD7MemoryAddress address) {
            FileName = fileName;
            Start = start;
            End = end;
            Address = address;
        }

        int IDebugDocumentContext2.Compare(enum_DOCCONTEXT_COMPARE Compare, IDebugDocumentContext2[] rgpDocContextSet, uint dwDocContextSetLen, out uint pdwDocContext) {
            pdwDocContext = 0;
            return VSConstants.E_NOTIMPL;
        }

        int IDebugDocumentContext2.EnumCodeContexts(out IEnumDebugCodeContexts2 ppEnumCodeCxts) {
            ppEnumCodeCxts = new AD7CodeContextEnum(new[] { Address });
            return VSConstants.S_OK;
        }

        int IDebugDocumentContext2.GetDocument(out IDebugDocument2 ppDocument) {
            ppDocument = null;
            return VSConstants.E_NOTIMPL;
        }

        int IDebugDocumentContext2.GetLanguageInfo(ref string pbstrLanguage, ref Guid pguidLanguage) {
            pbstrLanguage = RContentTypeDefinition.LanguageName;
            pguidLanguage = DebuggerConstants.LanguageServiceGuid;
            return VSConstants.S_OK;
        }

        int IDebugDocumentContext2.GetName(enum_GETNAME_TYPE gnType, out string pbstrFileName) {
            pbstrFileName = FileName;
            return VSConstants.S_OK;
        }

        int IDebugDocumentContext2.GetSourceRange(TEXT_POSITION[] pBegPosition, TEXT_POSITION[] pEndPosition) {
            pBegPosition[0] = Start;
            pEndPosition[0] = End;
            return VSConstants.S_OK;
        }

        int IDebugDocumentContext2.GetStatementRange(TEXT_POSITION[] pBegPosition, TEXT_POSITION[] pEndPosition) {
            pBegPosition[0] = Start;
            pEndPosition[0] = End;
            return VSConstants.S_OK;
        }

        int IDebugDocumentContext2.Seek(int nCount, out IDebugDocumentContext2 ppDocContext) {
            ppDocContext = null;
            return VSConstants.E_NOTIMPL;
        }
    }
}