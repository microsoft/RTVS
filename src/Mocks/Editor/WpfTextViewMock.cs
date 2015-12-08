using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Media;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;

namespace Microsoft.VisualStudio.Editor.Mocks {
    [ExcludeFromCodeCoverage]
    public sealed class WpfTextViewMock : TextViewMock, IWpfTextView {

        public WpfTextViewMock(ITextBuffer textBuffer) : base(textBuffer) {
        }

        public Brush Background { get; set; }

        public IFormattedLineSource FormattedLineSource {
            get {
                throw new NotImplementedException();
            }
        }

        public ILineTransformSource LineTransformSource {
            get {
                throw new NotImplementedException();
            }
        }

        public System.Windows.FrameworkElement VisualElement {
            get {
                throw new NotImplementedException();
            }
        }

        public double ZoomLevel { get; set; } = 1.0;

        IWpfTextViewLineCollection IWpfTextView.TextViewLines {
            get {
                throw new NotImplementedException();
            }
        }

#pragma warning disable 67
        public event EventHandler<BackgroundBrushChangedEventArgs> BackgroundBrushChanged;
        public event EventHandler<ZoomLevelChangedEventArgs> ZoomLevelChanged;

        public IAdornmentLayer GetAdornmentLayer(string name) {
            return new AdornmentLayerMock();
        }

        public ISpaceReservationManager GetSpaceReservationManager(string name) {
            throw new NotImplementedException();
        }

        IWpfTextViewLine IWpfTextView.GetTextViewLineContainingBufferPosition(SnapshotPoint bufferPosition) {
            throw new NotImplementedException();
        }
    }
}
