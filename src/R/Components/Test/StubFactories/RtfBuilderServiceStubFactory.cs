// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.Text.Formatting;
using NSubstitute;

namespace Microsoft.R.Components.Test.StubFactories {
    [ExcludeFromCodeCoverage]
    public sealed class RtfBuilderServiceStubFactory {
        public static IRtfBuilderService CreateDefault() {
            return Substitute.For<IRtfBuilderService>();
        }
    }
}