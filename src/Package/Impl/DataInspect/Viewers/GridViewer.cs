// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.R.Components.Extensions;
using Microsoft.R.Debugger;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Package.Utilities;

namespace Microsoft.VisualStudio.R.Package.DataInspect.Viewers {
    [Export(typeof(IObjectDetailsViewer))]
    internal sealed class GridViewer : ViewerBase, IObjectDetailsViewer {
        private const int _toolWindowIdBase = 100;
        private readonly static string[] _tableClasses = new string[] { "matrix", "data.frame", "table" };
        private readonly static string[] _listClasses = new string[] { "list", "ts" };
        private const DebugEvaluationResultFields _fields = DebugEvaluationResultFields.Classes
                                                        | DebugEvaluationResultFields.Expression
                                                        | DebugEvaluationResultFields.TypeName
                                                        | DebugEvaluationResultFields.Dim
                                                        | DebugEvaluationResultFields.Length;

        private readonly IObjectDetailsViewerAggregator _aggregator;

        [ImportingConstructor]
        public GridViewer(IObjectDetailsViewerAggregator aggregator, IDataObjectEvaluator evaluator) :
            base(evaluator) {
            _aggregator = aggregator;
        }

        #region IObjectDetailsViewer
        public ViewerCapabilities Capabilities => ViewerCapabilities.List | ViewerCapabilities.Table;

        public bool CanView(DebugValueEvaluationResult evaluation) {
            if (evaluation != null) {
                if (evaluation.Classes.Any(t => _tableClasses.Contains(t))) {
                    return evaluation.Dim != null && evaluation.Dim.Count > 0 && evaluation.Dim.Count <= 2;
                } else if (evaluation.Classes.Any(t => _listClasses.Contains(t))) {
                    return evaluation.Dim == null && evaluation.Classes.Count == 1;
                }
                return evaluation.Length > 1;
            }
            return false;
        }

        public async Task ViewAsync(string expression, string title) {
            var evaluation = await EvaluateAsync(expression, _fields, "rtvs:::make_repr_str()") as DebugValueEvaluationResult;
            if (evaluation != null) {
                await VsAppShell.Current.SwitchToMainThreadAsync();

                var id = _toolWindowIdBase + evaluation.GetHashCode() % (Int32.MaxValue - _toolWindowIdBase);
                VariableGridWindowPane pane = ToolWindowUtilities.ShowWindowPane<VariableGridWindowPane>(id, true);
                title = !string.IsNullOrEmpty(title) ? title : evaluation.Expression;
                pane.SetEvaluation(new VariableViewModel(evaluation, _aggregator), title);
            }
        }
        #endregion
    }
}
