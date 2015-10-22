using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.R.Debugger;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    /// <summary>
    /// Model for variable tree grid, that provides UI customization of <see cref="DebugEvaluationResult"/>
    /// </summary>
    internal class EvaluationWrapper {
        private readonly DebugEvaluationResult _evaluation;

        private readonly char[] NameTrimChars = new char[] { '$' };
        private readonly string HiddenVariablePrefix = ".";

        public EvaluationWrapper(DebugEvaluationResult evaluation) {
            _evaluation = evaluation;

            Name = _evaluation.Name.TrimStart(NameTrimChars);

            if (_evaluation is DebugValueEvaluationResult) {
                var valueEvaluation = (DebugValueEvaluationResult)_evaluation;

                Value = FirstLine(valueEvaluation.Value);
                ValueDetail = valueEvaluation.Value;

                Type = valueEvaluation.TypeName;
                Class = string.Join(",", valueEvaluation.Classes); // TODO: espace ',' in class names
            }
        }

        public async Task<IList<EvaluationWrapper>> GetChildrenAsync() {
            List<EvaluationWrapper> result = null;

            var valueEvaluation = _evaluation as DebugValueEvaluationResult;
            if (valueEvaluation == null) {
                Debug.Assert(false, $"EvaluationWrapper has non {typeof(DebugValueEvaluationResult)} result");
                return result;
            }

            if (valueEvaluation.HasChildren) {
                var children = await valueEvaluation.GetChildrenAsync();

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

        public string Type { get; private set; }

        public string Class { get; private set; }

        public bool IsHidden {
            get { return Name.StartsWith(HiddenVariablePrefix); }
        }

        public bool IsSameEvaluation(EvaluationWrapper value) {
            return Name == value.Name;
        }

        /// <param name="multiLine">multiline string delimited by new line (\n)</param>
        private string FirstLine(string multiLine) {
            int firstLine = multiLine.IndexOf('\n');
            if (firstLine == -1) {
                return multiLine;
            } else {
                return multiLine.Substring(0, firstLine);
            }
        }
    }
}
