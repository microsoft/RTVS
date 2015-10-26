using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.R.Debugger;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    /// <summary>
    /// Model for variable tree grid, that provides UI customization of <see cref="DebugEvaluationResult"/>
    /// </summary>
    internal class EvaluationWrapper {
        private readonly DebugEvaluationResult _evaluation;

        private static readonly char[] NameTrimChars = new char[] { '$' };
        private static readonly string HiddenVariablePrefix = ".";
        private static readonly char[] NewLineDelimiter = new char[] { '\r', '\n' };

        private EvaluationWrapper() { }

        public EvaluationWrapper(DebugEvaluationResult evaluation) {
            _evaluation = evaluation;

            Name = _evaluation.Name.TrimStart(NameTrimChars);

            if (_evaluation is DebugValueEvaluationResult) {
                var valueEvaluation = (DebugValueEvaluationResult)_evaluation;

                Value = GetValue(valueEvaluation);
                ValueDetail = valueEvaluation.Value;
                TypeName = valueEvaluation.TypeName;
                Class = string.Join(",", valueEvaluation.Classes); // TODO: escape ',' in class names
                HasChildren = valueEvaluation.HasChildren;
            }
        }

        private object syncObj = new object();
        private Task<IReadOnlyList<EvaluationWrapper>> _getChildrenTask = null;
        public Task<IReadOnlyList<EvaluationWrapper>> GetChildrenAsync() {
            if (_getChildrenTask == null) {
                lock (syncObj) {
                    if (_getChildrenTask == null) {
                        _getChildrenTask = GetChildrenAsyncInternal();
                    }
                }
            }

            return _getChildrenTask;
        }

        public async Task<IReadOnlyList<EvaluationWrapper>> GetChildrenAsyncInternal() {
            List<EvaluationWrapper> result = null;

            var valueEvaluation = _evaluation as DebugValueEvaluationResult;
            if (valueEvaluation == null) {
                Debug.Assert(false, $"EvaluationWrapper result type is not {typeof(DebugValueEvaluationResult)}");
                return result;
            }

            if (valueEvaluation.HasChildren) {
                await TaskUtilities.SwitchToBackgroundThread();

                var children = await valueEvaluation.GetChildrenAsync(true);    // TODO: consider exception propagation such as OperationCanceledException

                result = new List<EvaluationWrapper>();
                foreach (var child in children) {
                    result.Add(new EvaluationWrapper(child));
                }

                if (valueEvaluation.Length > result.Count) {
                    result.Add(EvaluationWrapper.Ellipsis); // insert
                }
            }

            return result;
        }

        public string Name { get; private set; }

        public string Value { get; private set; }

        public string ValueDetail { get; private set; }

        public string TypeName { get; private set; }

        public string Class { get; private set; }

        public bool HasChildren { get; private set; }

        public bool IsHidden {
            get { return Name.StartsWith(HiddenVariablePrefix); }
        }

        private string FirstLine(string multiLine) {
            int firstLine = multiLine.IndexOfAny(NewLineDelimiter);
            if (firstLine == -1) {
                return multiLine;
            } else {
                return multiLine.Substring(0, firstLine);
            }
        }

        private static Lazy<EvaluationWrapper> _ellipsis = new Lazy<EvaluationWrapper>(() => {
            var instance = new EvaluationWrapper();
            instance.Name = string.Empty;
            instance.Value = "[truncated]";
            instance.HasChildren = false;
            return instance;
        });
        private static EvaluationWrapper Ellipsis {
            get { return _ellipsis.Value; }
        }

        private static string DataFramePrefix = "'data.frame':([^:]+):";
        private string GetValue(DebugValueEvaluationResult v) {
            var value = v.Str;
            if (v.Str != null) {
                Match match = Regex.Match(value, DataFramePrefix);
                if (match.Success) {
                    return match.Groups[1].Value.Trim();
                }
            }
            return value;
        }
    }
}
