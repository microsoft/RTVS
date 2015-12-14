using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Common.Core;
using Microsoft.R.Debugger;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.R.Package.Utilities;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    /// <summary>
    /// Model for variable tree grid, that provides UI customization of <see cref="DebugEvaluationResult"/>
    /// </summary>
    internal class EvaluationWrapper {
        private readonly DebugEvaluationResult _evaluation;

        private static readonly char[] NameTrimChars = new char[] { '$' };
        private static readonly string HiddenVariablePrefix = ".";
        private static readonly char[] NewLineDelimiter = new char[] { '\r', '\n' };

        private readonly bool _truncateChildren;

        private EvaluationWrapper() { }

        public EvaluationWrapper(DebugEvaluationResult evaluation) : this(evaluation, true) { }

        /// <summary>
        /// Create new instance of <see cref="EvaluationWrapper"/>
        /// </summary>
        /// <param name="evaluation">R session's evaluation result</param>
        /// <param name="truncateChildren">true to truncate children returned by GetChildrenAsync</param>
        public EvaluationWrapper(DebugEvaluationResult evaluation, bool truncateChildren) {
            _evaluation = evaluation;
            _truncateChildren = truncateChildren;

            Name = _evaluation.Name.TrimStart(NameTrimChars);

            if (_evaluation is DebugValueEvaluationResult) {
                var valueEvaluation = (DebugValueEvaluationResult)_evaluation;

                Value = GetValue(valueEvaluation);
                ValueDetail = valueEvaluation.Representation.DPut;
                TypeName = valueEvaluation.TypeName;
                var escaped = valueEvaluation.Classes.Select((x) => x.IndexOf(' ') >= 0 ? "'" + x + "'" : x);
                Class = string.Join(", ", escaped); // TODO: escape ',' in class names
                HasChildren = valueEvaluation.HasChildren;

                CanShowDetail = ComputeDetailAvailability(valueEvaluation);
                if (CanShowDetail) {
                    ShowDetailCommand = new DelegateCommand(
                        ShowVariableGridWindowPane,
                        (o) => CanShowDetail);
                    Dimensions = valueEvaluation.Dim;
                } else {
                    Dimensions = new List<int>();
                }
            }
        }

        private readonly object syncObj = new object();
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

                var fields = (DebugEvaluationResultFields.All & ~DebugEvaluationResultFields.ReprAll) |
                    DebugEvaluationResultFields.Repr | DebugEvaluationResultFields.ReprStr;
                var children = await valueEvaluation.GetChildrenAsync(fields, _truncateChildren ? (int?)20 : null, 100);    // TODO: consider exception propagation such as OperationCanceledException

                result = new List<EvaluationWrapper>();
                foreach (var child in children) {
                    result.Add(new EvaluationWrapper(child));
                }

                if (valueEvaluation.Length > result.Count) {
                    result.Add(EvaluationWrapper.Ellipsis); // insert dummy child to indicate truncation in UI
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

        public IReadOnlyList<int> Dimensions { get; private set; }

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
        public static EvaluationWrapper Ellipsis {
            get { return _ellipsis.Value; }
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

        #region Detail Grid

        public bool CanShowDetail { get; private set; }

        private static string[] detailClasses = new string[] { "matrix", "data.frame", "table" };
        private bool ComputeDetailAvailability(DebugValueEvaluationResult evaluation) {
            if (evaluation.Classes.Any(t => detailClasses.Contains(t))) {
                if (evaluation.Dim != null && evaluation.Dim.Count == 2) {
                    return true;
                }
            }
            return false;
        }

        public ICommand ShowDetailCommand { get; }

        private void ShowVariableGridWindowPane(object parameter) {
            VariableGridWindowPane pane = ToolWindowUtilities.ShowWindowPane<VariableGridWindowPane>(0, true);
            pane.SetEvaluation(this);
        }

        #endregion
    }
}
