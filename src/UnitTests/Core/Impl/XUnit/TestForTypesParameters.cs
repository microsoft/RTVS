using System;
using System.Diagnostics.CodeAnalysis;
using Xunit.Abstractions;

namespace Microsoft.UnitTests.Core.XUnit
{
    [ExcludeFromCodeCoverage]
    public class TestForTypesParameters : TestParameters
    {
        public TestForTypesParameters(IAttributeInfo factAttribute) : base(factAttribute)
        {
            Types = factAttribute.GetNamedArgument<Type[]>("Types");
        }

        public Type[] Types { get; set; }
    }
}