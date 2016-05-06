// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.R.Components.ContentTypes;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;

namespace Microsoft.R.Debugger {
    internal sealed class AD7MemoryAddress : IDebugCodeContext2, IDebugCodeContext100 {
        private IDebugMemoryContext2 IDebugMemoryContext2 => this;
        private IDebugCodeContext2 IDebugCodeContext2 => this;

        public AD7Engine Engine { get; }

        public string FileName { get; }

        public int? LineNumber { get; }

        public AD7StackFrame StackFrame { get; }

        public AD7DocumentContext DocumentContext { get; set; }

        public AD7MemoryAddress(AD7Engine engine, string fileName, int? lineNumber, AD7DocumentContext documentContext = null) {
            Engine = engine;
            FileName = fileName;
            LineNumber = lineNumber;
            DocumentContext = documentContext;
        }

        public AD7MemoryAddress(AD7StackFrame stackFrame) :
            this(stackFrame.Engine, stackFrame.StackFrame.FileName, stackFrame.StackFrame.LineNumber) {
        }

        int IDebugMemoryContext2.Add(ulong dwCount, out IDebugMemoryContext2 ppMemCxt) {
            ppMemCxt = new AD7MemoryAddress(Engine, FileName, LineNumber + (int)dwCount);
            return VSConstants.S_OK;
        }

        int IDebugMemoryContext2.Compare(enum_CONTEXT_COMPARE Compare, IDebugMemoryContext2[] rgpMemoryContextSet, uint dwMemoryContextSetLen, out uint pdwMemoryContext) {
            pdwMemoryContext = 0;

            for (int i = 0; i < rgpMemoryContextSet.Length; ++i) {
                var other = rgpMemoryContextSet[i] as AD7MemoryAddress;
                if (other == null || other.Engine != Engine) {
                    continue;
                }

                bool match = false;
                switch (Compare) {
                    case enum_CONTEXT_COMPARE.CONTEXT_EQUAL:
                        match = (LineNumber == other.LineNumber);
                        break;
                    case enum_CONTEXT_COMPARE.CONTEXT_LESS_THAN:
                        match = (LineNumber < other.LineNumber);
                        break;
                    case enum_CONTEXT_COMPARE.CONTEXT_LESS_THAN_OR_EQUAL:
                        match = (LineNumber <= other.LineNumber);
                        break;
                    case enum_CONTEXT_COMPARE.CONTEXT_GREATER_THAN:
                        match = (LineNumber > other.LineNumber);
                        break;
                    case enum_CONTEXT_COMPARE.CONTEXT_GREATER_THAN_OR_EQUAL:
                        match = (LineNumber >= other.LineNumber);
                        break;
                    case enum_CONTEXT_COMPARE.CONTEXT_SAME_PROCESS:
                        match = true;
                        break;
                    case enum_CONTEXT_COMPARE.CONTEXT_SAME_MODULE:
                        match = (FileName == other.FileName);
                        break;
                    case enum_CONTEXT_COMPARE.CONTEXT_SAME_FUNCTION:
                    case enum_CONTEXT_COMPARE.CONTEXT_SAME_SCOPE:
                        match = (LineNumber == other.LineNumber && FileName == other.FileName);
                        break;
                    default:
                        return VSConstants.E_NOTIMPL;
                }

                if (match) {
                    pdwMemoryContext = (uint)i;
                    return VSConstants.S_OK;
                }
            }

            return VSConstants.S_FALSE;
        }

        int IDebugMemoryContext2.GetInfo(enum_CONTEXT_INFO_FIELDS dwFields, CONTEXT_INFO[] pinfo) {
            pinfo[0].dwFields = 0;

            if ((dwFields & enum_CONTEXT_INFO_FIELDS.CIF_ADDRESS) != 0) {
                pinfo[0].bstrAddress = LineNumber.ToString();
                pinfo[0].dwFields |= enum_CONTEXT_INFO_FIELDS.CIF_ADDRESS;
            }

            if ((dwFields & enum_CONTEXT_INFO_FIELDS.CIF_FUNCTION) != 0) {
                pinfo[0].bstrFunction = StackFrame?.StackFrame?.CallingFrame?.Call ?? "<unknown>";
                pinfo[0].dwFields |= enum_CONTEXT_INFO_FIELDS.CIF_FUNCTION;
            }

            return VSConstants.S_OK;
        }

        int IDebugMemoryContext2.GetName(out string pbstrName) {
            pbstrName = null;
            return VSConstants.E_NOTIMPL;
        }

        int IDebugMemoryContext2.Subtract(ulong dwCount, out IDebugMemoryContext2 ppMemCxt) {
            ppMemCxt = new AD7MemoryAddress(Engine, FileName, LineNumber - (int)dwCount);
            return VSConstants.S_OK;
        }

        int IDebugCodeContext2.Add(ulong dwCount, out IDebugMemoryContext2 ppMemCxt) {
            return IDebugMemoryContext2.Add(dwCount, out ppMemCxt);
        }

        int IDebugCodeContext2.Compare(enum_CONTEXT_COMPARE Compare, IDebugMemoryContext2[] rgpMemoryContextSet, uint dwMemoryContextSetLen, out uint pdwMemoryContext) {
            return IDebugMemoryContext2.Compare(Compare, rgpMemoryContextSet, dwMemoryContextSetLen, out pdwMemoryContext);
        }

        int IDebugCodeContext2.GetDocumentContext(out IDebugDocumentContext2 ppSrcCxt) {
            ppSrcCxt = DocumentContext;
            return VSConstants.S_OK;
        }

        int IDebugCodeContext2.GetInfo(enum_CONTEXT_INFO_FIELDS dwFields, CONTEXT_INFO[] pinfo) {
            return IDebugMemoryContext2.GetInfo(dwFields, pinfo);
        }

        int IDebugCodeContext2.GetLanguageInfo(ref string pbstrLanguage, ref Guid pguidLanguage) {
            pbstrLanguage = RContentTypeDefinition.LanguageName;
            pguidLanguage = DebuggerConstants.LanguageServiceGuid;
            return VSConstants.S_OK;
        }

        int IDebugCodeContext2.GetName(out string pbstrName) {
            return IDebugMemoryContext2.GetName(out pbstrName);
        }

        int IDebugCodeContext2.Subtract(ulong dwCount, out IDebugMemoryContext2 ppMemCxt) {
            return IDebugMemoryContext2.Subtract(dwCount, out ppMemCxt);
        }

        int IDebugCodeContext100.GetProgram(out IDebugProgram2 ppProgram) {
            ppProgram = Engine;
            return VSConstants.S_OK;
        }
    }
}