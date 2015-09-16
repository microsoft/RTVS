using System;
using Xunit;
using Xunit.Sdk;

namespace Microsoft.UnitTests.Core.XUnit
{
    [XunitTestCaseDiscoverer("Microsoft.UnitTests.Core.XUnit.TestForTypesDiscoverer", "Microsoft.UnitTests.Core")]
    [TraitDiscoverer("Microsoft.UnitTests.Core.XUnit.UnitTestTraitDiscoverer", "Microsoft.UnitTests.Core")]
    [AttributeUsage(AttributeTargets.Method)]
    public class TestForTypesAttribute : FactAttribute, ITraitAttribute
    {
        public TestForTypesAttribute(params Type[] types)
        {
            Types = types;
        }

        public ThreadType ThreadType { get; set; }
        public Type[] Types { get; set; }
    }
}