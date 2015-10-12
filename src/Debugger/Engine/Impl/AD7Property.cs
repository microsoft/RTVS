using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;

namespace Microsoft.R.Debugger.Engine {
    internal sealed class AD7Property : IDebugProperty3 {
        public AD7StackFrame StackFrame { get; }

        public DebugEvaluationResult EvaluationResult { get; }

        public AD7Property(AD7StackFrame stackFrame, DebugEvaluationResult result) {
            StackFrame = stackFrame;
            EvaluationResult = result;
        }

        int IDebugProperty3.CreateObjectID() {
            return VSConstants.E_NOTIMPL;
        }

        int IDebugProperty3.DestroyObjectID() {
            return VSConstants.E_NOTIMPL;
        }

        int IDebugProperty2.EnumChildren(enum_DEBUGPROP_INFO_FLAGS dwFields, uint dwRadix, ref Guid guidFilter, enum_DBG_ATTRIB_FLAGS dwAttribFilter, string pszNameFilter, uint dwTimeout, out IEnumDebugPropertyInfo2 ppEnum) {
            throw new NotImplementedException();
        }

        int IDebugProperty3.EnumChildren(enum_DEBUGPROP_INFO_FLAGS dwFields, uint dwRadix, ref Guid guidFilter, enum_DBG_ATTRIB_FLAGS dwAttribFilter, string pszNameFilter, uint dwTimeout, out IEnumDebugPropertyInfo2 ppEnum) {
            throw new NotImplementedException();
        }

        int IDebugProperty3.GetCustomViewerCount(out uint pcelt) {
            throw new NotImplementedException();
        }

        int IDebugProperty3.GetCustomViewerList(uint celtSkip, uint celtRequested, DEBUG_CUSTOM_VIEWER[] rgViewers, out uint pceltFetched) {
            throw new NotImplementedException();
        }

        int IDebugProperty2.GetDerivedMostProperty(out IDebugProperty2 ppDerivedMost) {
            throw new NotImplementedException();
        }

        int IDebugProperty3.GetDerivedMostProperty(out IDebugProperty2 ppDerivedMost) {
            throw new NotImplementedException();
        }

        int IDebugProperty2.GetExtendedInfo(ref Guid guidExtendedInfo, out object pExtendedInfo) {
            pExtendedInfo = null;
            return VSConstants.E_NOTIMPL;
        }

        int IDebugProperty3.GetExtendedInfo(ref Guid guidExtendedInfo, out object pExtendedInfo) {
            pExtendedInfo = null;
            return VSConstants.E_NOTIMPL;
        }

        int IDebugProperty2.GetMemoryBytes(out IDebugMemoryBytes2 ppMemoryBytes) {
            ppMemoryBytes = null;
            return VSConstants.E_NOTIMPL;
        }

        int IDebugProperty3.GetMemoryBytes(out IDebugMemoryBytes2 ppMemoryBytes) {
            ppMemoryBytes = null;
            return VSConstants.E_NOTIMPL;
        }

        int IDebugProperty2.GetMemoryContext(out IDebugMemoryContext2 ppMemory) {
            ppMemory = null;
            return VSConstants.E_NOTIMPL;
        }

        int IDebugProperty3.GetMemoryContext(out IDebugMemoryContext2 ppMemory) {
            ppMemory = null;
            return VSConstants.E_NOTIMPL;
        }

        int IDebugProperty2.GetParent(out IDebugProperty2 ppParent) {
            ppParent = null;
            return VSConstants.E_NOTIMPL;
        }

        int IDebugProperty3.GetParent(out IDebugProperty2 ppParent) {
            ppParent = null;
            return VSConstants.E_NOTIMPL;
        }

        int IDebugProperty2.GetPropertyInfo(enum_DEBUGPROP_INFO_FLAGS dwFields, uint dwRadix, uint dwTimeout, IDebugReference2[] rgpArgs, uint dwArgCount, DEBUG_PROPERTY_INFO[] pPropertyInfo) {
            pPropertyInfo[0] = GetDebugPropertyInfo(dwRadix, dwFields);
            return VSConstants.S_OK;
        }

        int IDebugProperty3.GetPropertyInfo(enum_DEBUGPROP_INFO_FLAGS dwFields, uint dwRadix, uint dwTimeout, IDebugReference2[] rgpArgs, uint dwArgCount, DEBUG_PROPERTY_INFO[] pPropertyInfo) {
            return ((IDebugProperty2)this).GetPropertyInfo(dwFields, dwRadix, dwTimeout, rgpArgs, dwArgCount, pPropertyInfo);
        }

        int IDebugProperty2.GetReference(out IDebugReference2 ppReference) {
            ppReference = null;
            return VSConstants.E_NOTIMPL;
        }

        int IDebugProperty3.GetReference(out IDebugReference2 ppReference) {
            ppReference = null;
            return VSConstants.E_NOTIMPL;
        }

        int IDebugProperty2.GetSize(out uint pdwSize) {
            pdwSize = 0;
            return VSConstants.E_NOTIMPL;
        }

        int IDebugProperty3.GetSize(out uint pdwSize) {
            pdwSize = 0;
            return VSConstants.E_NOTIMPL;
        }

        int IDebugProperty3.GetStringCharLength(out uint pLen) {
            pLen = 0;

            var valueResult = EvaluationResult as DebugValueEvaluationResult;
            if (valueResult == null || valueResult.RawValue == null) {
                return VSConstants.E_FAIL;
            }

            pLen = (uint)valueResult.RawValue.Length;
            return VSConstants.S_OK;
        }

        int IDebugProperty3.GetStringChars(uint buflen, ushort[] rgString, out uint pceltFetched) {
            pceltFetched = 0;

            var valueResult = EvaluationResult as DebugValueEvaluationResult;
            if (valueResult == null || valueResult.RawValue == null) {
                return VSConstants.E_FAIL;
            }

            for (int i = 0; i < buflen; ++i) {
                rgString[i] = valueResult.RawValue[i];
            }
            return VSConstants.S_OK;
        }

        int IDebugProperty2.SetValueAsReference(IDebugReference2[] rgpArgs, uint dwArgCount, IDebugReference2 pValue, uint dwTimeout) {
            return VSConstants.E_NOTIMPL;
        }

        int IDebugProperty3.SetValueAsReference(IDebugReference2[] rgpArgs, uint dwArgCount, IDebugReference2 pValue, uint dwTimeout) {
            return VSConstants.E_NOTIMPL;
        }

        int IDebugProperty2.SetValueAsString(string pszValue, uint dwRadix, uint dwTimeout) {
            string errorString;
            return ((IDebugProperty3)this).SetValueAsStringWithError(pszValue, dwRadix, dwTimeout, out errorString);
        }

        int IDebugProperty3.SetValueAsString(string pszValue, uint dwRadix, uint dwTimeout) {
            return ((IDebugProperty2)this).SetValueAsString(pszValue, dwRadix, dwTimeout);
        }

        int IDebugProperty3.SetValueAsStringWithError(string pszValue, uint dwRadix, uint dwTimeout, out string errorString) {
            errorString = null;

            // TODO: dwRadix
            var setResult = EvaluationResult.SetValueAsync(pszValue).GetAwaiter().GetResult() as DebugErrorEvaluationResult;
            if (setResult != null) {
                errorString = setResult.ErrorText;
                return VSConstants.E_FAIL;
            }

            return VSConstants.S_OK;
        }

        internal DEBUG_PROPERTY_INFO GetDebugPropertyInfo(uint radix, enum_DEBUGPROP_INFO_FLAGS fields) {
            var dpi = new DEBUG_PROPERTY_INFO();

            // Always provide the property so that we can access locals from the automation object.
            dpi.pProperty = this;
            dpi.dwFields |= enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_PROP;

            var valueResult = EvaluationResult as DebugValueEvaluationResult;
            var errorResult = EvaluationResult as DebugErrorEvaluationResult;
            var promiseResult = EvaluationResult as DebugPromiseEvaluationResult;

            if (fields.HasFlag(enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_FULLNAME)) {
                dpi.bstrFullName = EvaluationResult.Expression;
                dpi.dwFields |= enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_FULLNAME;
            }

            if (fields.HasFlag(enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_NAME)) {
                dpi.bstrName = EvaluationResult.Expression;
                dpi.dwFields |= enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_NAME;
            }

            if (fields.HasFlag(enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_TYPE)) {
                if (valueResult != null) {
                    dpi.bstrType = valueResult.TypeName;
                    dpi.dwFields |= enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_TYPE;
                } else if (promiseResult != null) {
                    dpi.bstrType = "<promise>";
                    dpi.dwFields |= enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_TYPE;
                } else if (EvaluationResult is DebugActiveBindingEvaluationResult) {
                    dpi.bstrType = "<active binding>";
                    dpi.dwFields |= enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_TYPE;
                }
            }

            if (fields.HasFlag(enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_VALUE)) {
                if (valueResult != null) {
                    // TODO: handle radix
                    dpi.bstrValue = valueResult.Value;
                    dpi.dwFields |= enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_VALUE;
                } else if (promiseResult != null) {
                    dpi.bstrValue = promiseResult.Code;
                    dpi.dwFields |= enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_VALUE;
                } else if (errorResult != null) {
                    dpi.bstrValue = errorResult.ErrorText;
                    dpi.dwFields |= enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_VALUE;
                }
            }

            if (fields.HasFlag(enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_ATTRIB)) {
                dpi.dwFields |= enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_ATTRIB;

                // TODO:
                //    dpi.dwAttrib |= enum_DBG_ATTRIB_FLAGS.DBG_ATTRIB_OBJ_IS_EXPANDABLE;

                if (valueResult != null) {
                    dpi.dwAttrib |= enum_DBG_ATTRIB_FLAGS.DBG_ATTRIB_VALUE_RAW_STRING;
                    switch (valueResult.TypeName) {
                        //case "character":
                        //    dpi.dwAttrib |= enum_DBG_ATTRIB_FLAGS.DBG_ATTRIB_VALUE_RAW_STRING;
                        //    break;
                        case "logical":
                            dpi.dwAttrib |= enum_DBG_ATTRIB_FLAGS.DBG_ATTRIB_VALUE_BOOLEAN;
                            break;
                        case "closure":
                            dpi.dwAttrib |= enum_DBG_ATTRIB_FLAGS.DBG_ATTRIB_METHOD;
                            break;
                    }
                } else if (errorResult != null) {
                    dpi.dwAttrib |= enum_DBG_ATTRIB_FLAGS.DBG_ATTRIB_VALUE_ERROR;
                } else if (promiseResult != null) {
                    dpi.dwAttrib |= enum_DBG_ATTRIB_FLAGS.DBG_ATTRIB_VALUE_SIDE_EFFECT;
                } else if (EvaluationResult is DebugActiveBindingEvaluationResult) {
                    dpi.dwAttrib |= enum_DBG_ATTRIB_FLAGS.DBG_ATTRIB_PROPERTY;
                }
            }

            return dpi;
        }
    }
}