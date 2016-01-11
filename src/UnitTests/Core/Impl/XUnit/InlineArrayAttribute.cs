using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Xunit.Sdk;

namespace Microsoft.UnitTests.Core.XUnit
{
    [DataDiscoverer("Microsoft.UnitTests.Core.XUnit.InlineArrayDiscoverer", "Microsoft.UnitTests.Core")]
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    [ExcludeFromCodeCoverage]
    public sealed class InlineArrayAttribute : DataAttribute
    {
        private readonly object[] _array;

        public InlineArrayAttribute(params object[] array)
        {
            _array = array;
        }

        public override IEnumerable<object[]> GetData(MethodInfo testMethod)
        {
            yield return new object[] { _array };
        }
    }
}