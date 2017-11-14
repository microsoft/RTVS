// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using Xunit;
using Xunit.Sdk;

namespace Microsoft.UnitTests.Core.XUnit {
    [XunitTestCaseDiscoverer("Microsoft.UnitTests.Core.XUnit.CompositeTestDiscoverer", "Microsoft.UnitTests.Core")]
    [AttributeUsage(AttributeTargets.Method)]
    public class CompositeTestAttribute : FactAttribute, ITraitAttribute {
        public CompositeTestAttribute(ThreadType threadType = ThreadType.Default, bool showWindow = false) {
            ThreadType = threadType;
            ShowWindow = showWindow;
        }

        public CompositeTestAttribute(bool showWindow) {
            ThreadType = ThreadType.Default;
            ShowWindow = showWindow;
        }

        public ThreadType ThreadType { get; set; }
        public bool ShowWindow { get; set; }
    }
}
