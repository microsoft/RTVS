// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Windows.Automation;
using System.Windows.Automation.Provider;
using Microsoft.Common.Wpf.Automation;
using Microsoft.Common.Wpf.Extensions;
using static System.Windows.Automation.ScrollPatternIdentifiers;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    internal sealed class MatrixViewAutomationPeerScrollProvider : AutomationPropertyChangedBase, IScrollProvider {
        private readonly MatrixView _owner;
        private double _horizontalScrollPercent;
        private double _verticalScrollPercent;
        private double _horizontalViewSize;
        private double _verticalViewSize;
        private bool _horizontallyScrollable;
        private bool _verticallyScrollable;

        private VisualGridScroller Scroller => _owner.Scroller;

        public double HorizontalScrollPercent {
            get => _horizontalScrollPercent;
            private set => SetProperty(ref _horizontalScrollPercent, value, HorizontalScrollPercentProperty);
        }

        public double VerticalScrollPercent {
            get => _verticalScrollPercent;
            private set => SetProperty(ref _verticalScrollPercent, value, VerticalScrollPercentProperty);
        }

        public double HorizontalViewSize {
            get => _horizontalViewSize;
            private set => SetProperty(ref _horizontalViewSize, value, HorizontalViewSizeProperty);
        }

        public double VerticalViewSize {
            get => _verticalViewSize;
            private set => SetProperty(ref _verticalViewSize, value, VerticalViewSizeProperty);
        }

        public bool HorizontallyScrollable {
            get => _horizontallyScrollable;
            private set => SetProperty(ref _horizontallyScrollable, value, HorizontallyScrollableProperty);
        }

        public bool VerticallyScrollable {
            get => _verticallyScrollable;
            private set => SetProperty(ref _verticallyScrollable, value, VerticallyScrollableProperty);
        }

        public MatrixViewAutomationPeerScrollProvider(MatrixView owner) : base(owner) {
            _owner = owner;
        }

        public void UpdateValues() {
            var points = _owner.Points;
            HorizontallyScrollable = points.HorizontalExtent.GreaterThan(points.ViewportWidth);
            VerticallyScrollable = points.VerticalExtent.GreaterThan(points.ViewportHeight);
            HorizontalViewSize = HorizontallyScrollable ? 100 * points.ViewportWidth / points.HorizontalExtent : 100;
            VerticalViewSize = VerticallyScrollable ? 100 * points.ViewportHeight / points.VerticalExtent : 100;
            HorizontalScrollPercent = HorizontallyScrollable
                ? 100 * points.HorizontalOffset / (points.HorizontalExtent - points.ViewportWidth)
                : NoScroll;
            VerticalScrollPercent = VerticallyScrollable
                ? 100 * points.VerticalOffset / (points.VerticalExtent - points.ViewportHeight)
                : NoScroll;
        }

        public void Scroll(ScrollAmount horizontalAmount, ScrollAmount verticalAmount) {
            switch (horizontalAmount) {
                case ScrollAmount.LargeDecrement:
                    Scroller.EnqueueCommand(GridUpdateType.PageLeft, 1);
                    break;
                case ScrollAmount.SmallDecrement:
                    _owner.Scroller.EnqueueCommand(GridUpdateType.LineLeft, 1);
                    break;
                case ScrollAmount.SmallIncrement:
                    _owner.Scroller.EnqueueCommand(GridUpdateType.LineRight, 1);
                    break;
                case ScrollAmount.LargeIncrement:
                    _owner.Scroller.EnqueueCommand(GridUpdateType.PageRight, 1);
                    break;
            }

            switch (verticalAmount) {
                case ScrollAmount.LargeDecrement:
                    _owner.Scroller.EnqueueCommand(GridUpdateType.PageUp, 1);
                    break;
                case ScrollAmount.SmallDecrement:
                    _owner.Scroller.EnqueueCommand(GridUpdateType.LineUp, 1);
                    break;
                case ScrollAmount.SmallIncrement:
                    _owner.Scroller.EnqueueCommand(GridUpdateType.LineDown, 1);
                    break;
                case ScrollAmount.LargeIncrement:
                    _owner.Scroller.EnqueueCommand(GridUpdateType.PageDown, 1);
                    break;
            }
        }

        public void SetScrollPercent(double horizontalPercent, double verticalPercent) {
            _owner.Scroller.EnqueueCommand(GridUpdateType.SetHorizontalOffset, (horizontalPercent / 100, ThumbTrack.None));
            _owner.Scroller.EnqueueCommand(GridUpdateType.SetVerticalOffset, (verticalPercent / 100, ThumbTrack.None));
        }
    }
}