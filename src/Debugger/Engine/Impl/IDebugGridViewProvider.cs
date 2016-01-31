using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.R.Debugger.Engine {
    public interface IDebugGridViewProvider {
        bool CanShowDataGrid(DebugEvaluationResult evaluationResult);
        void ShowDataGrid(DebugEvaluationResult evaluationResult);
    }
}
