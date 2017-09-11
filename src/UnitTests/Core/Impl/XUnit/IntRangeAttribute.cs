// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using Xunit.Sdk;

namespace Microsoft.UnitTests.Core.XUnit {
    [DataDiscoverer("Microsoft.UnitTests.Core.XUnit.IntRangeDiscoverer", "Microsoft.UnitTests.Core")]
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public sealed class IntRangeAttribute : DataAttribute {
        private readonly int _start;
        private readonly int _count;
        private readonly int _step;

        public IntRangeAttribute(int count) : this(0, count, 1) {}

        public IntRangeAttribute(int start, int count) : this(start, count, 1) {}

        public IntRangeAttribute(int start, int count, int step) {
            _start = start;
            _count = count;
            _step = step;
        }

        public override IEnumerable<object[]> GetData(MethodInfo testMethod) {
            for (var i = 0; i < _count; i++) {
                yield return new object[] { _start + i * _step };
            }
        }
    }
}