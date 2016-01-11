using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Xunit.Abstractions;

namespace Microsoft.UnitTests.Core.XUnit {
    [ExcludeFromCodeCoverage]
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class AssemblyFixtureAttribute : Attribute {
        public static IList<ITypeInfo> GetFixtureTypes(IAssemblyInfo assemblyInfo, IMessageSink diagnosticMessageSink) {
            return assemblyInfo.GetTypes(false)
                .Where(t => t.GetCustomAttributes(typeof (AssemblyFixtureAttribute).AssemblyQualifiedName).Any())
                .ToList();
        }
    }
}