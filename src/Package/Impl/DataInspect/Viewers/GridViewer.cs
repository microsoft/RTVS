// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.R.Debugger;
using Microsoft.VisualStudio.R.Package.Utilities;

namespace Microsoft.VisualStudio.R.Package.DataInspect.Viewers {
    [Export(typeof(IObjectDetailsViewer))]
    internal sealed class GridViewer : IObjectDetailsViewer {
        private readonly static string[] _classes = new string[] { "matrix", "data.frame", "table" };

        #region IObjectDetailsViewer
        public bool IsTable => true;

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

        public Task ViewAsync(EvaluationWrapper evaluation) {
            VariableGridWindowPane pane = ToolWindowUtilities.ShowWindowPane<VariableGridWindowPane>(0, true);
            pane.SetEvaluation(evaluation);
            return Task.CompletedTask;
        }
        #endregion
    }
}
