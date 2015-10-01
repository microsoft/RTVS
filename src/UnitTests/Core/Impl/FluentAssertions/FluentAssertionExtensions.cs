using System;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using FluentAssertions.Specialized;

namespace Microsoft.UnitTests.Core.FluentAssertions
{
    public static class FluentAssertionExtensions
    {
        private static readonly MethodInfo ShouldThrowActionMethod;

        static FluentAssertionExtensions()
        {
            ShouldThrowActionMethod = typeof(AssertionExtensions).GetMethods().First(
                mi => mi.Name.Equals(nameof(AssertionExtensions.ShouldThrow)) && mi.GetParameters().First().ParameterType == typeof(Action));
        }

        public static void ShouldThrow(this Action action, Type exceptionType, string because = "", params object[] reasonArgs)
        {
            ShouldThrowActionMethod.MakeGenericMethod(exceptionType).Invoke(null, new object[] { action, because, reasonArgs });
        }
    }
}
