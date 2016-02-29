// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

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