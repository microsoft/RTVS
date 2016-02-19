using System.ComponentModel.Design;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.R.Package.Test.Assertions;

namespace Microsoft.VisualStudio.R.Package.Test {
    [ExcludeFromCodeCoverage]
    public static class AssertionExtensions {
        public static MenuCommandAssertions Should(this MenuCommand command) {
            return new MenuCommandAssertions(command);
        }
    }
}