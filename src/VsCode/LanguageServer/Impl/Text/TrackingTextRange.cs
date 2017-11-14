// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.Text;

namespace Microsoft.R.LanguageServer.Text {
    internal sealed class TrackingTextRange : ITrackingTextRange {
        private readonly ITextRange _range;

        public T As<T>() where T : class => throw new NotSupportedException();

        public TrackingTextRange(ITextRange range) {
            _range = range;
        }

        public int GetStartPoint(IEditorBufferSnapshot snapshot) => _range.Start;
        public int GetEndPoint(IEditorBufferSnapshot snapshot) => _range.End;
    }
}
