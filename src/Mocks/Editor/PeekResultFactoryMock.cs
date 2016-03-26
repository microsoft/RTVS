// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using NSubstitute;

namespace Microsoft.VisualStudio.Editor.Mocks {
    [ExcludeFromCodeCoverage]
    public sealed class PeekResultFactoryMock {
        public static IPeekResultFactory Create() {
            var factory = Substitute.For<IPeekResultFactory>();
            factory.Create(Arg.Any<IPeekResultDisplayInfo>(), Arg.Any<string>(), Arg.Any<Span>(), Arg.Any<int>(), Arg.Any<bool>()).Returns(
                x => new DocumentPeekResultMock((IPeekResultDisplayInfo)x[0], (string)x[1], (Span)x[2]));
            return factory;
        }
    }
}
