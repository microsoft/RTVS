using System;
using System.Reflection;

namespace Microsoft.UnitTests.Core.XUnit
{
    public abstract class BeforeCtorAfterDisposeAttribute : Attribute
    {
        public abstract void After(MethodInfo methodUnderTest);
        public abstract void Before(MethodInfo methodUnderTest);
    }
}
