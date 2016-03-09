// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Common.Core;
using Microsoft.R.Support.Settings;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;
using static System.FormattableString;

namespace Microsoft.R.Debugger.Engine {
    internal sealed class AD7Property : IDebugProperty3 {
        internal const int ChildrenMaxLength = 100;
        internal const int ReprMaxLength = 100;

        private const DebugEvaluationResultFields _prefetchedFields =
            DebugEvaluationResultFields.Expression |
            DebugEvaluationResultFields.Kind |
            DebugEvaluationResultFields.Repr |
            DebugEvaluationResultFields.ReprDeparse |
            DebugEvaluationResultFields.TypeName |
            DebugEvaluationResultFields.Classes |
            DebugEvaluationResultFields.Length |
            DebugEvaluationResultFields.SlotCount |
            DebugEvaluationResultFields.AttrCount |
            DebugEvaluationResultFields.Dim |
            DebugEvaluationResultFields.Flags;

        private IDebugProperty2 IDebugProperty2 => this;
        private IDebugProperty3 IDebugProperty3 => this;

        private Lazy<IReadOnlyList<DebugEvaluationResult>> _children;
        private Lazy<string> _reprToString;

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

            _children = Lazy.Create(CreateChildren);
            _reprToString = Lazy.Create(CreateReprToString);
        }

        private IReadOnlyList<DebugEvaluationResult> CreateChildren() {
            return TaskExtensions.RunSynchronouslyOnUIThread(ct => (EvaluationResult as DebugValueEvaluationResult)?.GetChildrenAsync(_prefetchedFields, ChildrenMaxLength, ReprMaxLength, ct))
                ?? new DebugEvaluationResult[0];
        }

        private string CreateReprToString() {
            var ev = TaskExtensions.RunSynchronouslyOnUIThread(ct => EvaluationResult.EvaluateAsync(DebugEvaluationResultFields.Repr | DebugEvaluationResultFields.ReprToString, cancellationToken: ct));
            return (ev as DebugValueEvaluationResult)?.GetRepresentation().ToString;
        }

        int IDebugProperty2.EnumChildren(enum_DEBUGPROP_INFO_FLAGS dwFields, uint dwRadix, ref Guid guidFilter, enum_DBG_ATTRIB_FLAGS dwAttribFilter, string pszNameFilter, uint dwTimeout, out IEnumDebugPropertyInfo2 ppEnum) {
            IEnumerable<DebugEvaluationResult> children = _children.Value;

            if (!RToolsSettings.Current.ShowDotPrefixedVariables) {
                children = children.Where(v => v.Name != null && !v.Name.StartsWith("."));
            }

            if (IsFrameEnvironment) {
                children = children.OrderBy(v => v.Name);
            }

            var infos = children.Select(v => new AD7Property(this, v).GetDebugPropertyInfo(dwRadix, dwFields));

            var valueResult = EvaluationResult as DebugValueEvaluationResult;
            if (valueResult != null && valueResult.HasAttributes == true) {
                string attrExpr = Invariant($"base::attributes({valueResult.Expression})");
                var attrResult = TaskExtensions.RunSynchronouslyOnUIThread(ct => StackFrame.StackFrame.EvaluateAsync(attrExpr, "attributes()", reprMaxLength: ReprMaxLength, cancellationToken:ct));
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
            pcelt = StackFrame.Engine.GridViewProvider?.CanShowDataGrid(EvaluationResult) == true ? 1u : 0u;
            return VSConstants.S_OK;
        }

        int IDebugProperty3.GetCustomViewerList(uint celtSkip, uint celtRequested, DEBUG_CUSTOM_VIEWER[] rgViewers, out uint pceltFetched) {
            if (celtSkip > 0 || celtRequested == 0) {
                pceltFetched = 0;
            } else {
                pceltFetched = 1;
                rgViewers[0] = new DEBUG_CUSTOM_VIEWER {
                    bstrMenuName = "Grid Visualizer",
                    bstrMetric = "CustomViewerCLSID",
                    guidLang = DebuggerGuids.LanguageGuid,
                    guidVendor = DebuggerGuids.VendorGuid,
                    dwID = 0
                };
            }
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

            if (_reprToString.Value == null) {
                return VSConstants.E_FAIL;
            }

            pLen = (uint)_reprToString.Value.Length;
            return VSConstants.S_OK;
        }

        int IDebugProperty3.GetStringChars(uint buflen, ushort[] rgString, out uint pceltFetched) {
            pceltFetched = 0;

            if (_reprToString.Value == null) {
                return VSConstants.E_FAIL;
            }

            for (int i = 0; i < buflen; ++i) {
                rgString[i] = _reprToString.Value[i];
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
            var setResult = TaskExtensions.RunSynchronouslyOnUIThread(ct => EvaluationResult.SetValueAsync(pszValue, ct)) as DebugErrorEvaluationResult;
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
                dpi.bstrName = EvaluationResult.Name ?? EvaluationResult.Expression;
                if (Parent?.IsFrameEnvironment == true && dpi.bstrName?.StartsWith("$") == true) {
                    dpi.bstrName = dpi.bstrName.Substring(1);
                }
                dpi.dwFields |= enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_NAME;
            }

            if (fields.HasFlag(enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_TYPE)) {
                if (valueResult != null) {
                    dpi.bstrType = valueResult.TypeName;
                    if (valueResult.Classes != null && valueResult.Classes.Count > 0) {
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
                    dpi.bstrValue = valueResult.GetRepresentation().Deparse;
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

                if (StackFrame.Engine.GridViewProvider?.CanShowDataGrid(EvaluationResult) == true) {
                    dpi.dwAttrib |= enum_DBG_ATTRIB_FLAGS.DBG_ATTRIB_VALUE_CUSTOM_VIEWER;
                }

                if (valueResult?.HasChildren == true || valueResult?.HasAttributes == true) {
                    dpi.dwAttrib |= enum_DBG_ATTRIB_FLAGS.DBG_ATTRIB_OBJ_IS_EXPANDABLE;
                }

                if (valueResult != null) {
                    switch (valueResult.TypeName) {
                        case "logical":
                            dpi.dwAttrib |= enum_DBG_ATTRIB_FLAGS.DBG_ATTRIB_VALUE_BOOLEAN;
                            if (valueResult.GetRepresentation().Deparse == "TRUE") {
                                dpi.dwAttrib |= enum_DBG_ATTRIB_FLAGS.DBG_ATTRIB_VALUE_BOOLEAN_TRUE;
                            }
                            break;
                        case "closure":
                            dpi.dwAttrib |= enum_DBG_ATTRIB_FLAGS.DBG_ATTRIB_METHOD;
                            break;
                        case "character":
                        case "symbol":
                            if (valueResult.Length == 1) {
                                dpi.dwAttrib |= enum_DBG_ATTRIB_FLAGS.DBG_ATTRIB_VALUE_RAW_STRING;
                            }
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