using Microsoft.R.Editor.Signatures;
using Microsoft.R.Editor.Test.Assertions;

namespace Microsoft.R.Editor.Test {
    internal static class AssertionExtensions {
        public static ParameterInfoAssertion Should(this ParameterInfo parameterInfo) {
            return new ParameterInfoAssertion(parameterInfo);
        }
    }
}