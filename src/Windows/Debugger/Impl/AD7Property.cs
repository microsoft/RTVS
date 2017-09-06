// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Components.Settings;
using Microsoft.R.DataInspection;
using Microsoft.R.Host.Client;
using Microsoft.R.StackTracing;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;
using static System.FormattableString;
using static Microsoft.R.DataInspection.REvaluationResultProperties;

namespace Microsoft.R.Debugger {
    internal sealed class AD7Property : IDebugProperty3 {
        internal const int ChildrenMaxCount = 100;
        internal static readonly string Repr = RValueRepresentations.Deparse(100);

        internal const REvaluationResultProperties PrefetchedProperties =
            ExpressionProperty |
            AccessorKindProperty |
            TypeNameProperty |
            ClassesProperty |
            LengthProperty |
            SlotCountProperty |
            AttributeCountProperty |
            DimProperty |
            FlagsProperty;

        private IDebugProperty2 IDebugProperty2 => this;
        private IDebugProperty3 IDebugProperty3 => this;

        private Lazy<IReadOnlyList<IREvaluationResultInfo>> _children;
        private Lazy<string> _reprToString;
        private IRSettings _settings;

        public AD7Property Parent { get; }
        public AD7StackFrame StackFrame { get; }
        public IREvaluationResultInfo EvaluationResult { get; }
        public bool IsSynthetic { get; }
        public bool IsFrameEnvironment { get; }

        public AD7Property(AD7Property parent, IREvaluationResultInfo result, bool isSynthetic = false)
            : this(parent.StackFrame, result, isSynthetic, false) {
            Parent = parent;
        }

        public AD7Property(AD7StackFrame stackFrame, IREvaluationResultInfo result, bool isSynthetic = false, bool isFrameEnvironment = false) {
            StackFrame = stackFrame;
            EvaluationResult = result;
            IsSynthetic = isSynthetic;
            IsFrameEnvironment = isFrameEnvironment;

            _children = Lazy.Create(CreateChildren);
            _reprToString = Lazy.Create(GetReprToString);
            _settings = stackFrame.Engine.Shell.GetService<IRSettings>();
        }

        private IReadOnlyList<IREvaluationResultInfo> CreateChildren() =>
            TaskExtensions.RunSynchronouslyOnUIThread(async ct => {
                var valueResult = EvaluationResult as IRValueInfo;
                if (valueResult == null) {
                    return new IREvaluationResultInfo[0];
                }

                var properties = _settings.EvaluateActiveBindings ? REvaluationResultProperties.ComputedValueProperty : 0;
                properties |= PrefetchedProperties;
                var children = await valueResult.DescribeChildrenAsync(properties, Repr, ChildrenMaxCount, ct);

                // Children of environments do not have any meaningful order, so sort them by name.
                if (valueResult.TypeName == "environment") {
                    children = children.OrderBy(er => er.Name).ToArray();
                }

                return children;
            });

        private string GetReprToString() {
            var code = Invariant($"rtvs:::repr_toString(eval(quote({EvaluationResult.Expression}), envir = {EvaluationResult.EnvironmentExpression}))");
            return TaskExtensions.RunSynchronouslyOnUIThread(ct => StackFrame.Engine.Tracer.Session.EvaluateAsync<string>(code, REvaluationKind.Normal, ct));
        }

        int IDebugProperty2.EnumChildren(enum_DEBUGPROP_INFO_FLAGS dwFields, uint dwRadix, ref Guid guidFilter, enum_DBG_ATTRIB_FLAGS dwAttribFilter, string pszNameFilter, uint dwTimeout, out IEnumDebugPropertyInfo2 ppEnum) {
            IEnumerable<IREvaluationResultInfo> children = _children.Value;

            if (!_settings.ShowDotPrefixedVariables) {
                children = children.Where(v => v.Name != null && !v.Name.StartsWithOrdinal("."));
            }

            var infos = children.Select(v => new AD7Property(this, v).GetDebugPropertyInfo(dwRadix, dwFields));

            var valueResult = EvaluationResult as IRValueInfo;
            if (valueResult != null) {
                if (valueResult.HasAttributes() == true) {
                    var attrExpr = Invariant($"base::attributes({valueResult.Expression})");
                    var attrResult = TaskExtensions.RunSynchronouslyOnUIThread(ct => StackFrame.StackFrame.TryEvaluateAndDescribeAsync(attrExpr, "attributes()", PrefetchedProperties, Repr, ct));
                    if (!(attrResult is IRErrorInfo)) {
                        var attrInfo = new AD7Property(this, attrResult, isSynthetic: true).GetDebugPropertyInfo(dwRadix, dwFields);
                        infos = new[] { attrInfo }.Concat(infos);
                    }
                }

                if (valueResult.Flags.HasFlag(RValueFlags.HasParentEnvironment)) {
                    var parentExpr = Invariant($"base::parent.env({valueResult.Expression})");
                    var parentResult = TaskExtensions.RunSynchronouslyOnUIThread(ct => StackFrame.StackFrame.TryEvaluateAndDescribeAsync(parentExpr, "parent.env()", PrefetchedProperties, Repr, ct));
                    if (!(parentResult is IRErrorInfo)) {
                        var parentInfo = new AD7Property(this, parentResult, isSynthetic: true).GetDebugPropertyInfo(dwRadix, dwFields);
                        infos = new[] { parentInfo }.Concat(infos);
                    }
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

        int IDebugProperty2.SetValueAsReference(IDebugReference2[] rgpArgs, uint dwArgCount, IDebugReference2 pValue, uint dwTimeout) => VSConstants.E_NOTIMPL;

        int IDebugProperty2.SetValueAsString(string pszValue, uint dwRadix, uint dwTimeout) {
            string errorString;
            return IDebugProperty3.SetValueAsStringWithError(pszValue, dwRadix, dwTimeout, out errorString);
        }

        int IDebugProperty3.SetValueAsString(string pszValue, uint dwRadix, uint dwTimeout) {
            return IDebugProperty2.SetValueAsString(pszValue, dwRadix, dwTimeout);
        }

        int IDebugProperty3.CreateObjectID() => VSConstants.E_NOTIMPL;
        int IDebugProperty3.DestroyObjectID() => VSConstants.E_NOTIMPL;

        int IDebugProperty3.EnumChildren(enum_DEBUGPROP_INFO_FLAGS dwFields, uint dwRadix, ref Guid guidFilter, enum_DBG_ATTRIB_FLAGS dwAttribFilter, string pszNameFilter, uint dwTimeout, out IEnumDebugPropertyInfo2 ppEnum)
            => IDebugProperty2.EnumChildren(dwFields, dwRadix, guidFilter, dwAttribFilter, pszNameFilter, dwTimeout, out ppEnum);

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

        int IDebugProperty3.GetDerivedMostProperty(out IDebugProperty2 ppDerivedMost)=> IDebugProperty2.GetDerivedMostProperty(out ppDerivedMost);
        int IDebugProperty3.GetExtendedInfo(ref Guid guidExtendedInfo, out object pExtendedInfo) => IDebugProperty2.GetExtendedInfo(guidExtendedInfo, out pExtendedInfo);
        int IDebugProperty3.GetMemoryBytes(out IDebugMemoryBytes2 ppMemoryBytes) => IDebugProperty2.GetMemoryBytes(out ppMemoryBytes);
        int IDebugProperty3.GetMemoryContext(out IDebugMemoryContext2 ppMemory) => IDebugProperty2.GetMemoryContext(out ppMemory);
        int IDebugProperty3.GetParent(out IDebugProperty2 ppParent) => IDebugProperty2.GetParent(out ppParent);
        int IDebugProperty3.GetPropertyInfo(enum_DEBUGPROP_INFO_FLAGS dwFields, uint dwRadix, uint dwTimeout, IDebugReference2[] rgpArgs, uint dwArgCount, DEBUG_PROPERTY_INFO[] pPropertyInfo)
            => IDebugProperty2.GetPropertyInfo(dwFields, dwRadix, dwTimeout, rgpArgs, dwArgCount, pPropertyInfo);
        int IDebugProperty3.GetSize(out uint pdwSize) => IDebugProperty2.GetSize(out pdwSize);

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

            for (var i = 0; i < buflen; ++i) {
                rgString[i] = _reprToString.Value[i];
            }
            return VSConstants.S_OK;
        }

        int IDebugProperty3.GetReference(out IDebugReference2 ppReference) => IDebugProperty2.GetReference(out ppReference);
        int IDebugProperty3.SetValueAsReference(IDebugReference2[] rgpArgs, uint dwArgCount, IDebugReference2 pValue, uint dwTimeout)
            => IDebugProperty2.SetValueAsReference(rgpArgs, dwArgCount, pValue, dwTimeout);

        int IDebugProperty3.SetValueAsStringWithError(string pszValue, uint dwRadix, uint dwTimeout, out string errorString) {
            errorString = null;

            // TODO: dwRadix
            try {
                TaskExtensions.RunSynchronouslyOnUIThread(ct => EvaluationResult.AssignAsync(pszValue, ct));
            } catch (RException ex) { 
                errorString = ex.Message;
                return VSConstants.E_FAIL;
            }

            return VSConstants.S_OK;
        }

        internal DEBUG_PROPERTY_INFO GetDebugPropertyInfo(uint radix, enum_DEBUGPROP_INFO_FLAGS fields) {
            var dpi = new DEBUG_PROPERTY_INFO {pProperty = this};

            // Always provide the property so that we can access locals from the automation object.
            dpi.dwFields |= enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_PROP;

            var valueInfo = EvaluationResult as IRValueInfo;
            var errorInfo = EvaluationResult as IRErrorInfo;
            var promiseInfo = EvaluationResult as IRPromiseInfo;
            var activeBindingInfo = EvaluationResult as IRActiveBindingInfo;

            if (fields.HasFlag(enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_FULLNAME)) {
                dpi.bstrFullName = EvaluationResult.Expression;
                dpi.dwFields |= enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_FULLNAME;
            }

            if (fields.HasFlag(enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_NAME)) {
                dpi.bstrName = EvaluationResult.Name ?? EvaluationResult.Expression;
                if (Parent?.IsFrameEnvironment == true && dpi.bstrName?.StartsWithOrdinal("$") == true) {
                    dpi.bstrName = dpi.bstrName.Substring(1);
                }
                dpi.dwFields |= enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_NAME;
            }

            if (fields.HasFlag(enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_TYPE)) {
                if (valueInfo != null) {
                    dpi.bstrType = valueInfo.TypeName;
                    if (valueInfo.Classes != null && valueInfo.Classes.Count > 0) {
                        dpi.bstrType += " (" + string.Join(", ", valueInfo.Classes) + ")";
                    }
                    dpi.dwFields |= enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_TYPE;
                } else if (promiseInfo != null) {
                    dpi.bstrType = "<promise>";
                    dpi.dwFields |= enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_TYPE;
                } else if (activeBindingInfo != null) {
                    if (activeBindingInfo.ComputedValue != null) {
                        dpi.bstrType = activeBindingInfo.ComputedValue.TypeName;
                        if (activeBindingInfo.ComputedValue.Classes != null && activeBindingInfo.ComputedValue.Classes.Count > 0) {
                            dpi.bstrType += " (" + string.Join(", ", activeBindingInfo.ComputedValue.Classes) + ")";
                        }
                    } else {
                        dpi.bstrType = "<active binding>";
                    }
                    
                    dpi.dwFields |= enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_TYPE;
                }
            }

            if (fields.HasFlag(enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_VALUE)) {
                if (valueInfo != null && valueInfo.Representation != null) {
                    // TODO: handle radix
                    dpi.bstrValue = valueInfo.Representation.ToUnicodeQuotes();
                    dpi.dwFields |= enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_VALUE;
                } else if (promiseInfo != null) {
                    dpi.bstrValue = promiseInfo.Code;
                    dpi.dwFields |= enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_VALUE;
                } else if (errorInfo != null) {
                    dpi.bstrValue = errorInfo.ErrorText;
                    dpi.dwFields |= enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_VALUE;
                } else if (activeBindingInfo != null) {
                    if (activeBindingInfo.ComputedValue != null) {
                        dpi.bstrValue = activeBindingInfo.ComputedValue.Representation;
                        dpi.dwFields |= enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_VALUE;
                    }
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

                if (valueInfo?.HasChildren == true || valueInfo?.HasAttributes() == true) {
                    dpi.dwAttrib |= enum_DBG_ATTRIB_FLAGS.DBG_ATTRIB_OBJ_IS_EXPANDABLE;
                }

                if (valueInfo != null) {
                    switch (valueInfo.TypeName) {
                        case "logical":
                            dpi.dwAttrib |= enum_DBG_ATTRIB_FLAGS.DBG_ATTRIB_VALUE_BOOLEAN;
                            if (valueInfo.Representation == "TRUE") {
                                dpi.dwAttrib |= enum_DBG_ATTRIB_FLAGS.DBG_ATTRIB_VALUE_BOOLEAN_TRUE;
                            }
                            break;
                        case "closure":
                            dpi.dwAttrib |= enum_DBG_ATTRIB_FLAGS.DBG_ATTRIB_METHOD;
                            break;
                        case "character":
                        case "symbol":
                            if (valueInfo.Length == 1) {
                                dpi.dwAttrib |= enum_DBG_ATTRIB_FLAGS.DBG_ATTRIB_VALUE_RAW_STRING;
                            }
                            break;
                    }
                } else if (errorInfo != null) {
                    dpi.dwAttrib |= enum_DBG_ATTRIB_FLAGS.DBG_ATTRIB_VALUE_ERROR;
                } else if (promiseInfo != null) {
                    dpi.dwAttrib |= enum_DBG_ATTRIB_FLAGS.DBG_ATTRIB_VALUE_SIDE_EFFECT;
                } else if (activeBindingInfo != null) {
                    dpi.dwAttrib |= enum_DBG_ATTRIB_FLAGS.DBG_ATTRIB_PROPERTY;
                    dpi.dwAttrib |= enum_DBG_ATTRIB_FLAGS.DBG_ATTRIB_VALUE_SIDE_EFFECT;
                }
            }

            return dpi;
        }
    }
}