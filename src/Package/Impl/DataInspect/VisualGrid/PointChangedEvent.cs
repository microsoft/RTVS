using System;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    internal class PointChangedEvent : EventArgs {
        public PointChangedEvent(ScrollDirection direction) {
            Direction = direction;
        }

        public ScrollDirection Direction { get; }
    }
}
