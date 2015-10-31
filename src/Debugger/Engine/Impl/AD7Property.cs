using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Common.Core;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;
using static System.FormattableString;

namespace Microsoft.R.Debugger.Engine {
    internal sealed class AD7Property : IDebugProperty3 {
        private IDebugProperty2 IDebugProperty2 => this;
        private IDebugProperty3 IDebugProperty3 => this;

        private Lazy<IReadOnlyList<DebugEvaluationResult>> _children;

        public AD7Property Parent { get; }
        public AD7StackFrame StackFrame { get; }
        public DebugEvaluationResult EvaluationResult { get; }
        public bool IsSynthetic { get; }
        public bool IsFrameEnvironment { get; }

        public AD7Property(AD7Property parent, DebugEvaluationResult result, bool isSynthetic = false)
            : this(parent.StackFrame, result, isSynthetic, false) {
            Parent = parent;
        }

        public AD7Property(AD7StackFrame stackFrame, DebugEvaluationResult result, bool isSynthetic = false, bool isFrameEnvironment = false) {
            StackFrame = stackFrame;
            EvaluationResult = result;
            IsSynthetic = isSynthetic;
            IsFrameEnvironment = isFrameEnvironment;

            _children = Lazy.Create(() =>
                (EvaluationResult as DebugValueEvaluationResult)?.GetChildrenAsync()?.GetResultOnUIThread()
                ?? new DebugEvaluationResult[0]);
        }

        int IDebugProperty2.EnumChildren(enum_DEBUGPROP_INFO_FLAGS dwFields, uint dwRadix, ref Guid guidFilter, enum_DBG_ATTRIB_FLAGS dwAttribFilter, string pszNameFilter, uint dwTimeout, out IEnumDebugPropertyInfo2 ppEnum) {
            IEnumerable<DebugEvaluationResult> children = _children.Value;
            if (IsFrameEnvironment) {
                children = children.OrderBy(v => v.Name);
            }

            var infos = children.Select(v => new AD7Property(this, v).GetDebugPropertyInfo(dwRadix, dwFields));

            var valueResult = EvaluationResult as DebugValueEvaluationResult;
            if (valueResult != null && valueResult.HasAttributes == true) {
                var attrResult = StackFrame.StackFrame.EvaluateAsync(Invariant($"attributes({valueResult.Expression})"), "attributes()").GetResultOnUIThread();
                if (!(attrResult is DebugErrorEvaluationResult)) {
                    var attrInfo = new AD7Property(this, attrResult, isSynthetic: true).GetDebugPropertyInfo(dwRadix, dwFields);
                    infos = new[] { attrInfo }.Concat(infos);
                }
            }

            ppEnum = new AD7PropertyInfoEnum(infos.ToArray());
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
            ppParent = Parent;
            return ppParent != null ? VSConstants.S_OK : DebuggerConstants.S_GETPARENT_NO_PARENT;
        }

        int IDebugProperty2.GetPropertyInfo(enum_DEBUGPROP_INFO_FLAGS dwFields, uint dwRadix, uint dwTimeout, IDebugReference2[] rgpArgs, uint dwArgCount, DEBUG_PROPERTY_INFO[] pPropertyInfo) {
            pPropertyInfo[0] = GetDebugPropertyInfo(dwRadix, dwFields);
            return VSConstants.S_OK;
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
            string errorString;
            return IDebugProperty3.SetValueAsStringWithError(pszValue, dwRadix, dwTimeout, out errorString);
        }

        int IDebugProperty3.SetValueAsString(string pszValue, uint dwRadix, uint dwTimeout) {
            return IDebugProperty2.SetValueAsString(pszValue, dwRadix, dwTimeout);
        }

        int IDebugProperty3.CreateObjectID() {
            return VSConstants.E_NOTIMPL;
        }

        int IDebugProperty3.DestroyObjectID() {
            return VSConstants.E_NOTIMPL;
        }

        int IDebugProperty3.EnumChildren(enum_DEBUGPROP_INFO_FLAGS dwFields, uint dwRadix, ref Guid guidFilter, enum_DBG_ATTRIB_FLAGS dwAttribFilter, string pszNameFilter, uint dwTimeout, out IEnumDebugPropertyInfo2 ppEnum) {
            return IDebugProperty2.EnumChildren(dwFields, dwRadix, guidFilter, dwAttribFilter, pszNameFilter, dwTimeout, out ppEnum);
        }

        int IDebugProperty3.GetCustomViewerCount(out uint pcelt) {
            pcelt = 0;
            return VSConstants.S_OK;
        }

        int IDebugProperty3.GetCustomViewerList(uint celtSkip, uint celtRequested, DEBUG_CUSTOM_VIEWER[] rgViewers, out uint pceltFetched) {
            pceltFetched = 0;
            return VSConstants.S_OK;
        }

        int IDebugProperty3.GetDerivedMostProperty(out IDebugProperty2 ppDerivedMost) {
            return IDebugProperty2.GetDerivedMostProperty(out ppDerivedMost);
        }

        int IDebugProperty3.GetExtendedInfo(ref Guid guidExtendedInfo, out object pExtendedInfo) {
            return IDebugProperty2.GetExtendedInfo(guidExtendedInfo, out pExtendedInfo);
        }

        int IDebugProperty3.GetMemoryBytes(out IDebugMemoryBytes2 ppMemoryBytes) {
            return IDebugProperty2.GetMemoryBytes(out ppMemoryBytes);
        }

        int IDebugProperty3.GetMemoryContext(out IDebugMemoryContext2 ppMemory) {
            return IDebugProperty2.GetMemoryContext(out ppMemory);
        }

        int IDebugProperty3.GetParent(out IDebugProperty2 ppParent) {
            return IDebugProperty2.GetParent(out ppParent);
        }

        int IDebugProperty3.GetPropertyInfo(enum_DEBUGPROP_INFO_FLAGS dwFields, uint dwRadix, uint dwTimeout, IDebugReference2[] rgpArgs, uint dwArgCount, DEBUG_PROPERTY_INFO[] pPropertyInfo) {
            return IDebugProperty2.GetPropertyInfo(dwFields, dwRadix, dwTimeout, rgpArgs, dwArgCount, pPropertyInfo);
        }

        int IDebugProperty3.GetSize(out uint pdwSize) {
            return IDebugProperty2.GetSize(out pdwSize);
        }

        int IDebugProperty3.GetStringCharLength(out uint pLen) {
            pLen = 0;

            var valueResult = EvaluationResult as DebugValueEvaluationResult;
            if (valueResult == null || valueResult.Representation.ToString == null) {
                return VSConstants.E_FAIL;
            }

            pLen = (uint)valueResult.Representation.ToString.Length;
            return VSConstants.S_OK;
        }

        int IDebugProperty3.GetStringChars(uint buflen, ushort[] rgString, out uint pceltFetched) {
            pceltFetched = 0;

            var valueResult = EvaluationResult as DebugValueEvaluationResult;
            if (valueResult == null || valueResult.Representation.ToString == null) {
                return VSConstants.E_FAIL;
            }

            for (int i = 0; i < buflen; ++i) {
                rgString[i] = valueResult.Representation.ToString[i];
            }
            return VSConstants.S_OK;
        }

        int IDebugProperty3.GetReference(out IDebugReference2 ppReference) {
            return IDebugProperty2.GetReference(out ppReference);
        }

        int IDebugProperty3.SetValueAsReference(IDebugReference2[] rgpArgs, uint dwArgCount, IDebugReference2 pValue, uint dwTimeout) {
            return IDebugProperty2.SetValueAsReference(rgpArgs, dwArgCount, pValue, dwTimeout);
        }

        int IDebugProperty3.SetValueAsStringWithError(string pszValue, uint dwRadix, uint dwTimeout, out string errorString) {
            errorString = null;

            // TODO: dwRadix
            var setResult = EvaluationResult.SetValueAsync(pszValue).GetResultOnUIThread() as DebugErrorEvaluationResult;
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
                dpi.bstrName = EvaluationResult.Name;
                if (Parent?.IsFrameEnvironment == true && dpi.bstrName?.StartsWith("$") == true) {
                    dpi.bstrName = dpi.bstrName.Substring(1);
                }
                dpi.dwFields |= enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_NAME;
            }

            if (fields.HasFlag(enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_TYPE)) {
                if (valueResult != null) {
                    dpi.bstrType = valueResult.TypeName;
                    if (valueResult.Classes.Count > 0) {
                        dpi.bstrType += " (" + string.Join(", ", valueResult.Classes) + ")";
                    }
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
                    dpi.bstrValue = valueResult.Representation.DPut;
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

                if (IsSynthetic) {
                    dpi.dwAttrib |= enum_DBG_ATTRIB_FLAGS.DBG_ATTRIB_METHOD | enum_DBG_ATTRIB_FLAGS.DBG_ATTRIB_TYPE_VIRTUAL;
                }

                if (valueResult?.HasChildren == true || valueResult?.HasAttributes == true) {
                    dpi.dwAttrib |= enum_DBG_ATTRIB_FLAGS.DBG_ATTRIB_OBJ_IS_EXPANDABLE;
                }

                if (valueResult != null) {
                    dpi.dwAttrib |= enum_DBG_ATTRIB_FLAGS.DBG_ATTRIB_VALUE_RAW_STRING;
                    switch (valueResult.TypeName) {
                        case "logical":
                            dpi.dwAttrib |= enum_DBG_ATTRIB_FLAGS.DBG_ATTRIB_VALUE_BOOLEAN;
                            if (valueResult.Representation.DPut == "TRUE") {
                                dpi.dwAttrib |= enum_DBG_ATTRIB_FLAGS.DBG_ATTRIB_VALUE_BOOLEAN_TRUE;
                            }
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
                    dpi.dwAttrib |= enum_DBG_ATTRIB_FLAGS.DBG_ATTRIB_VALUE_SIDE_EFFECT;
                }
            }

            return dpi;
        }
    }
}