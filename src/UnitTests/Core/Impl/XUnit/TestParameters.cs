// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Xunit.Abstractions;

namespace Microsoft.UnitTests.Core.XUnit {
    public class TestParameters {
        public TestParameters(ITestMethod testMethod, IAttributeInfo factAttribute) {
            SkipReason = factAttribute.GetNamedArgument<string>(nameof(TestAttribute.Skip));
            ThreadType = factAttribute.GetNamedArgument<ThreadType>(nameof(TestAttribute.ThreadType));
            ShowWindow = factAttribute.GetNamedArgument<bool>(nameof(TestAttribute.ShowWindow));

            if (ThreadType == ThreadType.Default) {
                var classThreadTypeAttribute = GetClassAttribute<ThreadTypeAttribute>(testMethod);
                if (classThreadTypeAttribute != null) {
                    ThreadType = classThreadTypeAttribute.GetNamedArgument<ThreadType>(nameof(TestAttribute.ThreadType));
                }
            }
        }

        private IAttributeInfo GetClassAttribute<T>(ITestMethod testMethod) 
            => testMethod.TestClass.Class.GetCustomAttributes(typeof(T)).FirstOrDefault();

        public bool ShowWindow { get; }
        public ThreadType ThreadType { get; }
        public string SkipReason { get; }
    }
}