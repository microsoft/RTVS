using System;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    internal class VariableChangedArgs : EventArgs {
        public EvaluationWrapper NewVariable { get; set; }
    }
}