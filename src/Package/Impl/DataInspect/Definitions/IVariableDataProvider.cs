using System.Threading.Tasks;

namespace Microsoft.VisualStudio.R.Package.DataInspect.Definitions {
    internal interface IVariableDataProvider {
        EvaluationWrapper GlobalEnvironment { get; }
        Task<IGridData<string>> GetGridDataAsync(string expression, GridRange gridRange);
    }
}
