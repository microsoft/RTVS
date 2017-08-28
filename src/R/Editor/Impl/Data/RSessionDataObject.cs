// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.R.DataInspection;
using Microsoft.R.Host.Client;
using static Microsoft.R.DataInspection.REvaluationResultProperties;

namespace Microsoft.R.Editor.Data {
    /// <summary>
    /// Model for variable tree grid, that provides UI customization of <see cref="REvaluationInfo"/>
    /// </summary>
    public class RSessionDataObject : IRSessionDataObject {
        private static readonly char[] NameTrimChars = new char[] { '$' };
        private static readonly string HiddenVariablePrefix = ".";

        private readonly bool _evaluateActiveBindings;
        private readonly object syncObj = new object();
        private Task<IReadOnlyList<IRSessionDataObject>> _getChildrenTask;

        protected const int DefaultMaxReprLength = 100;
        protected const int DefaultMaxGrandChildren = 20;

        protected RSessionDataObject() {
            MaxReprLength = DefaultMaxReprLength;
        }

        public RSessionDataObject(IREvaluationResultInfo evaluation, bool evaluateActiveBindings, int? maxChildrenCount = null) : this() {
            DebugEvaluation = evaluation;
            Name = DebugEvaluation.Name?.TrimStart(NameTrimChars);
            _evaluateActiveBindings = evaluateActiveBindings;

            if (DebugEvaluation is IRValueInfo) {
                var valueEvaluation = (IRValueInfo)DebugEvaluation;
                Initalize(valueEvaluation);
            } else if (DebugEvaluation is IRPromiseInfo) {
                var promiseInfo = (IRPromiseInfo)DebugEvaluation;
                Value = promiseInfo.Code;
                Class = TypeName = "<promise>";
            } else if (DebugEvaluation is IRActiveBindingInfo) {
                const string ActiveBindingText = "<active binding>";
                var activeBinding = (IRActiveBindingInfo)DebugEvaluation;
                if(activeBinding.ComputedValue != null) {
                    Initalize(activeBinding.ComputedValue);
                } else {
                    Value = ActiveBindingText;
                    TypeName = ActiveBindingText;
                    Class = ActiveBindingText;
                }
            }

            if (Dimensions == null) {
                Dimensions = new List<long>();
            }

            MaxChildrenCount = maxChildrenCount;
        }

        private void Initalize(IRValueInfo valueInfo) {
            Value = GetValue(valueInfo)?.Trim();
            TypeName = valueInfo.TypeName;

            if (valueInfo.Classes != null) {
                var escaped = valueInfo.Classes.Select((x) => x.IndexOf(' ') >= 0 ? "'" + x + "'" : x);
                Class = string.Join(", ", escaped); // TODO: escape ',' in class names
            }

            HasChildren = valueInfo.HasChildren;

            if (valueInfo.Dim != null) {
                Dimensions = valueInfo.Dim;
            } else if (valueInfo.Length.HasValue) {
                Dimensions = new List<long>() { valueInfo.Length.Value, 1 };
            } else {
                Dimensions = new List<long>();
            }
        }

        protected int? MaxChildrenCount { get; set; }

        protected int MaxReprLength { get; set; }

        public IREvaluationResultInfo DebugEvaluation { get; }

        public Task<IReadOnlyList<IRSessionDataObject>> GetChildrenAsync() {
            if (_getChildrenTask == null) {
                lock (syncObj) {
                    if (_getChildrenTask == null) {
                        _getChildrenTask = GetChildrenAsyncInternal();
                    }
                }
            }

            return _getChildrenTask;
        }

        protected virtual async Task<IReadOnlyList<IRSessionDataObject>> GetChildrenAsyncInternal() {
            var valueEvaluation = DebugEvaluation as IRValueInfo;
            if (valueEvaluation == null) {
                // Failed to fetch children - for example, because the object is already gone.
                return null;
            }

            await TaskUtilities.SwitchToBackgroundThread();
            try {
                if (valueEvaluation.HasChildren) {
                    var properties =
                        ExpressionProperty |
                        AccessorKindProperty |
                        TypeNameProperty |
                        ClassesProperty |
                        LengthProperty |
                        SlotCountProperty |
                        AttributeCountProperty |
                        DimProperty |
                        FlagsProperty |
                        (_evaluateActiveBindings ? ComputedValueProperty : 0);
                    var children = await valueEvaluation.DescribeChildrenAsync(properties, RValueRepresentations.Str(MaxReprLength), MaxChildrenCount);
                    return EvaluateChildren(children);
                }
            } catch(REvaluationException) { }

            return null;
        }

        protected virtual List<IRSessionDataObject> EvaluateChildren(IReadOnlyList<IREvaluationResultInfo> children) {
            var result = new List<IRSessionDataObject>();
            for (var i = 0; i < children.Count; i++) {
                result.Add(new RSessionDataObject(children[i], _evaluateActiveBindings, GetMaxChildrenCount(children[i])));
            }
            return result;
        }

        protected int? GetMaxChildrenCount(IREvaluationResultInfo parent) {
            var value = parent as IRValueInfo;
            if (value != null) {
                if (value.Classes.Contains("data.frame")) {
                    return null;
                }
            }
            return DefaultMaxGrandChildren;
        }

        private static string DataFramePrefix = "'data.frame':([^:]+):";
        private string GetValue(IRValueInfo v) {
            var value = v.Representation;
            if (value != null) {
                var match = Regex.Match(value, DataFramePrefix);
                if (match.Success) {
                    return match.Groups[1].Value.Trim();
                }
            }
            return value?.ConvertCharacterCodes();
        }

        #region IRSessionDataObject

        public string Name { get; protected set; }

        public string Value { get; protected set; }

        public string TypeName { get; protected set; }

        public string Class { get; protected set; }

        public bool HasChildren { get; protected set; }

        public IReadOnlyList<long> Dimensions { get; protected set; }

        public bool IsHidden => Name.StartsWithOrdinal(HiddenVariablePrefix);

        public string Expression => DebugEvaluation.Expression;

        #endregion
    }
}
