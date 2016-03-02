// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Projection;

namespace Microsoft.VisualStudio.Editor.Mocks
{
    [ExcludeFromCodeCoverage]
    public sealed class MappingPointMock : IMappingPoint
    {
        private int _position;

        public MappingPointMock(ITextBuffer textBuffer, int position)
        {
            AnchorBuffer = textBuffer;
            _position = position;
        }
        public ITextBuffer AnchorBuffer { get; private set; }

        public IBufferGraph BufferGraph
        {
            get { return null; }
        }

        public SnapshotPoint? GetInsertionPoint(Predicate<ITextBuffer> match)
        {
            return new SnapshotPoint(AnchorBuffer.CurrentSnapshot, _position);
        }

        public SnapshotPoint? GetPoint(Predicate<ITextBuffer> match, PositionAffinity affinity)
        {
            return new SnapshotPoint(AnchorBuffer.CurrentSnapshot, _position);
        }

        public SnapshotPoint? GetPoint(ITextSnapshot targetSnapshot, PositionAffinity affinity)
        {
            return new SnapshotPoint(targetSnapshot, _position);
        }

        public SnapshotPoint? GetPoint(ITextBuffer targetBuffer, PositionAffinity affinity)
        {
            return new SnapshotPoint(targetBuffer.CurrentSnapshot, _position);
        }
    }
}
