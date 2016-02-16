using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.UnitTests.Core.XUnit {
    [ExcludeFromCodeCoverage]
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class AssemblyFixtureAttribute : Attribute {}
}