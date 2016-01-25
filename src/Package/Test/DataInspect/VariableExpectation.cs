using System.Diagnostics.CodeAnalysis;

namespace Microsoft.VisualStudio.R.Package.Test.DataInspect {
    /// <summary>
    /// contains expectation for EvaluationWrapper
    /// </summary>
    [ExcludeFromCodeCoverage]
    internal class VariableExpectation {
        public string Name { get; set; }
        public string Value { get; set; }
        public string Class { get; set; }
        public string TypeName { get; set; }
        public bool HasChildren { get; set; }
        public bool CanShowDetail { get; set; }
    }
}
