// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Reflection;

namespace Microsoft.UnitTests.Core.XUnit
{
    public abstract class BeforeCtorAfterDisposeAttribute : Attribute
    {
        public abstract void After(MethodInfo methodUnderTest);
        public abstract void Before(MethodInfo methodUnderTest);
    }
}
