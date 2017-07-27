// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using Microsoft.Common.Core;
using Microsoft.Common.Wpf.Extensions;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    internal abstract class MatrixViewItemAutomationPeer : AutomationPeer {
        private string _value;

        protected bool IsRealized { get; private set; }
        protected MatrixView Owner { get; }
        protected GridPoints Points => Owner.Points;

        protected MatrixViewItemAutomationPeer(MatrixView owner) {
            Owner = owner;
        }

        public void SetValue(string value) {
            if (_value.EqualsOrdinal(value)) {
                return;
            }

            var oldValue = _value;
            _value = value;
            IsRealized = true;
            RaisePropertyChangedEvent(ValuePatternIdentifiers.ValueProperty, oldValue, value);
        }

        public void ClearValue() {
            _value = null;
            IsRealized = false;
        }

        protected override string GetAcceleratorKeyCore() => string.Empty;
        protected override string GetAccessKeyCore() => string.Empty;
        protected override List<AutomationPeer> GetChildrenCore() => null;
        protected override string GetHelpTextCore() => string.Empty;
        protected override string GetItemStatusCore() => string.Empty;
        protected override string GetItemTypeCore() => string.Empty;
        protected override AutomationPeer GetLabeledByCore() => null;
        protected override string GetNameCore() => _value;
        protected override bool IsControlElementCore() => true;
        protected override bool IsEnabledCore() => Owner.IsEnabled;
        protected override bool IsPasswordCore() => false;
        protected override bool IsRequiredForFormCore() => false;

        protected override AutomationOrientation GetOrientationCore() => AutomationOrientation.None;
        
        protected override Point GetClickablePointCore() {
            var visibleRect = GetVisibleRect();
            return visibleRect.Height.IsCloseTo(0) || visibleRect.Width.IsCloseTo(0)
                ? new Point(double.NaN, double.NaN)
                : Owner.PointToScreen(new Point(visibleRect.Left + visibleRect.Width * 0.5, visibleRect.Top + visibleRect.Height * 0.5));
        }

        protected override bool IsOffscreenCore() {
            var visibleRect = GetVisibleRect();
            return visibleRect.Height.IsCloseTo(0) || visibleRect.Width.IsCloseTo(0);
        }

        protected abstract Rect GetVisibleRect();
    }
}