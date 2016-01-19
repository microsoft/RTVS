using System;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    internal class PointChangedEventArgs : EventArgs {
        public PointChangedEventArgs(ScrollDirection direction) {
            Direction = direction;
        }

        public ScrollDirection Direction { get; }
    }
}
