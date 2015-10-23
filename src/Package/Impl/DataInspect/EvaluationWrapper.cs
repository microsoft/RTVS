using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
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

        public EvaluationWrapper(DebugEvaluationResult evaluation) {
            _evaluation = evaluation;

            Name = _evaluation.Name.TrimStart(NameTrimChars);

            if (_evaluation is DebugValueEvaluationResult) {
                var valueEvaluation = (DebugValueEvaluationResult)_evaluation;

                Value = valueEvaluation.Str;// FirstLine(valueEvaluation.Value);   // TODO: it takes first line only for now. Visual representation will be tuned up later e.g. R str or custom formatting
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
    }
}
