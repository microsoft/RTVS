using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
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
                ValueDetail = valueEvaluation.Representation.Deparse;
                TypeName = valueEvaluation.TypeName;

                if (valueEvaluation.Classes != null) {
                    var escaped = valueEvaluation.Classes.Select((x) => x.IndexOf(' ') >= 0 ? "'" + x + "'" : x);
                    Class = string.Join(", ", escaped); // TODO: escape ',' in class names
                }

                HasChildren = valueEvaluation.HasChildren;

                Dimensions = valueEvaluation.Dim ?? new List<int>();
            } else if (DebugEvaluation is DebugPromiseEvaluationResult) {
                const string PromiseVaue = "<promise>";
                var promise = (DebugPromiseEvaluationResult)DebugEvaluation;

                Value = promise.Code;
                TypeName = PromiseVaue;
                Class = PromiseVaue;
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

                var fields = (DebugEvaluationResultFields.All & ~DebugEvaluationResultFields.ReprAll) |
                        DebugEvaluationResultFields.Repr | DebugEvaluationResultFields.ReprStr;

                // assumption: DebugEvaluationResult returns children in ascending order
                IReadOnlyList<DebugEvaluationResult> children = await valueEvaluation.GetChildrenAsync(fields, MaxChildrenCount, MaxReprLength);
                result = EvaluateChildren(children);
            }

            return result;
        }

        protected virtual List<IRSessionDataObject> EvaluateChildren(IReadOnlyList<DebugEvaluationResult> children) {
            var result = new List<IRSessionDataObject>();
            for (int i = 0; i < children.Count; i++) {
                result.Add(new RSessionDataObject(children[i], DefaultMaxGrandChildren));
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
            return value != null ? ConvertCharacterCodes(value) : value;
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

        /// <summary>
        /// Convert R string that comes encoded into &lt;U+ABCD&gt; into Unicode
        /// characters so user can see actual language symbols rather than 
        /// the character codes. Trims trailing '| __truncated__' that R tends 
        /// to append at the end.
        /// </summary>
        private string ConvertCharacterCodes(string s) {
            int t = s.IndexOf("\"| __truncated__");
            if (t >= 0) {
                s = s.Substring(0, t);
            }

            if (s.IndexOf("<U+") < 0) {
                // Nothing to convert
                return s;
            }

            char[] converted = new char[s.Length];
            int j = 0;
            for (int i = 0; i < s.Length;) {
                if (i < s.Length - 8 &&
                    s[i] == '<' && s[i + 1] == 'U' && s[i + 2] == '+' && s[i + 7] == '>') {
                    int code = s.SubstringToHex(i + 3, 4);
                    if (code > 0 && code < 65535) {
                        converted[j++] = Convert.ToChar(code);
                        i += 8;
                        continue;
                    }
                }
                converted[j++] = s[i++];
            }
            return new string(converted, 0, j);
        }
    }
}
