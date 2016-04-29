// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.R.DataInspection;
using Microsoft.R.Host.Client;

namespace Microsoft.R.Editor.Data {
    /// <summary>
    /// Model for variable tree grid, that provides UI customization of <see cref="REvaluationInfo"/>
    /// </summary>
    public class RSessionDataObject : IRSessionDataObject {
        private static readonly char[] NameTrimChars = new char[] { '$' };
        private static readonly string HiddenVariablePrefix = ".";

        private readonly object syncObj = new object();
        private Task<IReadOnlyList<IRSessionDataObject>> _getChildrenTask = null;

        protected const int DefaultMaxReprLength = 100;
        protected const int DefaultMaxGrandChildren = 20;

        protected RSessionDataObject() {
            MaxReprLength = DefaultMaxReprLength;
        }

        /// <summary>
        /// Create new instance of <see cref="DataEvaluation"/>
        /// </summary>
        /// <param name="evaluation">R session's evaluation result</param>
        public RSessionDataObject(IREvaluationInfo evaluation, int? maxChildrenCount = null) : this() {
            DebugEvaluation = evaluation;

            Name = DebugEvaluation.Name?.TrimStart(NameTrimChars);

            if (DebugEvaluation is IRValueInfo) {
                var valueEvaluation = (IRValueInfo)DebugEvaluation;

                Value = GetValue(valueEvaluation)?.Trim();
                TypeName = valueEvaluation.TypeName;

                if (valueEvaluation.Classes != null) {
                    var escaped = valueEvaluation.Classes.Select((x) => x.IndexOf(' ') >= 0 ? "'" + x + "'" : x);
                    Class = string.Join(", ", escaped); // TODO: escape ',' in class names
                }

                HasChildren = valueEvaluation.HasChildren;

                if (valueEvaluation.Dim != null) {
                    Dimensions = valueEvaluation.Dim;
                } else if(valueEvaluation.Length.HasValue) {
                    Dimensions = new List<int>() { valueEvaluation.Length.Value, 1 };
                } else {
                    Dimensions = new List<int>();
                }
            } else if (DebugEvaluation is IRPromiseInfo) {
                const string PromiseValue = "<promise>";

                Value = PromiseValue;
                TypeName = PromiseValue;
                Class = PromiseValue;
            } else if (DebugEvaluation is IRActiveBindingInfo) {
                const string ActiveBindingValue = "<active binding>";
                var activeBinding = (IRActiveBindingInfo)DebugEvaluation;

                Value = ActiveBindingValue;
                TypeName = ActiveBindingValue;
                Class = ActiveBindingValue;
            }

            if (Dimensions == null) Dimensions = new List<int>();

            MaxChildrenCount = maxChildrenCount;
        }

        protected int? MaxChildrenCount { get; set; }

        protected int MaxReprLength { get; set; }

        protected IREvaluationInfo DebugEvaluation { get; }

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
            List<IRSessionDataObject> result = null;

            var valueEvaluation = DebugEvaluation as IRValueInfo;
            if (valueEvaluation == null) {
                Debug.Assert(false, $"{nameof(RSessionDataObject)} result type is not {nameof(IRValueInfo)}");
                return result;
            }

            if (valueEvaluation.HasChildren) {
                await TaskUtilities.SwitchToBackgroundThread();

                const RValueProperties fields =
                    RValueProperties.Expression |
                    RValueProperties.Kind |
                    RValueProperties.TypeName |
                    RValueProperties.Classes |
                    RValueProperties.Length |
                    RValueProperties.SlotCount |
                    RValueProperties.AttrCount |
                    RValueProperties.Dim |
                    RValueProperties.Flags;
                var children = await valueEvaluation.DescribeChildrenAsync(fields, MaxChildrenCount, RValueRepresentations.Str(MaxReprLength));
                result = EvaluateChildren(children);
            }

            return result;
        }

        protected virtual List<IRSessionDataObject> EvaluateChildren(IReadOnlyList<IREvaluationInfo> children) {
            var result = new List<IRSessionDataObject>();
            for (int i = 0; i < children.Count; i++) {
                result.Add(new RSessionDataObject(children[i], GetMaxChildrenCount(children[i])));
            }
            return result;
        }

        protected int? GetMaxChildrenCount(IREvaluationInfo parent) {
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
                Match match = Regex.Match(value, DataFramePrefix);
                if (match.Success) {
                    return match.Groups[1].Value.Trim();
                }
            }
            return value != null ? value.ConvertCharacterCodes() : value;
        }

        #region IRSessionDataObject

        public string Name { get; protected set; }

        public string Value { get; protected set; }

        public string TypeName { get; protected set; }

        public string Class { get; protected set; }

        public bool HasChildren { get; protected set; }

        public IReadOnlyList<int> Dimensions { get; protected set; }

        public bool IsHidden {
            get { return Name.StartsWithOrdinal(HiddenVariablePrefix); }
        }

        public string Expression {
            get {
                return DebugEvaluation.Expression;
            }
        }
        #endregion
    }
}
