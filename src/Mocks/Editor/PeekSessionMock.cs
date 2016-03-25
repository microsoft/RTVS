// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using NSubstitute;

namespace Microsoft.VisualStudio.Editor.Mocks {
    [ExcludeFromCodeCoverage]
    public static class PeekSessionMock  {
        public static IPeekSession Create(ITextView tv, int triggerPoint) {
            var session = Substitute.For<IPeekSession>();
            session.IsDismissed.Returns(false);
            session.Properties.Returns(new PropertyCollection());
            session.GetTriggerPoint(Arg.Any<ITextSnapshot>()).Returns(new SnapshotPoint(tv.TextBuffer.CurrentSnapshot, triggerPoint));
            session.TextView.Returns(tv);
            return session;
        }
    }
}
