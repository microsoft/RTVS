using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.R.Debugger;
using Microsoft.R.Debugger.Engine;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    [Export(typeof(IDebugGridViewProvider))]
    internal class DebugGridViewProvider : IDebugGridViewProvider {
        public bool CanShowDataGrid(DebugEvaluationResult evaluationResult) {
            var wrapper = new EvaluationWrapper(evaluationResult);
            return wrapper.CanShowDetail;
        }

        public void ShowDataGrid(DebugEvaluationResult evaluationResult) {
            var wrapper = new EvaluationWrapper(evaluationResult);
            if (!wrapper.CanShowDetail) {
                throw new InvalidOperationException("Cannot show data grid on evaluation result " + evaluationResult);
            }
            wrapper.ShowDetailCommand.Execute(null);
        }
    }
}
