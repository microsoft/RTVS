using System.Diagnostics.CodeAnalysis;
using Xunit.Abstractions;

namespace Microsoft.UnitTests.Core.XUnit
{
    [ExcludeFromCodeCoverage]
    public class TestParameters
    {
        public TestParameters(IAttributeInfo factAttribute)
        {
            SkipReason = factAttribute.GetNamedArgument<string>("Skip");
            ThreadType = factAttribute.GetNamedArgument<ThreadType>("ThreadType");
        }

        public ThreadType ThreadType { get; }
        public string SkipReason { get; }
    }
}