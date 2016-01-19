using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.R.Debugger;

namespace Microsoft.R.Editor.Data {
    /// <summary>
    /// Model for variable tree grid, that provides UI customization of <see cref="DebugEvaluationResult"/>
    /// </summary>
    public class RSessionDataObject : IRSessionDataObject {
        private readonly DebugEvaluationResult _evaluation;

        private static readonly char[] NameTrimChars = new char[] { '$' };
        private static readonly string HiddenVariablePrefix = ".";
        private static readonly char[] NewLineDelimiter = new char[] { '\r', '\n' };

        private readonly bool _truncateChildren;
        private readonly object syncObj = new object();
        private Task<IReadOnlyList<IRSessionDataObject>> _getChildrenTask = null;

        protected RSessionDataObject() { Index = -1; }

        /// <summary>
        /// Create new instance of <see cref="DataEvaluation"/>
        /// </summary>
        /// <param name="evaluation">R session's evaluation result</param>
        /// <param name="truncateChildren">true to truncate children returned by GetChildrenAsync</param>
        public RSessionDataObject(int index, DebugEvaluationResult evaluation, bool truncateChildren) {
            _evaluation = evaluation;
            _truncateChildren = truncateChildren;

            Name = _evaluation.Name.TrimStart(NameTrimChars);

            if (_evaluation is DebugValueEvaluationResult) {
                var valueEvaluation = (DebugValueEvaluationResult)_evaluation;

                Value = GetValue(valueEvaluation).Trim();
                ValueDetail = valueEvaluation.Representation.DPut;
                TypeName = valueEvaluation.TypeName;
                var escaped = valueEvaluation.Classes.Select((x) => x.IndexOf(' ') >= 0 ? "'" + x + "'" : x);
                Class = string.Join(", ", escaped); // TODO: escape ',' in class names
                HasChildren = valueEvaluation.HasChildren;

                CanShowDetail = ComputeDetailAvailability(valueEvaluation);
                if (CanShowDetail) {
                    Dimensions = valueEvaluation.Dim;
                } else {
                    Dimensions = new List<int>();
                }
            }
        }

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

        private async Task<IReadOnlyList<IRSessionDataObject>> GetChildrenAsyncInternal() {
            List<IRSessionDataObject> result = null;

            var valueEvaluation = _evaluation as DebugValueEvaluationResult;
            if (valueEvaluation == null) {
                Debug.Assert(false, $"EvaluationWrapper result type is not {typeof(DebugValueEvaluationResult)}");
                return result;
            }

            if (valueEvaluation.HasChildren) {
                await TaskUtilities.SwitchToBackgroundThread();

                var fields = (DebugEvaluationResultFields.All & ~DebugEvaluationResultFields.ReprAll) |
                        DebugEvaluationResultFields.Repr | DebugEvaluationResultFields.ReprStr;

                // assumption: DebugEvaluationResult returns children in ascending order
                IReadOnlyList<DebugEvaluationResult> children = await valueEvaluation.GetChildrenAsync(fields, _truncateChildren ? (int?)20 : null, 100);    // TODO: consider exception propagation such as OperationCanceledException
                result = EvaluateChildren(children, _truncateChildren);
            }

            return result;
        }

        protected virtual List<IRSessionDataObject> EvaluateChildren(IReadOnlyList<DebugEvaluationResult> children, bool truncateChildren) {
            var result = new List<IRSessionDataObject>();
            for (int i = 0; i < children.Count; i++) {
                result.Add(new RSessionDataObject(i, children[i], _truncateChildren));
            }
            return result;
        }

        private static string DataFramePrefix = "'data.frame':([^:]+):";
        private string GetValue(DebugValueEvaluationResult v) {
            var value = v.Representation.Str;
            if (value != null) {
                Match match = Regex.Match(value, DataFramePrefix);
                if (match.Success) {
                    return match.Groups[1].Value.Trim();
                }
            }
            return value;
        }

        private static string[] detailClasses = new string[] { "matrix", "data.frame", "table" };
        private bool ComputeDetailAvailability(DebugValueEvaluationResult evaluation) {
            if (evaluation.Classes.Any(t => detailClasses.Contains(t))) {
                if (evaluation.Dim != null && evaluation.Dim.Count == 2) {
                    return true;
                }
            }
            return false;
        }

        #region IRSessionDataObject
        /// <summary>
        /// Index returned from evaluation provider, Sort is based on this, and assumes that DebugEvaluationResult returns in ascending order
        /// </summary>
        public int Index { get; }

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
                return _evaluation.Expression;
            }
        }

        public bool CanShowDetail { get; protected set; }
        #endregion
    }
}
