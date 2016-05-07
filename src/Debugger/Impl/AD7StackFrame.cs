// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.IO;
using Microsoft.Common.Core;
using Microsoft.R.Components.ContentTypes;
using Microsoft.R.StackTracing;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;
using static System.FormattableString;

namespace Microsoft.R.Debugger {
    internal sealed class AD7StackFrame : IDebugStackFrame2, IDebugExpressionContext2 {
        private Lazy<AD7Property> _property;

        public AD7Engine Engine { get; }
        public IRStackFrame StackFrame { get; }

        public AD7StackFrame(AD7Engine engine, IRStackFrame stackFrame) {
            Engine = engine;
            StackFrame = stackFrame;

            _property = Lazy.Create(() => new AD7Property(this, TaskExtensions.RunSynchronouslyOnUIThread(ct => StackFrame.DescribeEnvironmentAsync(cancellationToken: ct)), isFrameEnvironment: true));
        }

        int IDebugStackFrame2.EnumProperties(enum_DEBUGPROP_INFO_FLAGS dwFields, uint nRadix, ref Guid guidFilter, uint dwTimeout, out uint pcelt, out IEnumDebugPropertyInfo2 ppEnum) {
            pcelt = 0;

            int hr = ((IDebugProperty2)_property.Value).EnumChildren(dwFields, nRadix, guidFilter, enum_DBG_ATTRIB_FLAGS.DBG_ATTRIB_ALL, null, dwTimeout, out ppEnum);
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
            ppProperty = _property.Value;
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
                fi.m_bstrFuncName = StackFrame.EnvironmentName ?? StackFrame.CallingFrame?.Call ?? "<unknown>";
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
            pbstrName = StackFrame.CallingFrame?.Call;
            return VSConstants.S_OK;
        }

        int IDebugStackFrame2.GetThread(out IDebugThread2 ppThread) {
            ppThread = Engine.MainThread;
            return VSConstants.S_OK;
        }

        int IDebugStackFrame2.GetPhysicalStackRange(out ulong paddrMin, out ulong paddrMax) {
            paddrMin = paddrMax = 0;
            return VSConstants.E_NOTIMPL;
        }

        int IDebugExpressionContext2.GetName(out string pbstrName) {
            pbstrName = Invariant($"{StackFrame.FileName}:{StackFrame.LineNumber}|{StackFrame.CallingFrame?.Call}");
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