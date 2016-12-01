// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Microsoft.UnitTests.Core.XUnit {
    [AttributeUsage(AttributeTargets.Class)]
    public class ThreadTypeAttribute : Attribute {
        public ThreadTypeAttribute(ThreadType threadType) {
            ThreadType = threadType;
        }

        public ThreadType ThreadType { get; set; }
    }
}