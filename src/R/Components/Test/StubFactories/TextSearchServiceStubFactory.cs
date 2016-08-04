// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.Text.Operations;
using NSubstitute;

namespace Microsoft.R.Components.Test.StubFactories {
    [ExcludeFromCodeCoverage]
    public sealed class TextSearchServiceStubFactory {
        public static ITextSearchService2 CreateDefault() {
            return Substitute.For<ITextSearchService2>();
        }
    }
}