// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

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