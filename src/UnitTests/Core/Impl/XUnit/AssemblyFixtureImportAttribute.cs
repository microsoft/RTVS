// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Microsoft.UnitTests.Core.XUnit {
    [AttributeUsage(AttributeTargets.Assembly)]
    public sealed class AssemblyFixtureImportAttribute : Attribute {
        public Type[] Types { get; }

        public AssemblyFixtureImportAttribute(params Type[] types) {
            Types = types;
        }
    }
}