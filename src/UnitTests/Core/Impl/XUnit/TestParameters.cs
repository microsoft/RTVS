// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Xunit.Abstractions;

namespace Microsoft.UnitTests.Core.XUnit {
    [ExcludeFromCodeCoverage]
    public class TestParameters {
        public TestParameters(IAttributeInfo factAttribute) {
            SkipReason = factAttribute.GetNamedArgument<string>(nameof(TestAttribute.Skip));
            ThreadType = factAttribute.GetNamedArgument<ThreadType>(nameof(TestAttribute.ThreadType));
            ShowWindow = factAttribute.GetNamedArgument<bool>(nameof(TestAttribute.ShowWindow));
        }

        public bool ShowWindow { get; }
        public ThreadType ThreadType { get; }
        public string SkipReason { get; }
    }
}