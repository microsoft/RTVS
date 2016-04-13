// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.R.Debugger;
using Microsoft.R.Host.Client;

namespace Microsoft.R.Editor.Data {
    /// <summary>
    /// Model for variable tree grid, that provides UI customization of <see cref="DebugEvaluationResult"/>
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
        public RSessionDataObject(DebugEvaluationResult evaluation, int? maxChildrenCount = null) : this() {
            DebugEvaluation = evaluation;

            Name = DebugEvaluation.Name?.TrimStart(NameTrimChars);

            if (DebugEvaluation is DebugValueEvaluationResult) {
                var valueEvaluation = (DebugValueEvaluationResult)DebugEvaluation;

                Value = GetValue(valueEvaluation)?.Trim();
                ValueDetail = valueEvaluation.GetRepresentation().Deparse;
                TypeName = valueEvaluation.TypeName;

                if (valueEvaluation.Classes != null) {
                    var escaped = valueEvaluation.Classes.Select((x) => x.IndexOf(' ') >= 0 ? "'" + x + "'" : x);
                    Class = string.Join(", ", escaped); // TODO: escape ',' in class names
                }

                HasChildren = valueEvaluation.HasChildren;

                Dimensions = valueEvaluation.Dim ?? new List<int>();
            } else if (DebugEvaluation is DebugPromiseEvaluationResult) {
                const string PromiseValue = "<promise>";

                Value = PromiseValue;
                TypeName = PromiseValue;
                Class = PromiseValue;
            } else if (DebugEvaluation is DebugActiveBindingEvaluationResult) {
                const string ActiveBindingValue = "<active binding>";
                var activeBinding = (DebugActiveBindingEvaluationResult)DebugEvaluation;

                Value = ActiveBindingValue;
                TypeName = ActiveBindingValue;
                Class = ActiveBindingValue;
            }

            if (Dimensions == null) Dimensions = new List<int>();

            MaxChildrenCount = maxChildrenCount;
        }

        protected int? MaxChildrenCount { get; set; }

        protected int MaxReprLength { get; set; }

        protected DebugEvaluationResult DebugEvaluation { get; }

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

            var valueEvaluation = DebugEvaluation as DebugValueEvaluationResult;
            if (valueEvaluation == null) {
                Debug.Assert(false, $"{nameof(RSessionDataObject)} result type is not {typeof(DebugValueEvaluationResult)}");
                return result;
            }

            if (valueEvaluation.HasChildren) {
                await TaskUtilities.SwitchToBackgroundThread();

                const DebugEvaluationResultFields fields =
                    DebugEvaluationResultFields.Expression |
                    DebugEvaluationResultFields.Kind |
                    DebugEvaluationResultFields.ReprStr |
                    DebugEvaluationResultFields.TypeName |
                    DebugEvaluationResultFields.Classes |
                    DebugEvaluationResultFields.Length |
                    DebugEvaluationResultFields.SlotCount |
                    DebugEvaluationResultFields.AttrCount |
                    DebugEvaluationResultFields.Dim |
                    DebugEvaluationResultFields.Flags;
                IReadOnlyList<DebugEvaluationResult> children = await valueEvaluation.GetChildrenAsync(fields, MaxChildrenCount, MaxReprLength);
                result = EvaluateChildren(children);
            }

            return result;
        }

        protected virtual List<IRSessionDataObject> EvaluateChildren(IReadOnlyList<DebugEvaluationResult> children) {
            var result = new List<IRSessionDataObject>();
            for (int i = 0; i < children.Count; i++) {
                result.Add(new RSessionDataObject(children[i], GetMaxChildrenCount(children[i])));
            }
            return result;
        }

        protected int? GetMaxChildrenCount(DebugEvaluationResult parent) {
            var value = parent as DebugValueEvaluationResult;
            if (value != null) {
                if (value.Classes.Contains("data.frame")) {
                    return null;
                }
            }
            return DefaultMaxGrandChildren;
        }

        private static string DataFramePrefix = "'data.frame':([^:]+):";
        private string GetValue(DebugValueEvaluationResult v) {
            var value = v.GetRepresentation().Str;
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

        public string ValueDetail { get; protected set; }

        public string TypeName { get; protected set; }

        public string Class { get; protected set; }

        public bool HasChildren { get; protected set; }

        public IReadOnlyList<int> Dimensions { get; protected set; }

        public bool IsHidden {
            get { return Name.StartsWith(HiddenVariablePrefix); }
        }

        public string Expression {
            get {
                return DebugEvaluation.Expression;
            }
        }
        #endregion
    }
}
