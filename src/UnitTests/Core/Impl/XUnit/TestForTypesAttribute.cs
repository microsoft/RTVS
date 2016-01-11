using System;
using System.Diagnostics.CodeAnalysis;
using Xunit;
using Xunit.Sdk;

namespace Microsoft.UnitTests.Core.XUnit
{
    [ExcludeFromCodeCoverage]
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