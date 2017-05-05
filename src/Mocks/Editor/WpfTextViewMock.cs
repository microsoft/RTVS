// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Media;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;

namespace Microsoft.VisualStudio.Editor.Mocks {
    [ExcludeFromCodeCoverage]
    public sealed class WpfTextViewMock : TextViewMock, IWpfTextView {

        public WpfTextViewMock(ITextBuffer textBuffer) : base(textBuffer) { }

        public Brush Background { get; set; }

        public IFormattedLineSource FormattedLineSource => throw new NotImplementedException();
        public ILineTransformSource LineTransformSource => throw new NotImplementedException();
        public System.Windows.FrameworkElement VisualElement => null;
        public double ZoomLevel { get; set; } = 1.0;

        IWpfTextViewLineCollection IWpfTextView.TextViewLines => throw new NotImplementedException();

#pragma warning disable 67
        public event EventHandler<BackgroundBrushChangedEventArgs> BackgroundBrushChanged;
        public event EventHandler<ZoomLevelChangedEventArgs> ZoomLevelChanged;

        public IAdornmentLayer GetAdornmentLayer(string name) => new AdornmentLayerMock();
        public ISpaceReservationManager GetSpaceReservationManager(string name) => throw new NotImplementedException();
        IWpfTextViewLine IWpfTextView.GetTextViewLineContainingBufferPosition(SnapshotPoint bufferPosition) => throw new NotImplementedException();
    }
}
