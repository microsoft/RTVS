using System;
using System.Threading.Tasks;

namespace Microsoft.VisualStudio.R.Package.DataInspect.Definitions {
    internal interface IVariableDataProvider : IDisposable {
        event EventHandler<VariableChangedArgs> VariableChanged;
        EvaluationWrapper LastEvaluation { get; }
        Task<IGridData<string>> GetGridDataAsync(string expression, GridRange gridRange);
    }
}
