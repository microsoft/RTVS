using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.R.Editor.ContentType;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;

namespace Microsoft.R.Debugger.Engine {
    internal sealed class AD7StackFrame : IDebugStackFrame2, IDebugExpressionContext2, IDebugProperty2 {
        public AD7Engine Engine { get; }

        public DebugStackFrame StackFrame { get; }

        public AD7StackFrame(AD7Engine engine, DebugStackFrame stackFrame) {
            Engine = engine;
            StackFrame = stackFrame;
        }

        int IDebugStackFrame2.EnumProperties(enum_DEBUGPROP_INFO_FLAGS dwFields, uint nRadix, ref Guid guidFilter, uint dwTimeout, out uint pcelt, out IEnumDebugPropertyInfo2 ppEnum) {
            pcelt = 0;

            int hr = ((IDebugProperty2)this).EnumChildren(dwFields, nRadix, guidFilter, enum_DBG_ATTRIB_FLAGS.DBG_ATTRIB_ALL, null, dwTimeout, out ppEnum);
            if (hr < 0) {
                return hr;
            }

            if (ppEnum != null) {
                hr = ppEnum.GetCount(out pcelt);
                if (hr < 0) {
                    return hr;
                }
            }

            return VSConstants.S_OK;
        }

        int IDebugStackFrame2.GetCodeContext(out IDebugCodeContext2 ppCodeCxt) {
            ppCodeCxt = new AD7MemoryAddress(this);
            return VSConstants.S_OK;
        }

        int IDebugStackFrame2.GetDebugProperty(out IDebugProperty2 ppProperty) {
            ppProperty = this;
            return VSConstants.S_OK;
        }

        int IDebugStackFrame2.GetDocumentContext(out IDebugDocumentContext2 ppCxt) {
            var pos = new TEXT_POSITION { dwColumn = 0, dwLine = (uint)((StackFrame.LineNumber - 1) ?? 0) };

            string fileName = StackFrame.FileName;
            if (fileName != null) {
                try {
                    fileName = Path.GetFullPath(fileName);
                } catch (Exception) {
                }
            }

            ppCxt = new AD7DocumentContext(fileName, pos, pos, null);
            return VSConstants.S_OK;
        }

        int IDebugStackFrame2.GetExpressionContext(out IDebugExpressionContext2 ppExprCxt) {
            ppExprCxt = this;
            return VSConstants.S_OK;
        }

        int IDebugStackFrame2.GetInfo(enum_FRAMEINFO_FLAGS dwFieldSpec, uint nRadix, FRAMEINFO[] pFrameInfo) {
            string moduleName = StackFrame.FileName;
            if (moduleName != null) {
                try {
                    moduleName = Path.GetFileName(moduleName);
                } catch (ArgumentException) {
                }
            } else if (!StackFrame.IsGlobal) { 
                moduleName = "<unknown>";
            }

            var fi = new FRAMEINFO();

            if (dwFieldSpec.HasFlag(enum_FRAMEINFO_FLAGS.FIF_LANGUAGE)) {
                fi.m_bstrLanguage = RContentTypeDefinition.LanguageName;
                fi.m_dwValidFields |= enum_FRAMEINFO_FLAGS.FIF_LANGUAGE;
            }

            if (dwFieldSpec.HasFlag(enum_FRAMEINFO_FLAGS.FIF_DEBUGINFO)) {
                fi.m_fHasDebugInfo = 1;
                fi.m_dwValidFields |= enum_FRAMEINFO_FLAGS.FIF_DEBUGINFO;
            }

            if (dwFieldSpec.HasFlag(enum_FRAMEINFO_FLAGS.FIF_STALECODE)) {
                fi.m_fStaleCode = 0;
                fi.m_dwValidFields |= enum_FRAMEINFO_FLAGS.FIF_STALECODE;
            }

            if (dwFieldSpec.HasFlag(enum_FRAMEINFO_FLAGS.FIF_FRAME)) {
                fi.m_pFrame = this;
                fi.m_dwValidFields |= enum_FRAMEINFO_FLAGS.FIF_FRAME;
            }

            if (dwFieldSpec.HasFlag(enum_FRAMEINFO_FLAGS.FIF_MODULE) && moduleName != null) {
                fi.m_bstrModule = moduleName;
                fi.m_dwValidFields |= enum_FRAMEINFO_FLAGS.FIF_MODULE;
            }

            if (dwFieldSpec.HasFlag(enum_FRAMEINFO_FLAGS.FIF_FUNCNAME)) {
                fi.m_bstrFuncName = StackFrame.CallingExpression ?? (StackFrame.IsGlobal ? "<global>" : "<unknown>");
                fi.m_dwValidFields |= enum_FRAMEINFO_FLAGS.FIF_FUNCNAME;

                if (dwFieldSpec.HasFlag(enum_FRAMEINFO_FLAGS.FIF_FUNCNAME_MODULE) && moduleName != null) {
                    fi.m_bstrFuncName += " in " + moduleName;
                    fi.m_dwValidFields |= enum_FRAMEINFO_FLAGS.FIF_FUNCNAME_MODULE;
                }

                if (dwFieldSpec.HasFlag(enum_FRAMEINFO_FLAGS.FIF_FUNCNAME_LINES) && StackFrame.LineNumber != null) {
                    fi.m_bstrFuncName += " line " + StackFrame.LineNumber;
                    fi.m_dwValidFields |= enum_FRAMEINFO_FLAGS.FIF_FUNCNAME_LINES;
                }
            }

            pFrameInfo[0] = fi;
            return VSConstants.S_OK;
        }

        int IDebugStackFrame2.GetLanguageInfo(ref string pbstrLanguage, ref Guid pguidLanguage) {
            pbstrLanguage = RContentTypeDefinition.LanguageName;
            pguidLanguage = DebuggerConstants.LanguageServiceGuid;
            return VSConstants.S_OK;
        }

        int IDebugStackFrame2.GetName(out string pbstrName) {
            pbstrName = StackFrame.CallingExpression;
            return VSConstants.S_OK;
        }

        int IDebugStackFrame2.GetThread(out IDebugThread2 ppThread) {
            ppThread = Engine.MainThread;
            return VSConstants.S_OK;
        }

        int IDebugProperty2.EnumChildren(enum_DEBUGPROP_INFO_FLAGS dwFields, uint dwRadix, ref Guid guidFilter, enum_DBG_ATTRIB_FLAGS dwAttribFilter, string pszNameFilter, uint dwTimeout, out IEnumDebugPropertyInfo2 ppEnum) {
            var vars = StackFrame.GetVariables().GetAwaiter().GetResult().Values;
            var infos = vars
                .OrderBy(v => v.Expression)
                .Select(v => new AD7Property(this, v).GetDebugPropertyInfo(dwRadix, dwFields))
                .ToArray();
            ppEnum = new AD7PropertyInfoEnum(infos);
            return VSConstants.S_OK;
        }

        int IDebugProperty2.GetDerivedMostProperty(out IDebugProperty2 ppDerivedMost) {
            ppDerivedMost = null;
            return VSConstants.E_NOTIMPL;
        }

        int IDebugProperty2.GetExtendedInfo(ref Guid guidExtendedInfo, out object pExtendedInfo) {
            pExtendedInfo = null;
            return VSConstants.E_NOTIMPL;
        }

        int IDebugProperty2.GetMemoryBytes(out IDebugMemoryBytes2 ppMemoryBytes) {
            ppMemoryBytes = null;
            return VSConstants.E_NOTIMPL;
        }

        int IDebugProperty2.GetMemoryContext(out IDebugMemoryContext2 ppMemory) {
            ppMemory = null;
            return VSConstants.E_NOTIMPL;
        }

        int IDebugProperty2.GetParent(out IDebugProperty2 ppParent) {
            ppParent = null;
            return VSConstants.E_NOTIMPL;
        }

        int IDebugStackFrame2.GetPhysicalStackRange(out ulong paddrMin, out ulong paddrMax) {
            paddrMin = paddrMax = 0;
            return VSConstants.E_NOTIMPL;
        }

        int IDebugProperty2.GetPropertyInfo(enum_DEBUGPROP_INFO_FLAGS dwFields, uint dwRadix, uint dwTimeout, IDebugReference2[] rgpArgs, uint dwArgCount, DEBUG_PROPERTY_INFO[] pPropertyInfo) {
            return VSConstants.E_NOTIMPL;
        }

        int IDebugProperty2.GetReference(out IDebugReference2 ppReference) {
            ppReference = null;
            return VSConstants.E_NOTIMPL;
        }

        int IDebugProperty2.GetSize(out uint pdwSize) {
            pdwSize = 0;
            return VSConstants.E_NOTIMPL;
        }

        int IDebugProperty2.SetValueAsReference(IDebugReference2[] rgpArgs, uint dwArgCount, IDebugReference2 pValue, uint dwTimeout) {
            return VSConstants.E_NOTIMPL;
        }

        int IDebugProperty2.SetValueAsString(string pszValue, uint dwRadix, uint dwTimeout) {
            return VSConstants.E_NOTIMPL;
        }

        int IDebugExpressionContext2.GetName(out string pbstrName) {
            pbstrName = string.Format("{0}:{1}|{2}", StackFrame.FileName, StackFrame.LineNumber, StackFrame.CallingExpression);
            return VSConstants.S_OK;
        }

        int IDebugExpressionContext2.ParseText(string pszCode, enum_PARSEFLAGS dwFlags, uint nRadix, out IDebugExpression2 ppExpr, out string pbstrError, out uint pichError) {
            pbstrError = "";
            pichError = 0;
            ppExpr = new AD7Expression(this, pszCode);
            return VSConstants.S_OK;
        }
    }
}