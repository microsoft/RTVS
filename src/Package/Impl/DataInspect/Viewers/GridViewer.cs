// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.R.Debugger;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Package.Utilities;
using static System.FormattableString;

namespace Microsoft.VisualStudio.R.Package.DataInspect.Viewers {
    [Export(typeof(IObjectDetailsViewer))]
    internal sealed class GridViewer : IObjectDetailsViewer {
        private const int _toolWindowIdBase = 100; 
        private readonly static string[] _classes = new string[] { "matrix", "data.frame", "table" };
        private const DebugEvaluationResultFields _fields = DebugEvaluationResultFields.Classes
                                                        | DebugEvaluationResultFields.Expression
                                                        | DebugEvaluationResultFields.TypeName
                                                        | (DebugEvaluationResultFields.Repr | DebugEvaluationResultFields.ReprStr)
                                                        | DebugEvaluationResultFields.Dim
                                                        | DebugEvaluationResultFields.Length;

        #region IObjectDetailsViewer
        public bool IsTable => true;

        public DebugEvaluationResultFields EvaluationFields => _fields;

        public bool CanView(DebugValueEvaluationResult evaluation) {
            if (evaluation != null) {
                if (evaluation.Classes.Any(t => _classes.Contains(t))) {
                    if (evaluation.Dim != null && evaluation.Dim.Count == 2) {
                        return true;
                    }
                }
            }
            return false;
        }

        public Task ViewAsync(DebugValueEvaluationResult evaluation, string title) {
            VsAppShell.Current.DispatchOnUIThread(() => {
                var id = _toolWindowIdBase + evaluation.GetHashCode() % (Int32.MaxValue - _toolWindowIdBase);
                VariableGridWindowPane pane = ToolWindowUtilities.ShowWindowPane<VariableGridWindowPane>(id, true);
                pane.SetEvaluation(new VariableViewModel(evaluation), evaluation.Expression ?? title);
            });
            return Task.CompletedTask;
        }

        public Task<object> GetTooltipAsync(DebugValueEvaluationResult evaluation) {
            string tooltip = null;
            if (CanView(evaluation)) {
                var className = evaluation.Classes.FirstOrDefault(t => _classes.Contains(t));
                if (!string.IsNullOrEmpty(className))
                    tooltip = Invariant($"{className} {evaluation.Dim[0]}x{evaluation.Dim[1]}");
            }
            return Task.FromResult<object>(tooltip);
        }
        #endregion
    }
}
